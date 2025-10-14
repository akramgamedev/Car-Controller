using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Movement Settings")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;

    [Header("Turn Settings")]
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float driftFactor = 0.9f;
    [SerializeField] private float turnSmoothness = 5f;

    [Header("Drift Settings")]
    [SerializeField] private float driftAcceleration = 15f;
    [SerializeField] private float driftDeceleration = 8f;
    [SerializeField] private float driftTurnSpeed = 200f; // Faster turning during drift
    [SerializeField] private float driftSlipAmount = 0.7f; // How much car slides sideways (0-1)
    [SerializeField] private float driftDuration = 0.5f; // How long drift lasts
    [SerializeField] private float driftSpeedBoost = 1.2f; // Speed multiplier during drift

    [Header("Visual Settings")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 3f;

    //Internal State
    private float currentSpeed;
    private float targetSpeed;
    private float currentTurnAngle;
    private float targetTurnAngle;
    private bool isAccelerating;
    private Vector3 moveDirection;
    private Vector3 velocity;
    private Vector3 driftVelocity;

    //Cached components for performance
    private Transform cachedTransform;

    //Turn direction (will be set by waypoint system later)
    private float externalTurnInput;

    // Drift system
    private bool isDrifting;
    private float driftTimer;
    private Vector3 driftDirection;
    private float driftIntensity;

    // Flag to disable rotation when turn trigger is active
    private bool isTurnTriggerActive;

    void Awake()
    {
        cachedTransform = transform;
        moveDirection = cachedTransform.forward;
        driftVelocity = Vector3.zero;
    }

    void Update()
    {
        HandleTouchInput();
        UpdateMovement();
        UpdateDrift();
        if (!isTurnTriggerActive)
        {
            UpdateRotation();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                isAccelerating = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isAccelerating = false;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            isAccelerating = true;
        }
        else
        {
            isAccelerating = false;
        }

        targetSpeed = isAccelerating ? maxSpeed : 0f;
    }

    private void UpdateMovement()
    {
        float speedChangeRate = isAccelerating ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

        // Apply drift effect during turns
        if (Mathf.Abs(externalTurnInput) > 0.1f && !isTurnTriggerActive && currentSpeed > 2f)
        {
            // Initiate drift
            if (!isDrifting)
            {
                StartDrift();
            }

            // Blend between forward direction and actual velocity for drift effect
            moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, driftFactor * Time.deltaTime * 10f);
        }
        else
        {
            // Align movement with forward direction when not turning
            moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, Time.deltaTime * 5f);
        }

        // Calculate velocity
        float speedMultiplier = isDrifting ? driftSpeedBoost : 1f;
        velocity = moveDirection.normalized * currentSpeed * speedMultiplier;

        // Add drift velocity (sideways movement)
        if (isDrifting)
        {
            velocity += driftVelocity;
        }

        // Move the car
        cachedTransform.position += velocity * Time.deltaTime;
    }

    /// <summary>
    /// Starts a drift when turning sharply
    /// </summary>
    private void StartDrift()
    {
        isDrifting = true;
        driftTimer = driftDuration;
        driftDirection = cachedTransform.right * Mathf.Sign(externalTurnInput);
        driftIntensity = Mathf.Abs(externalTurnInput);
    }

    /// <summary>
    /// Updates drift physics and visual effects
    /// </summary>
    private void UpdateDrift()
    {
        if (!isDrifting)
            return;

        driftTimer -= Time.deltaTime;

        if (driftTimer <= 0)
        {
            // End drift
            isDrifting = false;
            driftVelocity = Vector3.Lerp(driftVelocity, Vector3.zero, Time.deltaTime * 5f);
            return;
        }

        // Calculate drift velocity (sideways slip)
        float driftSlip = driftIntensity * driftSlipAmount * currentSpeed * 0.5f;
        driftVelocity = driftDirection * driftSlip;

        // Smooth drift velocity fade
        float driftFade = driftTimer / driftDuration;
        driftVelocity *= driftFade;
    }

    private void UpdateRotation()
    {
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurnAngle, turnSmoothness * Time.deltaTime);

        if (currentSpeed > 0.1f)
        {
            // Use faster turn speed during drift
            float currentTurnSpeed = isDrifting ? driftTurnSpeed : turnSpeed;
            float turnAmount = externalTurnInput * currentTurnSpeed * Time.deltaTime;
            cachedTransform.Rotate(0f, turnAmount, 0f, Space.World);
            moveDirection = cachedTransform.forward;
        }

        // Apply visual tilt for better feedback
        float targetTilt = -externalTurnInput * tiltAngle;
        Vector3 currentRotation = cachedTransform.localEulerAngles;
        float newTilt = Mathf.LerpAngle(currentRotation.z, targetTilt, tiltSpeed * Time.deltaTime);
        cachedTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, newTilt);
    }

    public void SetTurnInput(float turnInput)
    {
        externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
        targetTurnAngle = turnInput;
    }

    // NEW: Tell car that turn trigger is taking over rotation
    public void SetTurnTriggerActive(bool active)
    {
        isTurnTriggerActive = active;
        if (!active)
        {
            externalTurnInput = 0f;
            // Align movement direction with current forward when trigger releases
            moveDirection = cachedTransform.forward;
        }
    }

    // NEW: Direct rotation from turn trigger (no smoothing conflict)
    public void ForceRotation(Quaternion targetRotation)
    {
        cachedTransform.rotation = targetRotation;
        moveDirection = cachedTransform.forward;
    }

    // NEW: Reset tilt when trigger completes
    public void ResetTilt()
    {
        Vector3 currentRotation = cachedTransform.localEulerAngles;
        cachedTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);
    }

    /// <summary>
    /// Trigger drift from external system (turn triggers)
    /// </summary>
    public void TriggerDrift(float turnIntensity)
    {
        if (currentSpeed > 2f && !isDrifting)
        {
            isDrifting = true;
            driftTimer = driftDuration * 1.5f; // Longer drift for trigger turns
            driftIntensity = Mathf.Abs(turnIntensity);
            driftDirection = cachedTransform.right * Mathf.Sign(turnIntensity);
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetNormalizedSpeed()
    {
        return currentSpeed / maxSpeed;
    }

    public bool IsMoving()
    {
        return currentSpeed > 0.1f;
    }

    public bool IsDrifting()
    {
        return isDrifting;
    }

    public void ForceStop()
    {
        targetSpeed = 0f;
        currentSpeed = 0f;
        isAccelerating = false;
        isDrifting = false;
        driftVelocity = Vector3.zero;
    }

    public void ResetCar()
    {
        currentSpeed = 0f;
        targetSpeed = 0f;
        currentTurnAngle = 0f;
        targetTurnAngle = 0f;
        externalTurnInput = 0f;
        isAccelerating = false;
        velocity = Vector3.zero;
        moveDirection = cachedTransform.forward;
        isDrifting = false;
        driftVelocity = Vector3.zero;
        isTurnTriggerActive = false;
    }
}


