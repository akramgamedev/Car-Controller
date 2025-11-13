using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using DG.Tweening;

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

    [Header("Car Stopping")]
    public bool forceStopped = false;

    public float CurrentSpeed => currentSpeed;

    private bool allowTouch = true;

    [Header("UI References")]
    public GameObject mainMenuUI;
    private bool gameStarted = false;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        skidMarks = GetComponent<CarSkidMarks>();

        if (splineContainer == null)
        {
            LogHelper.LogError("Spline Container not assigned!");
            return;
        }

        if (carChild == null)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Car") && child.gameObject.activeInHierarchy)
                {
                    carChild = child;
                    LogHelper.Log("Auto-assigned car child: " + child.name);
                    break;
                }
            }
        }

        //fallback method if not tagged Car found
        if (carChild == null && transform.childCount > 0)
        {
            carChild = transform.GetChild(0);
            LogHelper.Log($"Auto-assigned car child: {carChild.name}");
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

    public void RefreshCarChild()
{
    LogHelper.Log("=== RefreshCarChild CALLED ===");
    LogHelper.Log($"Parent object: {gameObject.name}, Active: {gameObject.activeInHierarchy}");
    LogHelper.Log($"Child count: {transform.childCount}");
    
    carChild = null;
    
    // Log all children
    for (int i = 0; i < transform.childCount; i++)
    {
        Transform child = transform.GetChild(i);
        LogHelper.Log($"Child {i}: {child.name}, Active: {child.gameObject.activeInHierarchy}, Tag: {child.tag}");
    }
    
    // Try to find car with "Car" tag
    foreach (Transform child in transform)
    {
        if (child.gameObject.activeInHierarchy)
        {
            if (child.CompareTag("Car"))
            {
                carChild = child;
                LogHelper.Log($"✓ Found car child with Car tag: {child.name}");
                break;
            }
        }
    }
    
    // Fallback to first active child
    if (carChild == null)
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                carChild = child;
                LogHelper.Log($"✓ Found car child (fallback): {carChild.name}");
                break;
            }
        }
    }
    
    if (carChild == null)
    {
        LogHelper.LogError("❌ NO CAR CHILD FOUND!");
        return;
    }
    
    // Reinitialize
    carChild.localRotation = Quaternion.identity;
    if (skidMarks != null)
    {
        skidMarks.Initialize(carChild);
        LogHelper.Log("✓ Skid marks reinitialized");
    }
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
        // -----------------------
        // BEFORE GAME START
        // -----------------------
        if (!gameStarted)
        {
            bool isOverUI = false;

            if (EventSystem.current != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                isOverUI = EventSystem.current.IsPointerOverGameObject();
#else
            // ADD HERE - FIRST LOCATION
            isOverUI = EventSystem.current.IsPointerOverGameObject(-1);
            if (!isOverUI && Input.touchCount > 0)
            {
                isOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
#endif
            }

            // Detect tap anywhere that is NOT UI
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0) && !isOverUI)
            {
                StartGame();
            }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began && !isOverUI)
            {
                StartGame();
            }
        }
#endif
            isTouching = false;
            return;
        }

        // -----------------------
        // AFTER GAME START
        // -----------------------
        if (!allowTouch)
        {
            isTouching = false;
            return;
        }

        bool touchingUI = false;

        if (EventSystem.current != null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            touchingUI = EventSystem.current.IsPointerOverGameObject();
#else
        // ADD HERE - SECOND LOCATION
        touchingUI = EventSystem.current.IsPointerOverGameObject(-1);
        if (!touchingUI && Input.touchCount > 0)
        {
            touchingUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
#endif
        }

        if (touchingUI)
        {
            isTouching = false;
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        isTouching = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
#else
    if (Input.touchCount > 0)
    {
        Touch t = Input.GetTouch(0);
        isTouching = t.phase == TouchPhase.Began ||
                     t.phase == TouchPhase.Stationary ||
                     t.phase == TouchPhase.Moved;
    }
    else
    {
        isTouching = false;
    }
#endif
    }

    void HandleMovement()
    {
        // FORCE STOP CHECK - MUST BE FIRST
        if (forceStopped)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 3f * Time.deltaTime);
            if (currentSpeed < 0.01f)
            {
                currentSpeed = 0f;
                LogHelper.Log("Car fully stopped for pickup");
            }
            return;
        }

        // Check if reached end
        if (reachedEnd && !loopSpline)
        {
            currentSpeed = 0f;
            return;
        }

        // Handle acceleration/deceleration based on input
        if (isTouching)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2.2f * Time.deltaTime);

        // Stop completely at low speeds when not touching
        if (currentSpeed < 0.3f && !isTouching)
            currentSpeed = 0f;

        // Don't move if speed is too low
        if (currentSpeed <= 0.01f) return;

        // Calculate movement along spline
        float speedOnSpline = currentSpeed / totalSplineLength;
        splineProgress += speedOnSpline * Time.deltaTime;

        // Handle loop or end
        if (splineProgress >= 1f)
        {
            if (loopSpline)
                splineProgress -= 1f;
            else
            {
                splineProgress = 1f;
                reachedEnd = true;
                currentSpeed = 0f;
            }
        }

        Vector3 splinePos = splineContainer.EvaluatePosition(splineProgress);

        // Calculate rotation
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

        // Apply position with side drift offset
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

    public void SetTouchEnabled(bool enabled)
    {
        isTouching = false;

        allowTouch = enabled;
    }

    void StartGame()
    {
        gameStarted = true;
        allowTouch = true;

        UIManager.Instance.HideMainMenu();

        LogHelper.Log("Game Started! UI animated and hidden. Player can now move.");
    }

}