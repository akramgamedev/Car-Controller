using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Movement Settings")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;

    [Header("Turn Settings")]
    [SerializeField] private float turnSpeed = 100f;

    [Header("Drift Settings")]
    [SerializeField] private float driftAmount = 5f; // How much the car slides outward
    [SerializeField] private float driftRotationMultiplier = 2f; // How much faster car rotates during drift
    [SerializeField] private float minSpeedForDrift = 2f;
    
    [Header("Path Following")]
    [SerializeField] private float pathReturnSpeed = 3f; // How fast car returns to center path after drift
    [SerializeField] private float pathSmoothness = 0.85f; // How tightly car follows its forward direction (lower = more drift lag)

    [Header("Visual Settings")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 3f;

    // Internal
    private float currentSpeed;
    private float targetSpeed;
    private bool isAccelerating;
    private bool isDrifting;
    private bool wasDrifting;
    
    // Movement vectors
    private Vector3 forwardDirection; // The direction car should travel (path direction)
    private Vector3 velocity; // Current actual velocity with drift
    private float lateralOffset; // How far off-center from path
    
    private float externalTurnInput;
    private bool isTurnTriggerActive;

    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
        forwardDirection = cachedTransform.forward;
        velocity = Vector3.zero;
        lateralOffset = 0f;
    }

    void Update()
    {
        HandleTouchInput();
        UpdateMovement();
        ApplyVisualTilt();
    }

    private void HandleTouchInput()
    {
        isAccelerating = Input.touchCount > 0 || Input.GetMouseButton(0);
        targetSpeed = isAccelerating ? maxSpeed : 0f;
    }

    private void UpdateMovement()
    {
        // Update speed
        float rate = isAccelerating ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // Check if we should drift
        bool isTurning = Mathf.Abs(externalTurnInput) > 0.15f;
        bool canDrift = currentSpeed > minSpeedForDrift && isTurning && isAccelerating;
        
        wasDrifting = isDrifting;
        isDrifting = canDrift;

        // Update forward direction (the intended path)
        forwardDirection = cachedTransform.forward;

        // Rotate car if not in trigger
        if (!isTurnTriggerActive && Mathf.Abs(externalTurnInput) > 0.01f)
        {
            // Apply extra rotation during drift
            float rotationMultiplier = isDrifting ? driftRotationMultiplier : 1f;
            float turnAmount = externalTurnInput * turnSpeed * rotationMultiplier * Time.deltaTime;
            cachedTransform.Rotate(0f, turnAmount, 0f);
        }

        // Calculate velocity
        if (isDrifting)
        {
            // During drift: move forward but also slide outward
            velocity = forwardDirection * currentSpeed;
            
            // Add outward drift force
            float driftForce = externalTurnInput * driftAmount;
            lateralOffset += driftForce * Time.deltaTime;
            
            // Apply lateral offset as sideways velocity
            velocity += cachedTransform.right * lateralOffset;
        }
        else
        {
            // Not drifting: return to center path
            velocity = forwardDirection * currentSpeed;
            
            // Gradually reduce lateral offset (return to center)
            lateralOffset = Mathf.Lerp(lateralOffset, 0f, pathReturnSpeed * Time.deltaTime);
            
            // Apply remaining lateral offset
            velocity += cachedTransform.right * lateralOffset;
        }

        // Blend velocity to follow car's rotation smoothly
        velocity = Vector3.Lerp(velocity, forwardDirection * currentSpeed, pathSmoothness * Time.deltaTime);

        // Lock Y position and apply movement
        float currentY = cachedTransform.position.y;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 newPosition = cachedTransform.position + horizontalVelocity * Time.deltaTime;
        newPosition.y = currentY;
        
        cachedTransform.position = newPosition;
    }

    private void ApplyVisualTilt()
    {
        // Visual tilt for feedback
        float targetTilt = -externalTurnInput * tiltAngle;
        Vector3 euler = cachedTransform.localEulerAngles;
        euler.z = Mathf.LerpAngle(euler.z, targetTilt, tiltSpeed * Time.deltaTime);
        cachedTransform.localEulerAngles = euler;
    }

    public void SetTurnInput(float turnInput)
    {
        externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
    }

    public void SetTurnTriggerActive(bool active)
    {
        isTurnTriggerActive = active;
        if (!active)
        {
            externalTurnInput = 0f;
        }
    }

    public void ForceRotation(Quaternion targetRotation)
    {
        cachedTransform.rotation = targetRotation;
        forwardDirection = cachedTransform.forward;
    }

    public void ResetTilt()
    {
        Vector3 rot = cachedTransform.localEulerAngles;
        rot.z = 0f;
        cachedTransform.localEulerAngles = rot;
    }

    public void TriggerDrift(float turnIntensity)
    {
        isDrifting = true;
    }

    // Utility methods
    public bool IsDrifting() => isDrifting;
    public bool IsMoving() => currentSpeed > 0.1f;
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
//     [SerializeField] private float turnSpeed = 100f;

//     [Header("Drift Settings")]
//     [SerializeField] private float driftAmount = 3.5f; // How much the car slides sideways
//     [SerializeField] private float forwardDriftGrip = 0.88f; // How much car follows its rotation (lower = more drift)
//     [SerializeField] private float driftRotationMultiplier = 1.5f; // How much faster car rotates during drift
//     [SerializeField] private float driftRecoverySpeed = 2.5f;
//     [SerializeField] private float minSpeedForDrift = 2f;

//     [Header("Visual Settings")]
//     [SerializeField] private float tiltAngle = 15f;
//     [SerializeField] private float tiltSpeed = 3f;

//     // Internal
//     private float currentSpeed;
//     private float targetSpeed;
//     private bool isAccelerating;
//     private bool isDrifting;
    
//     // Drift system
//     private Vector3 velocity; // Actual velocity of the car
//     private Vector3 smoothVelocity; // For smoothing
    
//     private float externalTurnInput;
//     private bool isTurnTriggerActive;

//     private Transform cachedTransform;

//     void Awake()
//     {
//         cachedTransform = transform;
//         velocity = cachedTransform.forward * 0.01f;
//     }

//     void Update()
//     {
//         HandleTouchInput();
//         UpdateMovement();
//         ApplyVisualTilt();
//     }

//     private void HandleTouchInput()
//     {
//         isAccelerating = Input.touchCount > 0 || Input.GetMouseButton(0);
//         targetSpeed = isAccelerating ? maxSpeed : 0f;
//     }

//     private void UpdateMovement()
//     {
//         // Update speed
//         float rate = isAccelerating ? acceleration : deceleration;
//         currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

//         // Rotate car if not in trigger
//         if (!isTurnTriggerActive && Mathf.Abs(externalTurnInput) > 0.01f)
//         {
//             // Apply extra rotation during drift
//             float rotationMultiplier = isDrifting ? driftRotationMultiplier : 1f;
//             float turnAmount = externalTurnInput * turnSpeed * rotationMultiplier * Time.deltaTime;
//             cachedTransform.Rotate(0f, turnAmount, 0f);
//         }

//         // Check if we should drift
//         bool isTurning = Mathf.Abs(externalTurnInput) > 0.15f;
//         bool canDrift = currentSpeed > minSpeedForDrift && isTurning && isAccelerating;
        
//         isDrifting = canDrift;

//         // Calculate target velocity based on car's forward direction
//         Vector3 targetVelocity = cachedTransform.forward * currentSpeed;

//         if (isDrifting)
//         {
//             // During drift: velocity lags behind the car's rotation
//             // This creates the "sliding" effect
//             velocity = Vector3.Lerp(velocity, targetVelocity, forwardDriftGrip * Time.deltaTime * 5f);
            
//             // Add perpendicular drift (sideways slide)
//             Vector3 driftForce = cachedTransform.right * externalTurnInput * driftAmount;
//             velocity += driftForce * Time.deltaTime;
//         }
//         else
//         {
//             // Not drifting: quickly align velocity with car direction
//             velocity = Vector3.Lerp(velocity, targetVelocity, driftRecoverySpeed * Time.deltaTime);
//         }

//         // IMPORTANT: Lock Y position to prevent ground clipping
//         float currentY = cachedTransform.position.y;
        
//         // Apply movement (only on X and Z axis)
//         Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
//         Vector3 newPosition = cachedTransform.position + horizontalVelocity * Time.deltaTime;
//         newPosition.y = currentY; // Keep Y position fixed
        
//         cachedTransform.position = newPosition;
//     }

//     private void ApplyVisualTilt()
//     {
//         // Visual tilt for feedback
//         float targetTilt = -externalTurnInput * tiltAngle;
//         Vector3 euler = cachedTransform.localEulerAngles;
//         euler.z = Mathf.LerpAngle(euler.z, targetTilt, tiltSpeed * Time.deltaTime);
//         cachedTransform.localEulerAngles = euler;
//     }

//     public void SetTurnInput(float turnInput)
//     {
//         externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
//     }

//     public void SetTurnTriggerActive(bool active)
//     {
//         isTurnTriggerActive = active;
//         if (!active)
//         {
//             externalTurnInput = 0f;
//         }
//     }

//     // DO NOT reset velocity here - let drift continue naturally
//     public void ForceRotation(Quaternion targetRotation)
//     {
//         cachedTransform.rotation = targetRotation;
//         // Keep velocity as-is to maintain drift momentum
//     }

//     public void ResetTilt()
//     {
//         Vector3 rot = cachedTransform.localEulerAngles;
//         rot.z = 0f;
//         cachedTransform.localEulerAngles = rot;
//     }

//     public void TriggerDrift(float turnIntensity)
//     {
//         isDrifting = true;
//     }

//     // Utility methods
//     public bool IsDrifting() => isDrifting;
//     public bool IsMoving() => currentSpeed > 0.1f;
//     public float GetCurrentSpeed() => currentSpeed;
//     public float GetNormalizedSpeed() => Mathf.Clamp01(currentSpeed / maxSpeed);
// }




//********** original code ***********************

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
