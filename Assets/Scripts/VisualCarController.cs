using UnityEngine;

/// <summary>
/// Handles visual aspects of the car (wheels, body tilt, suspension)
/// Works alongside CarController for movement
/// Optimized for mobile performance
/// </summary>
public class VisualCarController : MonoBehaviour
{
    [Header("Wheel References")]
    [Tooltip("Assign your wheel GameObjects here")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("Wheel Rotation Settings")]
    [SerializeField] private float wheelRadius = 0.35f; // Adjust based on your wheel size
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float steerSpeed = 5f;

    [Header("Wheel Orientation Fix")]
    [Tooltip("If wheels rotate wrong, adjust these axes")]
    [SerializeField] private Vector3 wheelRotationAxis = new Vector3(1, 0, 0); // X-axis by default
    [SerializeField] private Vector3 wheelSteerAxis = new Vector3(0, 1, 0); // Y-axis by default
    [SerializeField] private bool invertRotation = false;

    [Header("Suspension Visual Effect")]
    [SerializeField] private bool enableSuspension = true;
    [SerializeField] private float suspensionHeight = 0.1f;
    [SerializeField] private float suspensionSpeed = 8f;

    [Header("Body Tilt (Already handled by CarController)")]
    [SerializeField] private bool useCarControllerTilt = true;
    [Tooltip("Additional body tilt on top of CarController tilt")]
    [SerializeField] private float additionalBodyTilt = 5f;

    [Header("Advanced")]
    [SerializeField] private bool autoDetectWheels = true;

    // Internal state
    private float currentSteerAngle;
    private float wheelRotationAngle;
    private CarController carController;

    // Store initial rotations to preserve them
    private Quaternion flInitialRotation;
    private Quaternion frInitialRotation;
    private Quaternion rlInitialRotation;
    private Quaternion rrInitialRotation;

    // Original wheel positions for suspension
    private Vector3 flWheelOriginalPos;
    private Vector3 frWheelOriginalPos;
    private Vector3 rlWheelOriginalPos;
    private Vector3 rrWheelOriginalPos;

    // Suspension offset
    private float[] wheelSuspensionOffset = new float[4];
    private float suspensionTime;

    void Awake()
    {
        // Get CarController reference
        carController = GetComponent<CarController>();

        if (carController == null)
        {
            Debug.LogError("VisualCarController requires CarController component!");
        }

        // Auto-detect wheels if enabled
        if (autoDetectWheels)
        {
            AutoDetectWheels();
        }

        // Store original wheel positions
        StoreOriginalWheelPositions();
    }

    void Update()
    {
        if (carController != null)
        {
            UpdateWheelRotation();
            UpdateWheelSteering();

            if (enableSuspension)
            {
                UpdateSuspensionEffect();
            }
        }
    }

    /// <summary>
    /// Automatically finds wheels in children based on common naming
    /// </summary>
    private void AutoDetectWheels()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            string name = child.name.ToLower();

            // Try to detect wheels by name
            if (name.Contains("wheel") || name.Contains("tire") || name.Contains("tyre"))
            {
                if ((name.Contains("front") || name.Contains("fl")) && (name.Contains("left") || name.Contains("l")))
                    frontLeftWheel = child;
                else if ((name.Contains("front") || name.Contains("fr")) && (name.Contains("right") || name.Contains("r")))
                    frontRightWheel = child;
                else if ((name.Contains("rear") || name.Contains("back") || name.Contains("rl")) && (name.Contains("left") || name.Contains("l")))
                    rearLeftWheel = child;
                else if ((name.Contains("rear") || name.Contains("back") || name.Contains("rr")) && (name.Contains("right") || name.Contains("r")))
                    rearRightWheel = child;
            }
        }

        // Log results
        if (frontLeftWheel != null) Debug.Log("Auto-detected Front Left Wheel: " + frontLeftWheel.name);
        if (frontRightWheel != null) Debug.Log("Auto-detected Front Right Wheel: " + frontRightWheel.name);
        if (rearLeftWheel != null) Debug.Log("Auto-detected Rear Left Wheel: " + rearLeftWheel.name);
        if (rearRightWheel != null) Debug.Log("Auto-detected Rear Right Wheel: " + rearRightWheel.name);
    }

    /// <summary>
    /// Store original wheel positions for suspension animation
    /// </summary>
    private void StoreOriginalWheelPositions()
    {
        if (frontLeftWheel != null)
        {
            flWheelOriginalPos = frontLeftWheel.localPosition;
            flInitialRotation = frontLeftWheel.localRotation;
        }
        if (frontRightWheel != null)
        {
            frWheelOriginalPos = frontRightWheel.localPosition;
            frInitialRotation = frontRightWheel.localRotation;
        }
        if (rearLeftWheel != null)
        {
            rlWheelOriginalPos = rearLeftWheel.localPosition;
            rlInitialRotation = rearLeftWheel.localRotation;
        }
        if (rearRightWheel != null)
        {
            rrWheelOriginalPos = rearRightWheel.localPosition;
            rrInitialRotation = rearRightWheel.localRotation;
        }
    }

    /// <summary>
    /// Rotates wheels based on car speed
    /// Preserves original wheel orientation and applies rotation on correct axis
    /// </summary>
    private void UpdateWheelRotation()
    {
        float speed = carController.GetCurrentSpeed();

        // Calculate rotation based on speed and wheel radius
        float rotationPerSecond = (speed / (2f * Mathf.PI * wheelRadius)) * 360f;

        // Invert if needed
        if (invertRotation) rotationPerSecond = -rotationPerSecond;

        wheelRotationAngle += rotationPerSecond * Time.deltaTime;

        // Keep angle in reasonable range
        if (wheelRotationAngle > 360f) wheelRotationAngle -= 360f;
        if (wheelRotationAngle < -360f) wheelRotationAngle += 360f;

        // Create rotation quaternions
        Quaternion spinRotation = Quaternion.AngleAxis(wheelRotationAngle, wheelRotationAxis);
        Quaternion steerRotation = Quaternion.AngleAxis(currentSteerAngle, wheelSteerAxis);

        // Apply rotation to wheels, preserving their initial orientation
        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = flInitialRotation * steerRotation * spinRotation;
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = frInitialRotation * steerRotation * spinRotation;
        }

        if (rearLeftWheel != null)
        {
            rearLeftWheel.localRotation = rlInitialRotation * spinRotation;
        }

        if (rearRightWheel != null)
        {
            rearRightWheel.localRotation = rrInitialRotation * spinRotation;
        }
    }

    /// <summary>
    /// Updates steering angle for front wheels
    /// Gets turn input from CarController
    /// </summary>
    private void UpdateWheelSteering()
    {
        // Get turn input from CarController's tilt (external turn input)
        // We'll use the car's local Z rotation as an indicator
        float turnInput = transform.localEulerAngles.z;

        // Normalize the angle to -180 to 180
        if (turnInput > 180f) turnInput -= 360f;

        // Convert tilt to steering angle
        float targetSteerAngle = Mathf.Clamp(turnInput * 2f, -maxSteerAngle, maxSteerAngle);

        // Smooth steering transition
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Creates suspension bounce effect for visual appeal
    /// Simulates suspension compression based on speed and turning
    /// </summary>
    private void UpdateSuspensionEffect()
    {
        suspensionTime += Time.deltaTime;

        float speed = carController.GetNormalizedSpeed();

        // Create subtle bounce effect based on speed
        float bounceFrequency = 3f + speed * 2f; // Faster = more bounce
        float bounceAmount = suspensionHeight * speed * 0.5f;

        // Different phase for each wheel for realistic effect
        wheelSuspensionOffset[0] = Mathf.Sin(suspensionTime * bounceFrequency) * bounceAmount;
        wheelSuspensionOffset[1] = Mathf.Sin(suspensionTime * bounceFrequency + 0.5f) * bounceAmount;
        wheelSuspensionOffset[2] = Mathf.Sin(suspensionTime * bounceFrequency + 1f) * bounceAmount;
        wheelSuspensionOffset[3] = Mathf.Sin(suspensionTime * bounceFrequency + 1.5f) * bounceAmount;

        // Apply suspension to wheels
        if (frontLeftWheel != null)
            frontLeftWheel.localPosition = flWheelOriginalPos + Vector3.up * wheelSuspensionOffset[0];

        if (frontRightWheel != null)
            frontRightWheel.localPosition = frWheelOriginalPos + Vector3.up * wheelSuspensionOffset[1];

        if (rearLeftWheel != null)
            rearLeftWheel.localPosition = rlWheelOriginalPos + Vector3.up * wheelSuspensionOffset[2];

        if (rearRightWheel != null)
            rearRightWheel.localPosition = rrWheelOriginalPos + Vector3.up * wheelSuspensionOffset[3];
    }

    /// <summary>
    /// Manually assign wheels if auto-detect doesn't work
    /// </summary>
    public void ManuallyAssignWheels(Transform fl, Transform fr, Transform rl, Transform rr)
    {
        frontLeftWheel = fl;
        frontRightWheel = fr;
        rearLeftWheel = rl;
        rearRightWheel = rr;

        StoreOriginalWheelPositions();
    }

    /// <summary>
    /// Reset suspension and wheel rotation
    /// </summary>
    public void ResetVisuals()
    {
        wheelRotationAngle = 0f;
        currentSteerAngle = 0f;
        suspensionTime = 0f;

        StoreOriginalWheelPositions();
    }
}