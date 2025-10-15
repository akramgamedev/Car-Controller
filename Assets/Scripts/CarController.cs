using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Movement Settings")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;

    [Header("Turn Settings")]
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField, Range(0f, 1f)] private float baseDriftFactor = 0.92f;
    [SerializeField] private float turnSmoothness = 5f;

    [Header("Drift Settings")]
    [SerializeField] private float driftAcceleration = 15f;
    [SerializeField] private float driftDeceleration = 8f;
    [SerializeField] private float driftTurnSpeed = 220f;
    [SerializeField, Range(0f, 1f)] private float driftSlipAmount = 0.8f;
    [SerializeField] private float driftRecoveryRate = 4f;
    [SerializeField] private float driftBoost = 1.25f;

    [Header("Visual Settings")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 3f;

    // Internal
    private float currentSpeed;
    private float targetSpeed;
    private float currentTurnAngle;
    private float targetTurnAngle;
    private float driftFactor;
    private bool isAccelerating;
    private bool isDrifting;
    private float driftIntensity;
    private Vector3 moveDirection;
    private Vector3 velocity;
    private Vector3 driftVelocity;

    private float externalTurnInput;
    private bool isTurnTriggerActive;

    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
        moveDirection = cachedTransform.forward;
    }

    void Update()
    {
        HandleTouchInput();
        UpdateMovement();
        UpdateRotation();
    }

    private void HandleTouchInput()
    {
        isAccelerating = Input.touchCount > 0 || Input.GetMouseButton(0);
        targetSpeed = isAccelerating ? maxSpeed : 0f;
    }

    private void UpdateMovement()
    {
        float rate = isAccelerating ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // Dynamic drift factor: higher when turning fast or at high speed
        float speedPercent = Mathf.Clamp01(currentSpeed / maxSpeed);
        float driftStrength = Mathf.Abs(externalTurnInput) * speedPercent;
        driftFactor = Mathf.Lerp(baseDriftFactor, 0.7f, driftStrength); // lower = more slide

        // Blend direction for smooth top-down drift
        moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, driftFactor * Time.deltaTime * 8f);

        // Apply velocity
        float driftMultiplier = isDrifting ? driftBoost : 1f;
        velocity = moveDirection.normalized * currentSpeed * driftMultiplier;

        // Add side slip for better feel
        if (isDrifting)
            velocity += driftVelocity;

        cachedTransform.position += velocity * Time.deltaTime;

        // Gradually recover from drift
        if (isDrifting)
            driftVelocity = Vector3.Lerp(driftVelocity, Vector3.zero, Time.deltaTime * driftRecoveryRate);
    }

    private void UpdateRotation()
    {
        if (isTurnTriggerActive) return;

        float turnIntensity = Mathf.Abs(externalTurnInput);
        bool shouldDrift = turnIntensity > 0.1f && currentSpeed > 2f;

        if (shouldDrift && !isDrifting)
        {
            StartDrift(turnIntensity);
        }
        else if (!shouldDrift)
        {  
            isDrifting = false;
        }

        // Apply turning
        float currentTurnSpeed = isDrifting ? driftTurnSpeed : turnSpeed;
        float turnAmount = externalTurnInput * currentTurnSpeed * Time.deltaTime;
        cachedTransform.Rotate(0f, turnAmount, 0f);

        // Adjust drift velocity sideways
        if (isDrifting)
        {
            Vector3 sideways = cachedTransform.right * Mathf.Sign(externalTurnInput);
            driftVelocity = Vector3.Lerp(driftVelocity, sideways * currentSpeed * driftSlipAmount * 0.5f, Time.deltaTime * 2f);
        }

        // Visual tilt (for feedback)
        float targetTilt = -externalTurnInput * tiltAngle;
        Vector3 rot = cachedTransform.localEulerAngles;
        rot.z = Mathf.LerpAngle(rot.z, targetTilt, tiltSpeed * Time.deltaTime);
        cachedTransform.localEulerAngles = rot;
    }

    private void StartDrift(float intensity)
    {
        isDrifting = true;
        driftIntensity = intensity;
    }

    public void SetTurnInput(float turnInput)
    {
        externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
        targetTurnAngle = externalTurnInput;
    }

    public void SetTurnTriggerActive(bool active)
    {
        isTurnTriggerActive = active;
        if (!active)
        {
            externalTurnInput = 0f;
            moveDirection = cachedTransform.forward;
        }
    }

    public void ForceRotation(Quaternion targetRotation)
    {
        cachedTransform.rotation = targetRotation;
        moveDirection = cachedTransform.forward;
    }

    public void ResetTilt()
    {
        Vector3 rot = cachedTransform.localEulerAngles;
        rot.z = 0f;
        cachedTransform.localEulerAngles = rot;
    }

    // Keep TriggerDrift to match TurnTrigger usage
    public void TriggerDrift(float turnIntensity)
    {
        StartDrift(Mathf.Abs(turnIntensity));
    }

    // --- Added utility methods to satisfy TurnTrigger and other systems ---
    public bool IsDrifting() => isDrifting;

    // TurnTrigger expects IsMoving(); return true when we have non-trivial forward speed
    public bool IsMoving() => currentSpeed > 0.1f;

    // Optional helpers
    public float GetCurrentSpeed() => currentSpeed;
    public float GetNormalizedSpeed() => Mathf.Clamp01(currentSpeed / maxSpeed);
}





// using UnityEngine;

// public class CarController : MonoBehaviour
// {
//     [Header("Car Movement Settings")]
//     [SerializeField] private float maxSpeed = 15f;
//     [SerializeField] private float acceleration = 8f;
//     [SerializeField] private float deceleration = 12f;

//     [Header("Turn Settings")]
//     [SerializeField] private float turnSpeed = 120f;
//     [SerializeField] private float driftFactor = 0.9f;
//     [SerializeField] private float turnSmoothness = 5f;

//     [Header("Drift Settings")]
//     [SerializeField] private float driftAcceleration = 15f;
//     [SerializeField] private float driftDeceleration = 8f;
//     [SerializeField] private float driftTurnSpeed = 200f; // Faster turning during drift
//     [SerializeField] private float driftSlipAmount = 0.7f; // How much car slides sideways (0-1)
//     [SerializeField] private float driftDuration = 0.5f; // How long drift lasts
//     [SerializeField] private float driftSpeedBoost = 1.2f; // Speed multiplier during drift

//     [Header("Visual Settings")]
//     [SerializeField] private float tiltAngle = 15f;
//     [SerializeField] private float tiltSpeed = 3f;

//     //Internal State
//     private float currentSpeed;
//     private float targetSpeed;
//     private float currentTurnAngle;
//     private float targetTurnAngle;
//     private bool isAccelerating;
//     private Vector3 moveDirection;
//     private Vector3 velocity;
//     private Vector3 driftVelocity;

//     //Cached components for performance
//     private Transform cachedTransform;

//     //Turn direction (will be set by waypoint system later)
//     private float externalTurnInput;

//     // Drift system
//     private bool isDrifting;
//     private float driftTimer;
//     private Vector3 driftDirection;
//     private float driftIntensity;

//     // Flag to disable rotation when turn trigger is active
//     private bool isTurnTriggerActive;

//     void Awake()
//     {
//         cachedTransform = transform;
//         moveDirection = cachedTransform.forward;
//         driftVelocity = Vector3.zero;
//     }

//     void Update()
//     {
//         HandleTouchInput();
//         UpdateMovement();
//         UpdateDrift();
//         if (!isTurnTriggerActive)
//         {
//             UpdateRotation();
//         }
//     }

//     private void HandleTouchInput()
//     {
//         if (Input.touchCount > 0)
//         {
//             Touch touch = Input.GetTouch(0);
//             if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
//             {
//                 isAccelerating = true;
//             }
//             else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//             {
//                 isAccelerating = false;
//             }
//         }
//         else if (Input.GetMouseButton(0))
//         {
//             isAccelerating = true;
//         }
//         else
//         {
//             isAccelerating = false;
//         }

//         targetSpeed = isAccelerating ? maxSpeed : 0f;
//     }

//     private void UpdateMovement()
//     {
//         float speedChangeRate = isAccelerating ? acceleration : deceleration;
//         currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

//         // Apply drift effect during turns
//         if (Mathf.Abs(externalTurnInput) > 0.1f && !isTurnTriggerActive && currentSpeed > 2f)
//         {
//             // Initiate drift
//             if (!isDrifting)
//             {
//                 StartDrift();
//             }

//             // Blend between forward direction and actual velocity for drift effect
//             moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, driftFactor * Time.deltaTime * 10f);
//         }
//         else
//         {
//             // Align movement with forward direction when not turning
//             moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, Time.deltaTime * 5f);
//         }

//         // Calculate velocity
//         float speedMultiplier = isDrifting ? driftSpeedBoost : 1f;
//         velocity = moveDirection.normalized * currentSpeed * speedMultiplier;

//         // Add drift velocity (sideways movement)
//         if (isDrifting)
//         {
//             velocity += driftVelocity;
//         }

//         // Move the car
//         cachedTransform.position += velocity * Time.deltaTime;
//     }

//     /// <summary>
//     /// Starts a drift when turning sharply
//     /// </summary>
//     private void StartDrift()
//     {
//         isDrifting = true;
//         driftTimer = driftDuration;
//         driftDirection = cachedTransform.right * Mathf.Sign(externalTurnInput);
//         driftIntensity = Mathf.Abs(externalTurnInput);
//     }

//     /// <summary>
//     /// Updates drift physics and visual effects
//     /// </summary>
//     private void UpdateDrift()
//     {
//         if (!isDrifting)
//             return;

//         driftTimer -= Time.deltaTime;

//         if (driftTimer <= 0)
//         {
//             // End drift
//             isDrifting = false;
//             driftVelocity = Vector3.Lerp(driftVelocity, Vector3.zero, Time.deltaTime * 5f);
//             return;
//         }

//         // Calculate drift velocity (sideways slip)
//         float driftSlip = driftIntensity * driftSlipAmount * currentSpeed * 0.5f;
//         driftVelocity = driftDirection * driftSlip;

//         // Smooth drift velocity fade
//         float driftFade = driftTimer / driftDuration;
//         driftVelocity *= driftFade;
//     }

//     private void UpdateRotation()
//     {
//         currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurnAngle, turnSmoothness * Time.deltaTime);

//         if (currentSpeed > 0.1f)
//         {
//             // Use faster turn speed during drift
//             float currentTurnSpeed = isDrifting ? driftTurnSpeed : turnSpeed;
//             float turnAmount = externalTurnInput * currentTurnSpeed * Time.deltaTime;
//             cachedTransform.Rotate(0f, turnAmount, 0f, Space.World);
//             moveDirection = cachedTransform.forward;
//         }

//         // Apply visual tilt for better feedback
//         float targetTilt = -externalTurnInput * tiltAngle;
//         Vector3 currentRotation = cachedTransform.localEulerAngles;
//         float newTilt = Mathf.LerpAngle(currentRotation.z, targetTilt, tiltSpeed * Time.deltaTime);
//         cachedTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, newTilt);
//     }

//     public void SetTurnInput(float turnInput)
//     {
//         externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
//         targetTurnAngle = turnInput;
//     }

//     // NEW: Tell car that turn trigger is taking over rotation
//     public void SetTurnTriggerActive(bool active)
//     {
//         isTurnTriggerActive = active;
//         if (!active)
//         {
//             externalTurnInput = 0f;
//             // Align movement direction with current forward when trigger releases
//             moveDirection = cachedTransform.forward;
//         }
//     }

//     // NEW: Direct rotation from turn trigger (no smoothing conflict)
//     public void ForceRotation(Quaternion targetRotation)
//     {
//         cachedTransform.rotation = targetRotation;
//         moveDirection = cachedTransform.forward;
//     }

//     // NEW: Reset tilt when trigger completes
//     public void ResetTilt()
//     {
//         Vector3 currentRotation = cachedTransform.localEulerAngles;
//         cachedTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);
//     }

//     /// <summary>
//     /// Trigger drift from external system (turn triggers)
//     /// </summary>
//     public void TriggerDrift(float turnIntensity)
//     {
//         if (currentSpeed > 2f && !isDrifting)
//         {
//             isDrifting = true;
//             driftTimer = driftDuration * 1.5f; // Longer drift for trigger turns
//             driftIntensity = Mathf.Abs(turnIntensity);
//             driftDirection = cachedTransform.right * Mathf.Sign(turnIntensity);
//         }
//     }

//     public float GetCurrentSpeed()
//     {
//         return currentSpeed;
//     }

//     public float GetNormalizedSpeed()
//     {
//         return currentSpeed / maxSpeed;
//     }

//     public bool IsMoving()
//     {
//         return currentSpeed > 0.1f;
//     }

//     public bool IsDrifting()
//     {
//         return isDrifting;
//     }

//     public void ForceStop()
//     {
//         targetSpeed = 0f;
//         currentSpeed = 0f;
//         isAccelerating = false;
//         isDrifting = false;
//         driftVelocity = Vector3.zero;
//     }

//     public void ResetCar()
//     {
//         currentSpeed = 0f;
//         targetSpeed = 0f;
//         currentTurnAngle = 0f;
//         targetTurnAngle = 0f;
//         externalTurnInput = 0f;
//         isAccelerating = false;
//         velocity = Vector3.zero;
//         moveDirection = cachedTransform.forward;
//         isDrifting = false;
//         driftVelocity = Vector3.zero;
//         isTurnTriggerActive = false;
//     }
// }
