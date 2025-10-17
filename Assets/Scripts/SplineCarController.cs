using UnityEngine;
using UnityEngine.Splines;

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
    public float lookAheadDistance = 0.008f; // Much smaller!
    public float rotationSpeed = 30f; // Faster to compensate
    [Tooltip("Lookahead for rotation - very small = rotates AT turn, not before")]
    public float rotationLookAhead = 0.002f; // Almost zero!

    [Header("Visual Settings")]
    public Transform carChild;
    public ParticleSystem driftParticles;

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
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleDrift();
        HandleDriftParticles();
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
        {
            if (currentSpeed < maxSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2.2f * Time.deltaTime);
        }

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

        // PICK ME UP STYLE: Position calculation
        // During drift, the car slides off the spline sideways
        Vector3 splinePos = splineContainer.EvaluatePosition(splineProgress);
        
        // Calculate tangent for direction
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

        // CRITICAL: Apply sideways offset based on drift
        // This makes the car slide off the spline during drift
        Vector3 rightDir = transform.right;
        Vector3 offsetPos = splinePos + (rightDir * sideDriftOffset);
        
        // Keep Y position constant
        transform.position = new Vector3(offsetPos.x, transform.position.y, offsetPos.z);
    }

    void HandleDrift()
    {
        if (carChild == null) return;

        // Detect turn angle - use current position vs VERY close future
        float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
        Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
        Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
        nowTan.y = 0; futureTan.y = 0;
        nowTan.Normalize(); futureTan.Normalize();

        float turnAngle = Vector3.SignedAngle(nowTan, futureTan, Vector3.up);
        // Only detect turn if angle is significant enough
        bool isTurning = Mathf.Abs(turnAngle) > 2f; // Increased threshold

        // Below minimum speed = no drift at all
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
            // Calculate drift intensity based on speed
            float speedFactor = Mathf.InverseLerp(minDriftSpeed, maxSpeed, currentSpeed);
            float speedDriftMultiplier = Mathf.Pow(speedFactor, driftSpeedCurve);

            // Calculate target drift angle
            float baseTarget = Mathf.Clamp(turnAngle * turnSensitivity, -maxDriftAngle, maxDriftAngle) * speedDriftMultiplier;

            if (!isInTurn)
            {
                // Entering drift - add overshoot for that satisfying snap
                isInTurn = true;
                float overshoot = Mathf.Sign(baseTarget) * Mathf.Abs(baseTarget) * overshootFactor;
                targetDriftAngle = Mathf.Clamp(baseTarget + overshoot, -maxDriftAngle, maxDriftAngle);
            }
            else
            {
                // Already drifting - smoothly adjust
                targetDriftAngle = Mathf.Lerp(targetDriftAngle, baseTarget, Time.deltaTime * 4f);
            }

            // PICK ME UP STYLE: Calculate sideways offset
            // The car slides perpendicular to the spline during drift
            // Positive drift angle = slide right, negative = slide left
            float normalizedDrift = currentDriftAngle / maxDriftAngle;
            
            // Use sine curve for smooth, natural-looking slide
            float slideCurve = Mathf.Sin(normalizedDrift * Mathf.PI * 0.5f);
            float targetSideOffset = slideCurve * maxSideDriftOffset * speedDriftMultiplier;
            
            // Smoothly move sideways
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, targetSideOffset, ref sideDriftVelocity, sideDriftSpeed);

            // Slight speed reduction during drift (realistic)
            float driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.85f, Mathf.Abs(normalizedDrift));
            currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2f);
        }
        else
        {
            // Not drifting - return to center
            targetDriftAngle = 0f;
            isInTurn = false;

            // QUICK snap back to spline (Pick Me Up style)
            sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, centerReturnSpeed);

            // Return to full speed
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 2f);
        }

        // Smooth drift angle transition
        currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref driftVelocity, driftSmoothTime);

        // PICK ME UP STYLE: Apply rotation to child
        // This creates the "rear swings out" effect
        carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
    }

    void HandleDriftParticles()
    {
        if (driftParticles == null) return;
        bool shouldDrift = Mathf.Abs(currentDriftAngle) > 8f && currentSpeed > minDriftSpeed;

        if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
        else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
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

// using UnityEngine;
// using UnityEngine.Splines;

// public class SplineCarController : MonoBehaviour
// {
//     [Header("Spline Settings")]
//     public SplineContainer splineContainer;
//     public bool loopSpline = false;
//     private float splineProgress = 0f;
//     private bool reachedEnd = false;

//     [Header("Movement Settings")]
//     public float maxSpeed = 15f;
//     public float acceleration = 7f;
//     public float deceleration = 10f;
//     private float currentSpeed = 0f;

//     [Header("Low-Speed Behaviour")]
//     public float rotationStopSpeed = 4f;


//     [Header("Drift Settings (VIDEO-LIKE)")]
//     public float driftThreshold = 8f;
//     public float minDriftSpeed = 12f; // Minimum speed for ANY drift to occur
//     public float maxDriftAngle = 45f;
//     public float enterSmoothTime = 0.06f;
//     public float exitSmoothTime = 0.12f;
//     public float overshootFactor = 0.4f;
//     public float driftHoldDuration = 0.18f;
//     public float turnSensitivity = 1.5f;
//     [Tooltip("Controls how speed affects drift intensity (higher = needs more speed for full drift)")]
//     public float driftSpeedCurve = 2.5f;

//     private float currentDriftAngle = 0f;
//     private float currentDriftVelocity = 0f;
//     private float targetDriftAngle = 0f;
//     private float holdTimer = 0f;
//     private bool isInTurn = false;

//     [Header("Turn Detection")]
//     public float lookAheadDistance = 0.03f;
//     public float rotationSpeed = 25f;
//     public float rotationLookAhead = 0.05f;

//     [Header("Visual Settings")]
//     public Transform carChild;
//     public ParticleSystem driftParticles;

//     private Rigidbody rb;
//     private float totalSplineLength;
//     private bool isTouching = false;
//     private float driftSpeedTarget;
//     private float sideDriftOffset = 0f;
//     private float sideDriftVelocity = 0f;

//     [Header("Side Drift Effect")]
//     [Tooltip("How far the car slides sideways during drift")]
//     public float maxSideDriftOffset = 2.5f; // Increased for more visible drift
//     [Tooltip("How quickly it returns to center after drift")]
//     public float sideDriftSmoothTime = 0.35f; // Slower return for smoother feel
//     [Tooltip("How quickly to snap back to center")]
//     public float centerReturnSpeed = 0.15f; // Fast return when drift ends

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();

//         if (splineContainer == null)
//         {
//             Debug.LogError("Spline Container not assigned!");
//             return;
//         }

//         if (carChild == null && transform.childCount > 0)
//         {
//             carChild = transform.GetChild(0);
//             Debug.Log($"Auto-assigned car child: {carChild.name}");
//         }

//         totalSplineLength = splineContainer.Spline.GetLength();

//         Vector3 startPos = splineContainer.EvaluatePosition(0f);
//         transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

//         carChild.localRotation = Quaternion.identity;
//     }

//     void Update()
//     {
//         HandleInput();
//         HandleMovement();
//         HandleDrift();
//         HandleDriftParticles();
//     }

//     void HandleInput()
//     {
//         if (Input.touchCount > 0)
//         {
//             Touch t = Input.GetTouch(0);
//             isTouching = t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved;
//         }
//         else isTouching = false;

// #if UNITY_EDITOR || UNITY_STANDALONE
//         if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) isTouching = true;
// #endif
//     }

//     void HandleMovement()
//     {
//         if (reachedEnd && !loopSpline)
//         {
//             currentSpeed = 0f;
//             return;
//         }

//         // FIXED: Prevent speed from exceeding maxSpeed with repeated taps
//         if (isTouching)
//         {
//             // Only accelerate if below max speed
//             if (currentSpeed < maxSpeed)
//             {
//                 currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
//             }
//             // Clamp to ensure we never exceed maxSpeed
//             currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
//         }
//         else
//         {
//             // Quick deceleration when not touching
//             currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2.2f * Time.deltaTime);
//         }

//         // Stop completely at very low speeds
//         if (currentSpeed < 0.3f && !isTouching)
//             currentSpeed = 0f;

//         if (currentSpeed <= 0.01f) return;

//         float speedOnSpline = currentSpeed / totalSplineLength;
//         splineProgress += speedOnSpline * Time.deltaTime;

//         if (splineProgress >= 1f)
//         {
//             if (loopSpline) splineProgress -= 1f;
//             else { splineProgress = 1f; reachedEnd = true; currentSpeed = 0f; }
//         }

//         Vector3 pos = splineContainer.EvaluatePosition(splineProgress);

//         // Add side drift offset (local X) - more pronounced during drift
//         Vector3 offsetPos = pos + transform.right * sideDriftOffset;
//         transform.position = new Vector3(offsetPos.x, transform.position.y, offsetPos.z);

//         // Predictive rotation
//         float lookAhead = Mathf.Clamp01(splineProgress + rotationLookAhead);
//         Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
//         Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
//         nowTan.y = 0; futureTan.y = 0;
//         nowTan.Normalize(); futureTan.Normalize();

//         Vector3 blendDir = Vector3.Lerp(nowTan, futureTan, 0.6f).normalized;
//        if (blendDir != Vector3.zero)
// {
//     Quaternion targetRot = Quaternion.LookRotation(blendDir);

//     // Compute a speed-based blend value (0 = no rotation, 1 = full rotation)
//     float rotationFactor = Mathf.InverseLerp(rotationStopSpeed * 0.6f, maxSpeed, currentSpeed);
//     rotationFactor = Mathf.Pow(rotationFactor, 1.5f); // smooth nonlinear blend

//     // Compute dynamic rotation speed
//     float angleDiff = Quaternion.Angle(transform.rotation, targetRot);
//     float dynamicRot = Mathf.Lerp(2f, rotationSpeed, rotationFactor);
//     dynamicRot *= Mathf.Lerp(0.8f, 1.25f, Mathf.Clamp01(angleDiff / 25f));

//     // If very slow, damp rotation even more
//     if (currentSpeed < rotationStopSpeed)
//     {
//         // freeze orientation slightly, keep the car steady
//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             transform.rotation,
//             Time.deltaTime * 1f
//         );
//     }
//     else
//     {
//         // gradually rotate as speed increases
//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             targetRot,
//             Time.deltaTime * dynamicRot * rotationFactor
//         );
//     }
// }



//     }
//     void HandleDrift()
//     {
//         if (carChild == null) return;

//         float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
//         Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
//         Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
//         nowTan.y = 0; futureTan.y = 0;
//         nowTan.Normalize(); futureTan.Normalize();

//         float turnAngle = Vector3.SignedAngle(nowTan, futureTan, Vector3.up);
//         bool isTurning = Mathf.Abs(turnAngle) > 1f;

//         // NEW: completely block drift logic when below minimum drift speed
//         if (currentSpeed < minDriftSpeed)
//         {
//             targetDriftAngle = 0f;
//             currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, 0f, ref currentDriftVelocity, 0.1f);
//             sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, 0.1f);
//             carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
//             isInTurn = false;
//             return; // exit early – no drift at low speed
//         }

//         bool canDrift = currentSpeed > minDriftSpeed && isTurning;

//         if (canDrift)
//         {
//             float speedFactor = Mathf.InverseLerp(minDriftSpeed, maxSpeed, currentSpeed);
//             float speedDriftMultiplier = Mathf.Pow(speedFactor, driftSpeedCurve);

//             float baseTarget = Mathf.Clamp(turnAngle * turnSensitivity, -maxDriftAngle, maxDriftAngle) * speedDriftMultiplier;

//             if (!isInTurn)
//             {
//                 isInTurn = true;
//                 float overshoot = Mathf.Sign(baseTarget) * Mathf.Abs(baseTarget) * overshootFactor;
//                 targetDriftAngle = Mathf.Clamp(baseTarget + overshoot, -maxDriftAngle, maxDriftAngle);
//                 holdTimer = driftHoldDuration;
//             }
//             else
//             {
//                 targetDriftAngle = Mathf.Lerp(targetDriftAngle, baseTarget, Time.deltaTime * 3.5f);
//                 holdTimer = Mathf.Max(holdTimer, driftHoldDuration * 0.4f);
//             }

//             driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.8f, Mathf.Abs(targetDriftAngle) / maxDriftAngle);
//             currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2.5f);

//             float normalizedAngle = currentDriftAngle / maxDriftAngle;
//             float driftCurve = Mathf.Sin(normalizedAngle * Mathf.PI * 0.5f);
//             float sideTarget = driftCurve * maxSideDriftOffset * speedDriftMultiplier;
//             sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, sideTarget, ref sideDriftVelocity, sideDriftSmoothTime);
//         }
//         else
//         {
//             targetDriftAngle = 0f;

//             if (isInTurn)
//             {
//                 if (holdTimer > 0f)
//                     holdTimer -= Time.deltaTime;
//                 else
//                     isInTurn = false;
//             }

//             sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, centerReturnSpeed);
//             currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 1.5f);
//         }

//         float smoothTime = Mathf.Abs(targetDriftAngle) > Mathf.Abs(currentDriftAngle) ? enterSmoothTime : exitSmoothTime;
//         currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref currentDriftVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);

//         carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
//     }


  

//     void HandleDriftParticles()
//     {
//         if (driftParticles == null) return;
//         bool shouldDrift = Mathf.Abs(currentDriftAngle) > 6f && currentSpeed > minDriftSpeed;

//         if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
//         else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
//     }

//     void OnDrawGizmos()
//     {
//         if (splineContainer != null && splineContainer.Spline != null)
//         {
//             Gizmos.color = Color.yellow;
//             int segments = 50;
//             for (int i = 0; i <= segments; i++)
//             {
//                 float t = (float)i / segments;
//                 Vector3 p = splineContainer.EvaluatePosition(t);
//                 Gizmos.DrawSphere(p, 0.2f);
//                 if (i > 0)
//                 {
//                     float prevT = (float)(i - 1) / segments;
//                     Vector3 prevP = splineContainer.EvaluatePosition(prevT);
//                     Gizmos.DrawLine(prevP, p);
//                 }
//             }
//         }
//     }

//       // void HandleDrift()
//     // {
//     //     if (carChild == null) return;

//     //     float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
//     //     Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
//     //     Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
//     //     nowTan.y = 0; futureTan.y = 0;
//     //     nowTan.Normalize(); futureTan.Normalize();

//     //     float turnAngle = Vector3.SignedAngle(nowTan, futureTan, Vector3.up);
//     //     bool isTurning = Mathf.Abs(turnAngle) > 1f;

//     //     // FIXED: Only allow drift above minimum speed threshold
//     //     bool canDrift = currentSpeed > minDriftSpeed && isTurning;

//     //     if (canDrift)
//     //     {
//     //         // SPEED-BASED SCALING: Calculate how much speed affects drift intensity
//     //         // Use minDriftSpeed as the lower bound instead of driftThreshold
//     //         float speedFactor = Mathf.InverseLerp(minDriftSpeed, maxSpeed, currentSpeed);
//     //         // Apply exponential curve to speed factor for dramatic scaling
//     //         float speedDriftMultiplier = Mathf.Pow(speedFactor, driftSpeedCurve);

//     //         // Base drift angle WITH speed scaling applied
//     //         float baseTarget = Mathf.Clamp(turnAngle * turnSensitivity, -maxDriftAngle, maxDriftAngle) * speedDriftMultiplier;

//     //         if (!isInTurn)
//     //         {
//     //             isInTurn = true;
//     //             float overshoot = Mathf.Sign(baseTarget) * Mathf.Abs(baseTarget) * overshootFactor;
//     //             targetDriftAngle = Mathf.Clamp(baseTarget + overshoot, -maxDriftAngle, maxDriftAngle);
//     //             holdTimer = driftHoldDuration;
//     //         }
//     //         else
//     //         {
//     //             targetDriftAngle = Mathf.Lerp(targetDriftAngle, baseTarget, Time.deltaTime * 3.5f);
//     //             holdTimer = Mathf.Max(holdTimer, driftHoldDuration * 0.4f);
//     //         }

//     //         driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.8f, Mathf.Abs(targetDriftAngle) / maxDriftAngle);
//     //         currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2.5f);

//     //         // IMPROVED: More pronounced sideways drift with better curve
//     //         // Use a stronger sine curve for more visible lateral movement
//     //         float normalizedAngle = currentDriftAngle / maxDriftAngle; // -1 to 1
//     //         float driftCurve = Mathf.Sin(normalizedAngle * Mathf.PI * 0.5f); // Smooth curve

//     //         // Apply the same speed multiplier to side drift for consistency
//     //         float sideTarget = driftCurve * maxSideDriftOffset * speedDriftMultiplier;

//     //         sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, sideTarget, ref sideDriftVelocity, sideDriftSmoothTime);
//     //     }
//     //     else
//     //     {
//     //         // FORCE target to zero when not drifting
//     //         targetDriftAngle = 0f;

//     //         if (isInTurn)
//     //         {
//     //             if (holdTimer > 0f)
//     //                 holdTimer -= Time.deltaTime;
//     //             else
//     //             {
//     //                 isInTurn = false;
//     //             }
//     //         }

//     //         // IMPROVED: Faster return to center when drift ends
//     //         // Snap back to center quickly and smoothly
//     //         sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, centerReturnSpeed);

//     //         // Return to full speed
//     //         currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 1.5f);
//     //     }

//     //     // Always apply drift angle smoothing regardless of speed
//     //     float smoothTime = Mathf.Abs(targetDriftAngle) > Mathf.Abs(currentDriftAngle) ? enterSmoothTime : exitSmoothTime;
//     //     currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref currentDriftVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);

//     //     carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
//     // }
// }


//********* new working code with improved drift effect *************
// using UnityEngine;
// using UnityEngine.Splines;

// public class SplineCarController : MonoBehaviour
// {
//     [Header("Spline Settings")]
//     public SplineContainer splineContainer;
//     public bool loopSpline = false;
//     private float splineProgress = 0f;
//     private bool reachedEnd = false;

//     [Header("Movement Settings")]
//     public float maxSpeed = 15f;
//     public float acceleration = 7f;
//     public float deceleration = 10f;
//     private float currentSpeed = 0f;

//     [Header("Drift Settings (VIDEO-LIKE)")]
//     public float driftThreshold = 8f;
//     public float maxDriftAngle = 45f;
//     public float enterSmoothTime = 0.06f;
//     public float exitSmoothTime = 0.12f;
//     public float overshootFactor = 0.4f;
//     public float driftHoldDuration = 0.18f;
//     public float turnSensitivity = 1.5f;

//     private float currentDriftAngle = 0f;
//     private float currentDriftVelocity = 0f;
//     private float targetDriftAngle = 0f;
//     private float holdTimer = 0f;
//     private bool isInTurn = false;

//     [Header("Turn Detection")]
//     public float lookAheadDistance = 0.03f;
//     public float rotationSpeed = 25f;
//     public float rotationLookAhead = 0.05f;

//     [Header("Visual Settings")]
//     public Transform carChild;
//     public ParticleSystem driftParticles;

//     private Rigidbody rb;
//     private float totalSplineLength;
//     private bool isTouching = false;
//     private float driftSpeedTarget;
//     private float sideDriftOffset = 0f;
//     private float sideDriftVelocity = 0f;

//     [Header("Side Drift Effect")]
//     [Tooltip("How far the car slides sideways during drift")]
//     public float maxSideDriftOffset = 2.5f; // Increased for more visible drift
//     [Tooltip("How quickly it returns to center after drift")]
//     public float sideDriftSmoothTime = 0.35f; // Slower return for smoother feel
//     [Tooltip("How quickly to snap back to center")]
//     public float centerReturnSpeed = 0.15f; // Fast return when drift ends

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();

//         if (splineContainer == null)
//         {
//             Debug.LogError("Spline Container not assigned!");
//             return;
//         }

//         if (carChild == null && transform.childCount > 0)
//         {
//             carChild = transform.GetChild(0);
//             Debug.Log($"Auto-assigned car child: {carChild.name}");
//         }

//         totalSplineLength = splineContainer.Spline.GetLength();

//         Vector3 startPos = splineContainer.EvaluatePosition(0f);
//         transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

//         carChild.localRotation = Quaternion.identity;
//     }

//     void Update()
//     {
//         HandleInput();
//         HandleMovement();
//         HandleDrift();
//         HandleDriftParticles();
//     }

//     void HandleInput()
//     {
//         if (Input.touchCount > 0)
//         {
//             Touch t = Input.GetTouch(0);
//             isTouching = t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved;
//         }
//         else isTouching = false;

// #if UNITY_EDITOR || UNITY_STANDALONE
//         if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) isTouching = true;
// #endif
//     }

//     void HandleMovement()
//     {
//         if (reachedEnd && !loopSpline)
//         {
//             currentSpeed = 0f;
//             return;
//         }

//         // FIXED: Prevent speed from exceeding maxSpeed with repeated taps
//         if (isTouching)
//         {
//             // Only accelerate if below max speed
//             if (currentSpeed < maxSpeed)
//             {
//                 currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
//             }
//             // Clamp to ensure we never exceed maxSpeed
//             currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
//         }
//         else
//         {
//             // Quick deceleration when not touching
//             currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2.2f * Time.deltaTime);
//         }

//         // Stop completely at very low speeds
//         if (currentSpeed < 0.3f && !isTouching)
//             currentSpeed = 0f;

//         if (currentSpeed <= 0.01f) return;

//         float speedOnSpline = currentSpeed / totalSplineLength;
//         splineProgress += speedOnSpline * Time.deltaTime;

//         if (splineProgress >= 1f)
//         {
//             if (loopSpline) splineProgress -= 1f;
//             else { splineProgress = 1f; reachedEnd = true; currentSpeed = 0f; }
//         }

//         Vector3 pos = splineContainer.EvaluatePosition(splineProgress);

//         // Add side drift offset (local X) - more pronounced during drift
//         Vector3 offsetPos = pos + transform.right * sideDriftOffset;
//         transform.position = new Vector3(offsetPos.x, transform.position.y, offsetPos.z);

//         // Predictive rotation
//         float lookAhead = Mathf.Clamp01(splineProgress + rotationLookAhead);
//         Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
//         Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
//         nowTan.y = 0; futureTan.y = 0;
//         nowTan.Normalize(); futureTan.Normalize();

//         Vector3 blendDir = Vector3.Lerp(nowTan, futureTan, 0.6f).normalized;
//         if (blendDir != Vector3.zero)
//         {
//             Quaternion targetRot = Quaternion.LookRotation(blendDir);
//             float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
//             float angleDiff = Quaternion.Angle(transform.rotation, targetRot);
//             float dynamicRot = Mathf.Lerp(6f, rotationSpeed, speedFactor);
//             dynamicRot *= Mathf.Lerp(0.8f, 1.25f, Mathf.Clamp01(angleDiff / 25f));
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * dynamicRot);
//         }
//     }

//     void HandleDrift()
//     {
//         if (carChild == null) return;

//         float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
//         Vector3 nowTan = splineContainer.EvaluateTangent(splineProgress);
//         Vector3 futureTan = splineContainer.EvaluateTangent(lookAhead);
//         nowTan.y = 0; futureTan.y = 0;
//         nowTan.Normalize(); futureTan.Normalize();

//         float turnAngle = Vector3.SignedAngle(nowTan, futureTan, Vector3.up);
//         bool isTurning = Mathf.Abs(turnAngle) > 1f;
//         bool canDrift = currentSpeed > driftThreshold && isTurning;

//         if (canDrift)
//         {
//             float speedFactor = Mathf.InverseLerp(driftThreshold, maxSpeed, currentSpeed);
//             float baseTarget = Mathf.Clamp(turnAngle * turnSensitivity, -maxDriftAngle, maxDriftAngle) * speedFactor;

//             if (!isInTurn)
//             {
//                 isInTurn = true;
//                 float overshoot = Mathf.Sign(baseTarget) * Mathf.Abs(baseTarget) * overshootFactor;
//                 targetDriftAngle = Mathf.Clamp(baseTarget + overshoot, -maxDriftAngle, maxDriftAngle);
//                 holdTimer = driftHoldDuration;
//             }
//             else
//             {
//                 targetDriftAngle = Mathf.Lerp(targetDriftAngle, baseTarget, Time.deltaTime * 3.5f);
//                 holdTimer = Mathf.Max(holdTimer, driftHoldDuration * 0.4f);
//             }

//             driftSpeedTarget = Mathf.Lerp(maxSpeed, maxSpeed * 0.8f, Mathf.Abs(targetDriftAngle) / maxDriftAngle);
//             currentSpeed = Mathf.Lerp(currentSpeed, driftSpeedTarget, Time.deltaTime * 2.5f);

//             // IMPROVED: More pronounced sideways drift with better curve
//             // Use a stronger sine curve for more visible lateral movement
//             float normalizedAngle = currentDriftAngle / maxDriftAngle; // -1 to 1
//             float driftCurve = Mathf.Sin(normalizedAngle * Mathf.PI * 0.5f); // Smooth curve
//             float sideTarget = driftCurve * maxSideDriftOffset * speedFactor;

//             sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, sideTarget, ref sideDriftVelocity, sideDriftSmoothTime);
//         }
//         else
//         {
//             if (isInTurn)
//             {
//                 if (holdTimer > 0f)
//                     holdTimer -= Time.deltaTime;
//                 else
//                 {
//                     isInTurn = false;
//                     targetDriftAngle = 0f;
//                 }
//             }
//             else
//             {
//                 targetDriftAngle = 0f;
//             }

//             // IMPROVED: Faster return to center when drift ends
//             // Snap back to center quickly and smoothly
//             sideDriftOffset = Mathf.SmoothDamp(sideDriftOffset, 0f, ref sideDriftVelocity, centerReturnSpeed);

//             // Return to full speed
//             currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 1.5f);
//         }

//         float smoothTime = Mathf.Abs(targetDriftAngle) > Mathf.Abs(currentDriftAngle) ? enterSmoothTime : exitSmoothTime;
//         currentDriftAngle = Mathf.SmoothDampAngle(currentDriftAngle, targetDriftAngle, ref currentDriftVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);

//         carChild.localRotation = Quaternion.Euler(0f, currentDriftAngle, 0f);
//     }

//     void HandleDriftParticles()
//     {
//         if (driftParticles == null) return;
//         bool shouldDrift = Mathf.Abs(currentDriftAngle) > 6f && currentSpeed > driftThreshold;

//         if (shouldDrift && !driftParticles.isPlaying) driftParticles.Play();
//         else if (!shouldDrift && driftParticles.isPlaying) driftParticles.Stop();
//     }

//     void OnDrawGizmos()
//     {
//         if (splineContainer != null && splineContainer.Spline != null)
//         {
//             Gizmos.color = Color.yellow;
//             int segments = 50;
//             for (int i = 0; i <= segments; i++)
//             {
//                 float t = (float)i / segments;
//                 Vector3 p = splineContainer.EvaluatePosition(t);
//                 Gizmos.DrawSphere(p, 0.2f);
//                 if (i > 0)
//                 {
//                     float prevT = (float)(i - 1) / segments;
//                     Vector3 prevP = splineContainer.EvaluatePosition(prevT);
//                     Gizmos.DrawLine(prevP, p);
//                 }
//             }
//         }
//     }
// }

// //************* working code *******************
// using UnityEngine;
// using UnityEngine.Splines;

// public class SplineCarController : MonoBehaviour
// {
//     [Header("Spline Settings")]
//     public SplineContainer splineContainer;
//     public bool loopSpline = false; // Set to false to stop at end
//     private float splineProgress = 0f; // 0 to 1 along the spline
//     private bool reachedEnd = false;

//     [Header("Movement Settings")]
//     public float maxSpeed = 15f;
//     public float acceleration = 5f;
//     public float deceleration = 8f; // Increased for quicker stop
//     private float currentSpeed = 0f;

//     [Header("Drift Settings")]
//     public float driftThreshold = 8f;
//     public float maxDriftAngle = 35f; // Increased for more dramatic drift
//     public float driftSmoothness = 8f; // Faster drift response
//     private float currentDriftAngle = 0f;

//     [Header("Turn Detection")]
//     public float lookAheadDistance = 0.08f; // Increased for better turn prediction
//     public float rotationSpeed = 15f; // New: Control rotation smoothness

//     [Header("Visual Settings")]
//     public Transform carVisual;
//     public ParticleSystem driftParticles;

//     private Rigidbody rb;
//     private float totalSplineLength;
//     private bool isTouching = false;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();

//         if (splineContainer == null)
//         {
//             Debug.LogError("Spline Container not assigned!");
//             return;
//         }

//         totalSplineLength = splineContainer.Spline.GetLength();

//         Vector3 startPos = splineContainer.EvaluatePosition(0f);
//         transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

//         if (carVisual == null)
//             carVisual = transform;
//     }

//     void Update()
//     {
//         HandleInput();
//         HandleMovement();
//         HandleDrift();
//         HandleDriftParticles();
//     }

//     void HandleInput()
//     {
//         // Mobile touch input - hold to move
//         if (Input.touchCount > 0)
//         {
//             Touch touch = Input.GetTouch(0);
//             isTouching = (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved);
//         }
//         else
//         {
//             isTouching = false;
//         }

//         // Desktop testing - hold mouse or space
// #if UNITY_EDITOR || UNITY_STANDALONE
//         if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
//         {
//             isTouching = true;
//         }
// #endif
//     }

//     void HandleMovement()
//     {
//         // Stop if reached end and not looping
//         if (reachedEnd && !loopSpline)
//         {
//             currentSpeed = 0f;
//             return;
//         }

//         // Accelerate when touching, decelerate when not
//         if (isTouching)
//         {
//             currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
//         }
//         else
//         {
//             currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
//         }

//         if (currentSpeed > 0.01f)
//         {
//             // Move along spline
//             float speedOnSpline = currentSpeed / totalSplineLength;
//             splineProgress += speedOnSpline * Time.deltaTime;

//             // Handle end of spline
//             if (splineProgress >= 1f)
//             {
//                 if (loopSpline)
//                 {
//                     // Loop back to start
//                     splineProgress -= 1f;
//                     reachedEnd = false;
//                 }
//                 else
//                 {
//                     // Stop at end
//                     splineProgress = 1f;
//                     reachedEnd = true;
//                     currentSpeed = 0f;
//                 }
//             }

//             // Get position on spline
//             Vector3 posOnSpline = splineContainer.EvaluatePosition(splineProgress);
//             transform.position = new Vector3(posOnSpline.x, transform.position.y, posOnSpline.z);

//             // Get direction - NO ROTATION HERE, drift handles it all
//             // We only update position on spline, rotation is handled by drift
//         }
//     }


//     void HandleDrift()
//     {
//         // Get current spline direction
//         Vector3 currentTangent = splineContainer.EvaluateTangent(splineProgress);
//         currentTangent.y = 0;
//         currentTangent.Normalize();

//         // Calculate turn sharpness by looking ahead
//         float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
//         Vector3 futureTangent = splineContainer.EvaluateTangent(lookAhead);
//         futureTangent.y = 0;
//         futureTangent.Normalize();

//         // Calculate angle difference (turn sharpness)
//         float angleChange = Vector3.SignedAngle(currentTangent, futureTangent, Vector3.up);

//         // MAIN CAR ROTATION - Always follow spline direction
//         if (currentTangent != Vector3.zero)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(currentTangent);
//             float rotSpeed = rotationSpeed * Mathf.Clamp01(currentSpeed / maxSpeed);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
//         }

//         // Calculate VISUAL drift based on speed and turn sharpness
//         float targetDrift = 0f;

//         if (currentSpeed > driftThreshold)
//         {
//             // Speed factor for drift intensity
//             float speedFactor = Mathf.Clamp01((currentSpeed - driftThreshold) / (maxSpeed - driftThreshold));

//             // Turn sharpness with better scaling
//             float turnSharpness = angleChange / (lookAheadDistance * 50f); // Adjusted scale

//             // Calculate target drift angle
//             targetDrift = turnSharpness * speedFactor * maxDriftAngle;
//             targetDrift = Mathf.Clamp(targetDrift, -maxDriftAngle, maxDriftAngle);
//         }

//         // Smooth drift angle transition
//         currentDriftAngle = Mathf.Lerp(currentDriftAngle, targetDrift, Time.deltaTime * driftSmoothness);

//         // Apply drift to visual ONLY (not the main car transform)
//         if (carVisual != null && carVisual != transform)
//         {
//             carVisual.localRotation = Quaternion.Euler(0, currentDriftAngle, 0);
//         }
//     }

//     void HandleDriftParticles()
//     {
//         if (driftParticles != null)
//         {
//             // Enable particles when drifting
//             if (Mathf.Abs(currentDriftAngle) > 5f && currentSpeed > driftThreshold)
//             {
//                 if (!driftParticles.isPlaying)
//                     driftParticles.Play();
//             }
//             else
//             {
//                 if (driftParticles.isPlaying)
//                     driftParticles.Stop();
//             }
//         }
//     }

//     // Visualize the spline path in editor
//     void OnDrawGizmos()
//     {
//         if (splineContainer != null && splineContainer.Spline != null)
//         {
//             Gizmos.color = Color.yellow;
//             int segments = 50;
//             for (int i = 0; i <= segments; i++)
//             {
//                 float t = (float)i / segments;
//                 Vector3 pos = splineContainer.EvaluatePosition(t);
//                 Gizmos.DrawSphere(pos, 0.2f);

//                 if (i > 0)
//                 {
//                     float prevT = (float)(i - 1) / segments;
//                     Vector3 prevPos = splineContainer.EvaluatePosition(prevT);
//                     Gizmos.DrawLine(prevPos, pos);
//                 }
//             }

//             // Draw look ahead point
//             if (Application.isPlaying)
//             {
//                 Gizmos.color = Color.red;
//                 float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
//                 Vector3 lookAheadPos = splineContainer.EvaluatePosition(lookAhead);
//                 Gizmos.DrawSphere(lookAheadPos, 0.3f);
//             }
//         }
//     }
// }
