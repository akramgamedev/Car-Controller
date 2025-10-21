using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineCarController : MonoBehaviour
{
    [Header("Spline Settings")]
    public SplineContainer splineContainer;
    public bool loopSpline = false;
    private float splineProgress = 0f;
    private bool reachedEnd = false;

    [Header("Trail Color Settings")]
    public Color trailColor = Color.black;       // Base color of the trail
    [Range(0f, 1f)]
    public float trailAlpha = 0.9f;             // How opaque the trail is


    [Header("Movement Settings")]
    public float maxSpeed = 15f;
    public float acceleration = 7f;
    public float deceleration = 10f;
    private float currentSpeed = 0f;

    [Header("Low-Speed Behaviour")]
    public float rotationStopSpeed = 4f;

    [Header("Drift Settings (PICK ME UP 3D STYLE)")]
    public float minDriftSpeed = 10f;
    public float maxDriftAngle = 60f;
    public float turnSensitivity = 2.2f;
    [Tooltip("Controls how speed affects drift intensity")]
    public float driftSpeedCurve = 2.0f;
    [Tooltip("How quickly drift angle changes")]
    public float driftSmoothTime = 0.08f;
    [Tooltip("Initial snap when entering drift")]
    public float overshootFactor = 0.7f;

    private float currentDriftAngle = 0f;
    private float driftVelocity = 0f;
    private float targetDriftAngle = 0f;
    private bool isInTurn = false;

    [Header("Turn Detection")]
    [Tooltip("Lookahead for drift detection - smaller = drifts AT the turn")]
    public float lookAheadDistance = 0.008f;
    public float rotationSpeed = 30f;
    [Tooltip("Lookahead for rotation - very small = rotates AT turn, not before")]
    public float rotationLookAhead = 0.002f;

    [Header("Visual Settings")]
    public Transform carChild;
    public ParticleSystem driftParticles;

    [Header("Drift Trail Settings")]
    [Tooltip("Trail Renderers for left and right rear wheels")]
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;
    [Tooltip("Distance between rear wheels")]
    public float wheelDistance = 1.5f;
    [Tooltip("How far back from center are rear wheels")]
    public float rearWheelOffset = 1.2f;
    [Tooltip("Minimum drift angle to start trails")]
    public float trailStartAngle = 12f;
    [Tooltip("Base width of drift trails")]
    public float trailWidth = 0.3f;
    [Tooltip("End width of trail (fade out)")]
    public float trailEndWidth = 0.05f;
    [Tooltip("Multiply width based on drift intensity")]
    public bool dynamicWidth = true;
    [Tooltip("How long trails persist on ground (increased for visible marks)")]
    public float trailLifetime = 5f;

    [Header("Brake Mark Settings")]
    [Tooltip("Minimum speed to trigger brake marks")]
    public float minBrakeSpeed = 12f;
    [Tooltip("Duration at high speed before brake marks can trigger")]
    public float minHighSpeedDuration = 1.5f;

    [Header("Side Drift Effect (Pick Me Up Style)")]
    [Tooltip("Maximum sideways offset during drift")]
    public float maxSideDriftOffset = 3.0f;
    [Tooltip("How quickly car slides sideways")]
    public float sideDriftSpeed = 0.25f;
    [Tooltip("Quick snap back to center after drift")]
    public float centerReturnSpeed = 0.12f;
    [Tooltip("How much the FRONT leads vs BACK swings (0=front pivots, 1=whole car rotates)")]
    [Range(0f, 1f)]
    public float frontPivotRatio = 0.3f;

    private Rigidbody rb;
    private float totalSplineLength;
    private bool isTouching = false;
    private float sideDriftOffset = 0f;
    private float sideDriftVelocity = 0f;
    private Quaternion baseRotation;

    private GameObject leftTrailObject;
    private GameObject rightTrailObject;
    private float previousSpeed = 0f;
    private float highSpeedTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (splineContainer == null)
        {
            Debug.LogError("Spline Container not assigned!");
            return;
        }

        if (carChild == null && transform.childCount > 0)
        {
            carChild = transform.GetChild(0);
            Debug.Log($"Auto-assigned car child: {carChild.name}");
        }

        totalSplineLength = splineContainer.Spline.GetLength();

        Vector3 startPos = splineContainer.EvaluatePosition(0f);
        transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

        Vector3 startTangent = splineContainer.EvaluateTangent(0f);
        startTangent.y = 0;
        startTangent.Normalize();
        baseRotation = Quaternion.LookRotation(startTangent);
        transform.rotation = baseRotation;

        carChild.localRotation = Quaternion.identity;

        SetupTrailRenderers();
    }

    void SetupTrailRenderers()
    {
        if (leftTrail == null)
        {
            leftTrailObject = new GameObject("LeftDriftTrail");
            leftTrailObject.transform.SetParent(transform);
            leftTrail = leftTrailObject.AddComponent<TrailRenderer>();
        }

        if (rightTrail == null)
        {
            rightTrailObject = new GameObject("RightDriftTrail");
            rightTrailObject.transform.SetParent(transform);
            rightTrail = rightTrailObject.AddComponent<TrailRenderer>();
        }

        ConfigureTrail(leftTrail);
        ConfigureTrail(rightTrail);

        leftTrail.emitting = false;
        rightTrail.emitting = false;
    }

    void ConfigureTrail(TrailRenderer trail)
    {
        trail.time = trailLifetime;

        // Base width values
        trail.startWidth = trailWidth;
        trail.endWidth = trailEndWidth;

        // Smooth fade in/out for opacity
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(trailColor, 0.0f),
            new GradientColorKey(trailColor, 0.5f),
            new GradientColorKey(trailColor, 1.0f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(0.0f, 0.0f),
            new GradientAlphaKey(trailAlpha, 0.1f),
            new GradientAlphaKey(trailAlpha, 0.9f),
            new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;
        trail.material.color = trailColor;

        // ✅ Smooth width growth over lifetime (stays consistent while being made)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);     // start normal
        widthCurve.AddKey(1f, 1f); // mid lifetime slightly thicker

        trail.widthCurve = widthCurve;

        // Material and visual settings
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = Color.black;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleDrift();
        HandleDriftParticles();
        HandleDriftTrails();
    }

    void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            isTouching = t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved;
        }
        else isTouching = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) isTouching = true;
#endif
    }

    void HandleMovement()
    {
        if (reachedEnd && !loopSpline)
        {
            currentSpeed = 0f;
            return;
        }

        if (isTouching)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2.2f * Time.deltaTime);

        if (currentSpeed < 0.3f && !isTouching)
            currentSpeed = 0f;

        if (currentSpeed <= 0.01f) return;

        float speedOnSpline = currentSpeed / totalSplineLength;
        splineProgress += speedOnSpline * Time.deltaTime;

        if (splineProgress >= 1f)
        {
            if (loopSpline) splineProgress -= 1f;
            else { splineProgress = 1f; reachedEnd = true; currentSpeed = 0f; }
        }

        Vector3 splinePos = splineContainer.EvaluatePosition(splineProgress);

        float lookAhead = Mathf.Clamp01(splineProgress + rotationLookAhead);
        Vector3 currentTangent = splineContainer.EvaluateTangent(splineProgress);
        Vector3 futureTangent = splineContainer.EvaluateTangent(lookAhead);
        currentTangent.y = 0;
        futureTangent.y = 0;
        currentTangent.Normalize();
        futureTangent.Normalize();

        Vector3 targetDirection = Vector3.Lerp(currentTangent, futureTangent, 0.2f).normalized;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDirection);

            if (currentSpeed < rotationStopSpeed)
            {
                float slowRotationSpeed = Mathf.InverseLerp(0f, rotationStopSpeed, currentSpeed) * 2f;
                baseRotation = Quaternion.Slerp(baseRotation, targetRot, Time.deltaTime * slowRotationSpeed);
            }
            else
            {
                float speedBasedRotation = rotationSpeed * Mathf.InverseLerp(rotationStopSpeed, maxSpeed, currentSpeed);
                baseRotation = Quaternion.Slerp(baseRotation, targetRot, Time.deltaTime * speedBasedRotation);
            }

            transform.rotation = baseRotation;
        }

        Vector3 rightDir = transform.right;
        Vector3 offsetPos = splinePos + (rightDir * sideDriftOffset);
        transform.position = new Vector3(offsetPos.x, transform.position.y, offsetPos.z);
    }

    void HandleDrift()
    {
        if (carChild == null) return;

        float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
        Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
        Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
        nowTan.y = 0; futureTan.y = 0;
        nowTan.Normalize(); futureTan.Normalize();

        float turnAngle = Vector3.SignedAngle(nowTan, futureTan, Vector3.up);
        bool isTurning = Mathf.Abs(turnAngle) > 2f;

        if (currentSpeed < minDriftSpeed)
        {
            targetDriftAngle = 0f;
            currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, 0f, ref driftVelocity, 0.1f);
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, 0.1f);
            carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
            isInTurn = false;
            return;
        }

        bool canDrift = currentSpeed > minDriftSpeed && isTurning;

        if (canDrift)
        {
            float speedFactor = Mathf.InverseLerp(minDriftSpeed, maxSpeed, currentSpeed);
            float speedDriftMultiplier = Mathf.Pow(speedFactor, driftSpeedCurve);

            float baseTarget = Mathf.Clamp(turnAngle * turnSensitivity, -maxDriftAngle, maxDriftAngle) * speedDriftMultiplier;

            if (!isInTurn)
            {
                isInTurn = true;
                float overshoot = Mathf.Sign(baseTarget) * Mathf.Abs(baseTarget) * overshootFactor;
                targetDriftAngle = Mathf.Clamp(baseTarget + overshoot, -maxDriftAngle, maxDriftAngle);
            }
            else
            {
                targetDriftAngle = Mathf.Lerp(targetDriftAngle, baseTarget, Time.deltaTime * 4f);
            }

            float normalizedDrift = currentDriftAngle / maxDriftAngle;
            float slideCurve = Mathf.Sin(normalizedDrift * Mathf.PI * 0.5f);
            float targetSideOffset = slideCurve * maxSideDriftOffset * speedDriftMultiplier;

            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, targetSideOffset, ref sideDriftVelocity, sideDriftSpeed);

            float driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.85f, Mathf.Abs(normalizedDrift));
            currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2f);
        }
        else
        {
            targetDriftAngle = 0f;
            isInTurn = false;
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, centerReturnSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 2f);
        }

        currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref driftVelocity, driftSmoothTime);
        carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
    }

    void HandleDriftParticles()
    {
        if (driftParticles == null) return;
        bool shouldDrift = Mathf.Abs(currentDriftAngle) > 8f && currentSpeed > minDriftSpeed;

        if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
        else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
    }

    // void HandleDriftTrails()
    // {
    //     if (leftTrail == null || rightTrail == null) return;

    //     if (carChild != null)
    //     {
    //         Vector3 leftWheelLocal = new Vector3(-wheelDistance * 0.5f, 0.05f, -rearWheelOffset);
    //         Vector3 rightWheelLocal = new Vector3(wheelDistance * 0.5f, 0.05f, -rearWheelOffset);

    //         Vector3 leftWheelWorld = carChild.TransformPoint(leftWheelLocal);
    //         Vector3 rightWheelWorld = carChild.TransformPoint(rightWheelLocal);

    //         leftWheelWorld.y = transform.position.y + 0.05f;
    //         rightWheelWorld.y = transform.position.y + 0.05f;

    //         leftTrailObject.transform.position = leftWheelWorld;
    //         rightTrailObject.transform.position = rightWheelWorld;
    //     }

    //     if (isTouching && currentSpeed > minBrakeSpeed)
    //         highSpeedTimer += Time.deltaTime;
    //     else if (currentSpeed < minBrakeSpeed * 0.7f)
    //         highSpeedTimer = 0f;

    //     bool isDrifting = Mathf.Abs(currentDriftAngle) > trailStartAngle;
    //     bool showDriftMarks = isDrifting;

    //     float decelAmount = (previousSpeed - currentSpeed) / Mathf.Max(Time.deltaTime, 0.01f);
    //     bool isBrakingNow = !isTouching && currentSpeed < previousSpeed;

    //     bool cameFromSpeed = previousSpeed > 10f;
    //     bool brakingStrong = decelAmount > 3.5f;
    //     bool stillRolling = currentSpeed > 2.5f;

    //     bool showBrakeMarks =
    //         cameFromSpeed &&
    //         brakingStrong &&
    //         stillRolling &&
    //         isBrakingNow &&
    //         highSpeedTimer > 0.3f;

    //     if (currentSpeed < 2f)
    //         showBrakeMarks = false;

    //     bool shouldShowTrails = showDriftMarks || showBrakeMarks;
    //     leftTrail.emitting = shouldShowTrails;
    //     rightTrail.emitting = shouldShowTrails;

    //     if (dynamicWidth && shouldShowTrails)
    //     {
    //         float driftIntensity = Mathf.Clamp01(Mathf.Abs(currentDriftAngle) / maxDriftAngle);
    //         float width = Mathf.Lerp(trailWidth * 0.3f, trailWidth * 1.5f, driftIntensity);
    //         leftTrail.startWidth = width;
    //         rightTrail.startWidth = width;
    //     }
    //     else
    //     {
    //         leftTrail.startWidth = trailWidth;
    //         rightTrail.startWidth = trailWidth;
    //     }

    //     previousSpeed = currentSpeed;
    // }

    void HandleDriftTrails()
    {
        if (leftTrail == null || rightTrail == null) return;

        if (carChild != null)
        {
            Vector3 leftWheelLocal = new Vector3(-wheelDistance * 0.5f, 0.05f, -rearWheelOffset);
            Vector3 rightWheelLocal = new Vector3(wheelDistance * 0.5f, 0.05f, -rearWheelOffset);

            Vector3 leftWheelWorld = carChild.TransformPoint(leftWheelLocal);
            Vector3 rightWheelWorld = carChild.TransformPoint(rightWheelLocal);

            // Align trails with the actual ground using raycast
            RaycastHit hit;
            float raycastDistance = 2f; // Adjust if needed

            if (Physics.Raycast(leftWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
                leftWheelWorld = hit.point + Vector3.up * 0.02f; // small offset above ground
            else
                leftWheelWorld.y = transform.position.y + 0.02f;

            if (Physics.Raycast(rightWheelWorld + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance))
                rightWheelWorld = hit.point + Vector3.up * 0.02f;
            else
                rightWheelWorld.y = transform.position.y + 0.02f;

            leftTrailObject.transform.position = leftWheelWorld;
            rightTrailObject.transform.position = rightWheelWorld;
           

        }

        // --- same logic as before ---
        if (isTouching && currentSpeed > minBrakeSpeed)
            highSpeedTimer += Time.deltaTime;
        else if (currentSpeed < minBrakeSpeed * 0.7f)
            highSpeedTimer = 0f;

        bool isDrifting = Mathf.Abs(currentDriftAngle) > trailStartAngle;
        bool showDriftMarks = isDrifting;

        float decelAmount = (previousSpeed - currentSpeed) / Mathf.Max(Time.deltaTime, 0.01f);
        bool isBrakingNow = !isTouching && currentSpeed < previousSpeed;

        bool cameFromSpeed = previousSpeed > 10f;
        bool brakingStrong = decelAmount > 3.5f;
        bool stillRolling = currentSpeed > 2.5f;

        bool showBrakeMarks =
            cameFromSpeed &&
            brakingStrong &&
            stillRolling &&
            isBrakingNow &&
            highSpeedTimer > 0.3f;

        if (currentSpeed < 2f)
            showBrakeMarks = false;

        bool shouldShowTrails = showDriftMarks || showBrakeMarks;
        leftTrail.emitting = shouldShowTrails;
        rightTrail.emitting = shouldShowTrails;

        // if (dynamicWidth && shouldShowTrails)
        // {
        //     float driftIntensity = Mathf.Clamp01(Mathf.Abs(currentDriftAngle) / maxDriftAngle);
        //     float width = Mathf.Lerp(trailWidth * 0.3f, trailWidth * 1.5f, driftIntensity);
        //     leftTrail.startWidth = width;
        //     rightTrail.startWidth = width;
        // }
        // else
        // {
        //     leftTrail.startWidth = trailWidth;
        //     rightTrail.startWidth = trailWidth;
        // }
        leftTrail.startWidth = trailWidth;
        rightTrail.startWidth = trailWidth;


        previousSpeed = currentSpeed;
    }


    void OnDrawGizmos()
    {
        if (splineContainer != null && splineContainer.Spline != null)
        {
            Gizmos.color = Color.yellow;
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 p = splineContainer.EvaluatePosition(t);
                Gizmos.DrawSphere(p, 0.2f);
                if (i > 0)
                {
                    float prevT = (float)(i - 1) / segments;
                    Vector3 prevP = splineContainer.EvaluatePosition(prevT);
                    Gizmos.DrawLine(prevP, p);
                }
            }
        }
    }
}

