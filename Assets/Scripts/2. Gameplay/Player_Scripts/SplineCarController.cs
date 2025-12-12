using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using DG.Tweening;

[RequireComponent(typeof(CarSkidMarks))]
public class SplineCarController : MonoBehaviour
{
    [Header("Car Sound Settings")]
    [SerializeField] private float minEnginePitch = 0.6f;
    [SerializeField] private float maxEnginePitch = 1.8f;
    private bool engineSoundPlaying = false;

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
    [Header("Drift Sound Settings")]
    public float trailStartAngle = 12f;

    private Rigidbody rb;
    private float totalSplineLength;
    private bool isTouching = false;
    private float sideDriftOffset = 0f;
    private float sideDriftVelocity = 0f;
    private Quaternion baseRotation;
    private float previousSpeed = 0f;
    private float highSpeedTimer = 0f;

    private CarSkidMarks skidMarks;
    private bool driftSoundPlaying = false;

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

        InitializeCarChild();

        if (CheckpointManager.Instance.HasCheckpoint())
        {
            bool loaded = CheckpointManager.Instance.LoadCheckpoint(this);
            if (loaded)
            {
                LogHelper.Log("Checkpoint loaded successfully!");
            }
        }
    }

    void OnDisable()
    {
        if (engineSoundPlaying)
        {
            AudioManager.Instance.StopCarEngine();
            engineSoundPlaying = false;
        }

        if (driftSoundPlaying)
        {
            AudioManager.Instance.StopCarDrift();
            driftSoundPlaying = false;
        }
    }

    void OnDestroy()
    {
        if (engineSoundPlaying)
        {
            AudioManager.Instance.StopCarEngine();
            engineSoundPlaying = false;
        }
        if (driftSoundPlaying)
        {
            AudioManager.Instance.StopCarDrift();
            driftSoundPlaying = false;
        }

    }

    private void InitializeCarChild()
    {
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

        if (carChild == null && transform.childCount > 0)
        {
            carChild = transform.GetChild(0);
            LogHelper.Log($"Auto-assigned car child: {carChild.name}");
        }

        if (carChild != null)
        {
            carChild.localRotation = Quaternion.identity;
            if (skidMarks != null)
            {
                skidMarks.Initialize(carChild);
            }
        }
    }

    private void InitializeSpline()
    {
        if (splineContainer == null)
        {
            LogHelper.LogError("Spline Container not assigned!");
            return;
        }

        totalSplineLength = splineContainer.Spline.GetLength();
        splineProgress = 0f;
        reachedEnd = false;
        currentSpeed = 0f;
        currentDriftAngle = 0f;
        sideDriftOffset = 0f;
        sideDriftVelocity = 0f;
        driftVelocity = 0f;
        targetDriftAngle = 0f;
        isInTurn = false;

        Vector3 startPos = splineContainer.EvaluatePosition(0f);
        transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

        Vector3 startTangent = splineContainer.EvaluateTangent(0f);
        startTangent.y = 0;
        startTangent.Normalize();
        baseRotation = Quaternion.LookRotation(startTangent);
        transform.rotation = baseRotation;

        if (carChild != null)
        {
            carChild.localRotation = Quaternion.identity;
        }

        LogHelper.Log($"Spline initialized: Length = {totalSplineLength}");
    }


    public void RefreshCarChild()
    {
        LogHelper.Log("=== RefreshCarChild CALLED ===");
        LogHelper.Log($"Parent object: {gameObject.name}, Active: {gameObject.activeInHierarchy}");
        LogHelper.Log($"Child count: {transform.childCount}");

        carChild = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            LogHelper.Log($"Child {i}: {child.name}, Active: {child.gameObject.activeInHierarchy}, Tag: {child.tag}");
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                if (child.CompareTag("Car"))
                {
                    carChild = child;
                    LogHelper.Log($"Found car child with Car tag: {child.name}");
                    break;
                }
            }
        }

        if (carChild == null)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    carChild = child;
                    LogHelper.Log($"Found car child (fallback): {carChild.name}");
                    break;
                }
            }
        }

        if (carChild == null)
        {
            LogHelper.LogError("NO CAR CHILD FOUND!");
            return;
        }

        // Reinitialize
        carChild.localRotation = Quaternion.identity;
        if (skidMarks != null)
        {
            skidMarks.Initialize(carChild);
            LogHelper.Log("Skid marks reinitialized");
        }
    }

    public void RefreshSpline()
    {
        if (splineContainer == null)
        {
            LogHelper.LogError("Cannot refresh spline - splineContainer is null!");
            return;
        }
        LogHelper.Log("=== RefreshSpline CALLED ===");
        LogHelper.Log($"New Spline Container: {splineContainer.name}");

        InitializeSpline();

        LogHelper.Log("Spline refresh complete");
    }
    public void SetupNewLevel(SplineContainer newSpline)
    {
        if (newSpline == null)
        {
            LogHelper.LogError("Cannot setup new level - newSpline is null!");
            return;
        }

        LogHelper.Log($"=== Setting up new level with spline: {newSpline.name} ===");

        splineContainer = newSpline;
        InitializeSpline();

        LogHelper.Log("New level setup complete");
    }

    void Update()
    {
        if (!enabled) return;

        HandleInput();
        HandleMovement();
        HandleDrift();
        HandleEngineSound();
        //HandleDriftParticles();
        skidMarks.HandleDriftTrails(isTouching, currentSpeed, currentDriftAngle, ref previousSpeed, ref highSpeedTimer);
        HandleDriftSound();
    }

    void HandleInput()
    {
        if (!gameStarted)
        {
            bool isOverUI = false;

            if (EventSystem.current != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                isOverUI = EventSystem.current.IsPointerOverGameObject();
#else
            isOverUI = EventSystem.current.IsPointerOverGameObject(-1);
            if (!isOverUI && Input.touchCount > 0)
            {
                isOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
#endif
            }

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

    void HandleDriftSound()
    {
        if (!gameStarted) return;

        // Sync directly with skid marks trail state
        bool shouldPlayDrift = skidMarks.IsShowingTrails;

        if (shouldPlayDrift && !driftSoundPlaying)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(currentDriftAngle) / maxDriftAngle);
            if (intensity < 0.4f) intensity = 0.5f; // Minimum intensity for braking

            AudioManager.Instance.PlayCarDrift(intensity);
            driftSoundPlaying = true;
        }
        else if (shouldPlayDrift && driftSoundPlaying)
        {
            // Update drift sound intensity dynamically
            float intensity = Mathf.Clamp01(Mathf.Abs(currentDriftAngle) / maxDriftAngle);
            if (intensity < 0.4f) intensity = 0.5f;

            AudioManager.Instance.PlayCarDrift(intensity);
        }
        else if (!shouldPlayDrift && driftSoundPlaying)
        {
            AudioManager.Instance.StopCarDrift();
            driftSoundPlaying = false;
        }
    }

    void HandleEngineSound()
    {
        if (!gameStarted) return;

        if (currentSpeed > 0.1f && !engineSoundPlaying)
        {
            AudioManager.Instance.PlayCarEngine("CarEngine", minEnginePitch);
            engineSoundPlaying = true;
        }
        if (currentSpeed <= 0.1f && engineSoundPlaying)
        {
            AudioManager.Instance.StopCarEngine();
            engineSoundPlaying = false;
            return;
        }

        if (engineSoundPlaying)
        {
            float speedRatio = Mathf.InverseLerp(0f, maxSpeed, currentSpeed);
            float targetPitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedRatio);
            AudioManager.Instance.PlayCarEngine("CarEngine", targetPitch);
        }

    }

    // void HandleDriftParticles()
    // {
    //     if (driftParticles == null) return;
    //     bool shouldDrift = Mathf.Abs(currentDriftAngle) > 8f && currentSpeed > minDriftSpeed;

    //     if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
    //     else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
    // }

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
    public float GetSplineProgress()
    {
        return splineProgress;
    }

    public void RestoreFromCheckpoint(float progress, Vector3 position, Quaternion rotation, float speed)
    {
        splineProgress = progress;
        transform.position = position;
        transform.rotation = rotation;
        baseRotation = rotation;
        currentSpeed = speed;

        // Reset drift states
        currentDriftAngle = 0f;
        sideDriftOffset = 0f;
        sideDriftVelocity = 0f;
        driftVelocity = 0f;
        targetDriftAngle = 0f;
        isInTurn = false;
        reachedEnd = false;

        if (carChild != null)
        {
            carChild.localRotation = Quaternion.identity;
        }

        LogHelper.Log($"Car restored from checkpoint at progress: {progress:F3}");
    }
}