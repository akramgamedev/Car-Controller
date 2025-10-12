using UnityEngine;

/// <summary>
/// Turn trigger system that rotates the car to face a new direction
/// The car smoothly transitions to the target direction when entering the trigger
/// Once aligned, it continues straight in the new direction
/// </summary>
public class TurnTrigger : MonoBehaviour
{
    [Header("Turn Settings")]
    [Tooltip("The direction the car should face after passing through this trigger")]
    [SerializeField] private Vector3 targetDirection = Vector3.forward;

    [Tooltip("How quickly the car rotates to face the target direction")]
    [SerializeField] private float rotationSpeed = 180f; // degrees per second

    [Header("Trigger Settings")]
    [SerializeField] private float triggerWidth = 5f;
    [SerializeField] private float triggerHeight = 3f;
    [SerializeField] private float triggerLength = 10f;

    [Header("Visual Helper")]
    [Tooltip("Use this to set turn direction easily: 0=Forward, 90=Right, -90=Left, 180=Backward")]
    [SerializeField] private float targetAngle = 90f; // Helper to visualize direction

    [Header("Advanced")]
    [SerializeField] private bool requiresMovement = true;
    [SerializeField] private float alignmentThreshold = 5f; // Degrees - when close enough, snap to target

    private BoxCollider triggerCollider;
    private CarController activeCar;
    private Transform carTransform;
    private bool isCarInTrigger;
    private bool hasCompletedTurn;
    private Quaternion targetRotation;

    void Awake()
    {
        SetupTrigger();
        UpdateTargetRotation();
    }

    void OnValidate()
    {
        // Update target direction when angle changes in inspector
        UpdateTargetRotation();
    }

    /// <summary>
    /// Sets up the trigger collider automatically
    /// </summary>
    private void SetupTrigger()
    {
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }

        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(triggerWidth, triggerHeight, triggerLength);
    }

    /// <summary>
    /// Updates the target rotation based on the helper angle
    /// </summary>
    private void UpdateTargetRotation()
    {
        // Convert the angle to a world-space rotation
        targetRotation = transform.rotation * Quaternion.Euler(0f, targetAngle, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
        {
            activeCar = car;
            carTransform = other.transform;
            isCarInTrigger = true;
            hasCompletedTurn = false;

            UpdateTargetRotation(); // Ensure we have the latest target
        }
    }

    void Update()
    {
        if (activeCar != null && isCarInTrigger && !hasCompletedTurn)
        {
            // Only rotate if car is moving
            if (!requiresMovement || activeCar.IsMoving())
            {
                RotateCarTowardsTarget();
            }
        }
    }

    /// <summary>
    /// Smoothly rotates the car towards the target direction
    /// </summary>
    private void RotateCarTowardsTarget()
    {
        // Calculate the angle difference
        float angleDifference = Quaternion.Angle(carTransform.rotation, targetRotation);

        // If we're close enough, snap to target and mark as complete
        if (angleDifference < alignmentThreshold)
        {
            carTransform.rotation = targetRotation;
            hasCompletedTurn = true;
            activeCar.SetTurnInput(0f); // Stop any turning input
            return;
        }

        // Smoothly rotate towards target
        carTransform.rotation = Quaternion.RotateTowards(
            carTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // Calculate turn direction for visual tilt effect (-1 to 1)
        Vector3 cross = Vector3.Cross(carTransform.forward, targetRotation * Vector3.forward);
        float turnDirection = Mathf.Sign(cross.y);

        // Calculate turn intensity based on angle difference (0 to 1)
        float turnIntensity = Mathf.Clamp01(angleDifference / 90f);

        // Apply turn input for visual effects (tilt)
        activeCar.SetTurnInput(turnDirection * turnIntensity);
    }

    void OnTriggerExit(Collider other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null && car == activeCar)
        {
            // Ensure car is facing the correct direction when exiting
            if (!hasCompletedTurn)
            {
                carTransform.rotation = targetRotation;
            }

            // Reset turn input
            activeCar.SetTurnInput(0f);
            activeCar = null;
            carTransform = null;
            isCarInTrigger = false;
            hasCompletedTurn = false;
        }
    }

    /// <summary>
    /// Visualize turn direction in Scene view
    /// </summary>
    void OnDrawGizmos()
    {
        UpdateTargetRotation();

        // Draw trigger box
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

        // Draw wireframe
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

        // Reset matrix for world-space drawing
        Gizmos.matrix = Matrix4x4.identity;

        // Draw target direction arrow
        Vector3 arrowStart = transform.position + Vector3.up * 2f;
        Vector3 arrowDirection = targetRotation * Vector3.forward;
        Vector3 arrowEnd = arrowStart + arrowDirection * 3f;

        // Main arrow line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(arrowStart, arrowEnd);

        // Arrow head
        Vector3 right = targetRotation * Vector3.right;
        Vector3 arrowHead1 = arrowEnd - arrowDirection * 0.5f + right * 0.3f;
        Vector3 arrowHead2 = arrowEnd - arrowDirection * 0.5f - right * 0.3f;
        Gizmos.DrawLine(arrowEnd, arrowHead1);
        Gizmos.DrawLine(arrowEnd, arrowHead2);

        // Draw angle indicator
        Gizmos.color = Color.white;
        Vector3 forwardIndicator = transform.position + transform.forward * 2f + Vector3.up * 2f;
        Gizmos.DrawLine(arrowStart, forwardIndicator);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));
    }
}