using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(CarSkidMarks))]
public class SplineCarController : MonoBehaviour
{
    [Header("Spline Settings")]
    public SplineContainer splineContainer;
    public bool loopSpline = false;
    private float splineProgress = 0f;
    private bool reachedEnd = false;

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
    public float driftSpeedCurve = 2.0f;
    public float driftSmoothTime = 0.08f;
    public float overshootFactor = 0.7f;

    private float currentDriftAngle = 0f;
    private float driftVelocity = 0f;
    private float targetDriftAngle = 0f;
    private bool isInTurn = false;

    [Header("Turn Detection")]
    public float lookAheadDistance = 0.008f;
    public float rotationSpeed = 30f;
    public float rotationLookAhead = 0.002f;

    [Header("Visual Settings")]
    public Transform carChild;
    public ParticleSystem driftParticles;

    [Header("Side Drift Effect (Pick Me Up Style)")]
    public float maxSideDriftOffset = 3.0f;
    public float sideDriftSpeed = 0.25f;
    public float centerReturnSpeed = 0.12f;
    [Range(0f, 1f)]
    public float frontPivotRatio = 0.3f;

    private Rigidbody rb;
    private float totalSplineLength;
    private bool isTouching = false;
    private float sideDriftOffset = 0f;
    private float sideDriftVelocity = 0f;
    private Quaternion baseRotation;
    private float previousSpeed = 0f;
    private float highSpeedTimer = 0f;

    private CarSkidMarks skidMarks;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        skidMarks = GetComponent<CarSkidMarks>();

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

        skidMarks.Initialize(carChild);
    }

    void Update()
    {
        if (!enabled) return;

        HandleInput();
        HandleMovement();
        HandleDrift();
        HandleDriftParticles();
        skidMarks.HandleDriftTrails(isTouching, currentSpeed, currentDriftAngle, ref previousSpeed, ref highSpeedTimer);
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

        float driftExitSmoothTime = driftSmoothTime * 1.5f;
        float sideDriftExitSpeed = sideDriftSpeed * 1.2f;

        if (currentSpeed < minDriftSpeed)
        {
            targetDriftAngle = Mathf.Lerp(targetDriftAngle, 0f, Time.deltaTime * 4f);
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, sideDriftExitSpeed);
            currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, 0f, ref driftVelocity, driftExitSmoothTime);

            if (Mathf.Abs(sideDriftOffset) < 0.01f)
            {
                sideDriftOffset = 0f;
                sideDriftVelocity = 0f;
            }

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
            currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref driftVelocity, driftSmoothTime);

            float driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.85f, Mathf.Abs(normalizedDrift));
            currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2f);
        }
        else
        {
            targetDriftAngle = Mathf.Lerp(targetDriftAngle, 0f, Time.deltaTime * 4f);
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, sideDriftExitSpeed);
            currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, 0f, ref driftVelocity, driftExitSmoothTime);
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 2f);

            if (Mathf.Abs(sideDriftOffset) < 0.01f)
            {
                sideDriftOffset = 0f;
                sideDriftVelocity = 0f;
            }

            isInTurn = false;
        }

        carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
    }

    void HandleDriftParticles()
    {
        if (driftParticles == null) return;
        bool shouldDrift = Mathf.Abs(currentDriftAngle) > 8f && currentSpeed > minDriftSpeed;

        if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
        else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
    }
}



