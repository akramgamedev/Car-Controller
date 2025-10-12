using System.Runtime.CompilerServices;
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

    //Cached components for performance
    private Transform cachedTransform;

    //Turn direction (will be set by waypoint system later)
    private float externalTurnInput; // -1 to 1, set by path system

    void Awake()
    {
        // Cache transform to avoid GetComponent calls
        cachedTransform = transform;
        moveDirection = cachedTransform.forward;
    }

    void Update()
    {
        HandleTouchInput();
        UpdateMovement();
        UpdateRotation();
    }

    ///Summary>
    /// handles touch input for mobile devices
    /// optimzed to work with single or multi touch
    /// </Summary>


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
        // Fallback for Unity Editor testing with mouse
        else if (Input.GetMouseButton(0))
        {
            isAccelerating = true;
        }
        else
        {
            isAccelerating = false;
        }

        // Set target speed based on input
        targetSpeed = isAccelerating ? maxSpeed : 0f;
    }

    /// <summary>
    /// Updates car movement with smooth acceleration/deceleration
    /// Uses velocity-based movement for better control
    /// </summary>

    private void UpdateMovement()
    {
        // Smooth speed transition
        float speedChangeRate = isAccelerating ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

        // Apply drift effect during turns
        if (Mathf.Abs(externalTurnInput) > 0.1f)
        {
            // Blend between forward direction and actual velocity for drift effect
            moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, driftFactor * Time.deltaTime * 10f);
        }
        else
        {
            // Align movement with forward direction when not turning
            moveDirection = Vector3.Lerp(moveDirection, cachedTransform.forward, Time.deltaTime * 5f);
        }

        // Calculate velocity
        velocity = moveDirection.normalized * currentSpeed;

        // Move the car
        cachedTransform.position += velocity * Time.deltaTime;
    }

    /// <summary>
    /// Updates car rotation with smooth turning and visual tilt
    /// Separated for clarity and easy modification
    /// </summary>
    private void UpdateRotation()
    {
        // Smooth turn angle transition
        currentTurnAngle = Mathf.Lerp(currentTurnAngle, targetTurnAngle, turnSmoothness * Time.deltaTime);

        // Apply turning rotation
        if (currentSpeed > 0.1f) // Only turn when moving
        {
            float turnAmount = externalTurnInput * turnSpeed * Time.deltaTime;
            cachedTransform.Rotate(0f, turnAmount, 0f, Space.World);

            // Update movement direction
            moveDirection = cachedTransform.forward;
        }

        // Apply visual tilt for better feedback
        float targetTilt = -externalTurnInput * tiltAngle;
        Vector3 currentRotation = cachedTransform.localEulerAngles;
        float newTilt = Mathf.LerpAngle(currentRotation.z, targetTilt, tiltSpeed * Time.deltaTime);
        cachedTransform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, newTilt);
    }

    /// <summary>
    /// Public method for external systems (waypoints, AI) to control turning
    /// Call this from your path/waypoint detection system
    /// </summary>
    /// <param name="turnInput">Turn direction: -1 (left) to 1 (right)</param>

    public void SetTurnInput(float turnInput)
    {
        externalTurnInput = Mathf.Clamp(turnInput, -1f, 1f);
        targetTurnAngle = turnInput;
    }

    /// <summary>
    /// Get current speed (useful for UI, effects, etc.)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// Get normalized speed (0-1) for effects scaling
    /// </summary>
    public float GetNormalizedSpeed()
    {
        return currentSpeed / maxSpeed;
    }

    /// <summary>
    /// Check if car is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return currentSpeed > 0.1f;
    }

    /// <summary>
    /// Force stop the car (for crashes, finish line, etc.)
    /// </summary>
    public void ForceStop()
    {
        targetSpeed = 0f;
        currentSpeed = 0f;
        isAccelerating = false;
    }

    /// <summary>
    /// Reset car to initial state
    /// </summary>
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
    }




}
