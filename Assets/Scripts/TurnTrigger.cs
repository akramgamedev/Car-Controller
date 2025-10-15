using UnityEngine;

public class TurnTrigger : MonoBehaviour
{
    [Header("Turn Settings")]
    [Tooltip("The direction the car should face after passing through this trigger")]
    [SerializeField] private Vector3 targetDirection = Vector3.forward;

    [Tooltip("How quickly the car rotates to face the target direction")]
    [SerializeField] private float rotationSpeed = 120f; // REDUCED for smoother drift

    [Header("Trigger Settings")]
    [SerializeField] private float triggerWidth = 5f;
    [SerializeField] private float triggerHeight = 3f;
    [SerializeField] private float triggerLength = 10f;

    [Header("Visual Helper")]
    [Tooltip("Use this to set turn direction easily: 0=Forward, 90=Right, -90=Left, 180=Backward")]
    [SerializeField] private float targetAngle = 90f;

    [Header("Drift Settings")]
    [SerializeField] private bool allowDriftInTrigger = true; // NEW: Allow drift
    [SerializeField] private float driftTriggerIntensity = 1f;

    [Header("Advanced")]
    [SerializeField] private bool requiresMovement = true;
    [SerializeField] private float alignmentThreshold = 5f;

    [Header("Debug & Customization")]
    [SerializeField] private bool showTriggerVisuals = true;
    [SerializeField] private Color triggerFillColor = new Color(0.2f, 0.8f, 1f, 0.3f);
    [SerializeField] private Color triggerWireColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color arrowColor = Color.yellow;
    [SerializeField] private Color selectedTriggerColor = Color.cyan;

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
        triggerWidth = Mathf.Max(0.1f, triggerWidth);
        triggerHeight = Mathf.Max(0.1f, triggerHeight);
        triggerLength = Mathf.Max(0.1f, triggerLength);
        rotationSpeed = Mathf.Max(1f, rotationSpeed);
        alignmentThreshold = Mathf.Max(0.1f, alignmentThreshold);
        driftTriggerIntensity = Mathf.Clamp01(driftTriggerIntensity);

        UpdateTargetRotation();
        UpdateTriggerSize();
    }

    private void SetupTrigger()
    {
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }

        triggerCollider.isTrigger = true;
        UpdateTriggerSize();
    }

    private void UpdateTriggerSize()
    {
        if (triggerCollider != null)
        {
            triggerCollider.size = new Vector3(triggerWidth, triggerHeight, triggerLength);
        }
    }

    private void UpdateTargetRotation()
    {
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

            // Only disable manual turning, allow drift to continue
            if (!allowDriftInTrigger)
            {
                activeCar.SetTurnTriggerActive(true);
            }

            UpdateTargetRotation();
        }
    }

    void Update()
    {
        if (activeCar != null && isCarInTrigger && !hasCompletedTurn)
        {
            if (!requiresMovement || activeCar.IsMoving())
            {
                RotateCarTowardsTarget();
            }
        }
    }

    private void RotateCarTowardsTarget()
    {
        float angleDifference = Quaternion.Angle(carTransform.rotation, targetRotation);

        if (angleDifference < alignmentThreshold)
        {
            // Snap to exact target when close enough
            activeCar.ForceRotation(targetRotation);
            activeCar.ResetTilt();
            hasCompletedTurn = true;
            activeCar.SetTurnInput(0f);
            return;
        }

        // Calculate turn direction
        Vector3 cross = Vector3.Cross(carTransform.forward, targetRotation * Vector3.forward);
        float turnDirection = Mathf.Sign(cross.y);

        if (allowDriftInTrigger)
        {
            // LET THE CAR DRIFT - just provide turn input
            float turnIntensity = Mathf.Clamp01(angleDifference / 90f) * driftTriggerIntensity;
            activeCar.SetTurnInput(turnDirection * turnIntensity);
            
            // Gradually rotate using standard turning (car will drift naturally)
            float turnAmount = turnDirection * rotationSpeed * Time.deltaTime;
            carTransform.Rotate(0f, turnAmount, 0f);
        }
        else
        {
            // Old behavior: force rotation without drift
            Quaternion newRotation = Quaternion.RotateTowards(
                carTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            activeCar.ForceRotation(newRotation);

            float turnIntensity = Mathf.Clamp01(angleDifference / 90f);
            activeCar.SetTurnInput(turnDirection * turnIntensity);
        }
    }

    void OnTriggerExit(Collider other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null && car == activeCar)
        {
            if (!hasCompletedTurn)
            {
                activeCar.ForceRotation(targetRotation);
            }

            activeCar.ResetTilt();
            activeCar.SetTurnInput(0f);
            activeCar.SetTurnTriggerActive(false);

            activeCar = null;
            carTransform = null;
            isCarInTrigger = false;
            hasCompletedTurn = false;
        }
    }

    public void SetTriggerSize(float width, float height, float length)
    {
        triggerWidth = Mathf.Max(0.1f, width);
        triggerHeight = Mathf.Max(0.1f, height);
        triggerLength = Mathf.Max(0.1f, length);
        UpdateTriggerSize();
    }

    public void SetTargetAngle(float angle)
    {
        targetAngle = angle;
        UpdateTargetRotation();
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(1f, speed);
    }

    public Vector3 GetTriggerSize()
    {
        return new Vector3(triggerWidth, triggerHeight, triggerLength);
    }

    void OnDrawGizmos()
    {
        if (!showTriggerVisuals)
            return;

        UpdateTargetRotation();

        Gizmos.color = triggerFillColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

        Gizmos.color = triggerWireColor;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

        Gizmos.matrix = Matrix4x4.identity;

        Vector3 arrowStart = transform.position + Vector3.up * 2f;
        Vector3 arrowDirection = targetRotation * Vector3.forward;
        Vector3 arrowEnd = arrowStart + arrowDirection * 3f;

        Gizmos.color = arrowColor;
        Gizmos.DrawLine(arrowStart, arrowEnd);

        Vector3 right = targetRotation * Vector3.right;
        Vector3 arrowHead1 = arrowEnd - arrowDirection * 0.5f + right * 0.3f;
        Vector3 arrowHead2 = arrowEnd - arrowDirection * 0.5f - right * 0.3f;
        Gizmos.DrawLine(arrowEnd, arrowHead1);
        Gizmos.DrawLine(arrowEnd, arrowHead2);

        Gizmos.color = Color.white;
        Vector3 forwardIndicator = transform.position + transform.forward * 2f + Vector3.up * 2f;
        Gizmos.DrawLine(arrowStart, forwardIndicator);
    }

    void OnDrawGizmosSelected()
    {
        if (!showTriggerVisuals)
            return;

        Gizmos.color = selectedTriggerColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));
    }
}



// using UnityEngine;

// public class TurnTrigger : MonoBehaviour
// {
//     [Header("Turn Settings")]
//     [Tooltip("The direction the car should face after passing through this trigger")]
//     [SerializeField] private Vector3 targetDirection = Vector3.forward;

//     [Tooltip("How quickly the car rotates to face the target direction")]
//     [SerializeField] private float rotationSpeed = 180f;

//     [Header("Trigger Settings")]
//     [SerializeField] private float triggerWidth = 5f;
//     [SerializeField] private float triggerHeight = 3f;
//     [SerializeField] private float triggerLength = 10f;

//     [Header("Visual Helper")]
//     [Tooltip("Use this to set turn direction easily: 0=Forward, 90=Right, -90=Left, 180=Backward")]
//     [SerializeField] private float targetAngle = 90f;

//     [Header("Drift Settings")]
//     [SerializeField] private bool enableDriftOnTrigger = true;
//     [SerializeField] private float driftTriggerIntensity = 1f; // 0-1, how intense the drift is

//     [Header("Advanced")]
//     [SerializeField] private bool requiresMovement = true;
//     [SerializeField] private float alignmentThreshold = 5f;

//     [Header("Debug & Customization")]
//     [SerializeField] private bool showTriggerVisuals = true;
//     [SerializeField] private Color triggerFillColor = new Color(0.2f, 0.8f, 1f, 0.3f);
//     [SerializeField] private Color triggerWireColor = new Color(0.2f, 0.8f, 1f, 0.8f);
//     [SerializeField] private Color arrowColor = Color.yellow;
//     [SerializeField] private Color selectedTriggerColor = Color.cyan;

//     private BoxCollider triggerCollider;
//     private CarController activeCar;
//     private Transform carTransform;
//     private bool isCarInTrigger;
//     private bool hasCompletedTurn;
//     private bool hasDriftTriggered;
//     private Quaternion targetRotation;

//     void Awake()
//     {
//         SetupTrigger();
//         UpdateTargetRotation();
//     }

//     void OnValidate()
//     {
//         triggerWidth = Mathf.Max(0.1f, triggerWidth);
//         triggerHeight = Mathf.Max(0.1f, triggerHeight);
//         triggerLength = Mathf.Max(0.1f, triggerLength);
//         rotationSpeed = Mathf.Max(1f, rotationSpeed);
//         alignmentThreshold = Mathf.Max(0.1f, alignmentThreshold);
//         driftTriggerIntensity = Mathf.Clamp01(driftTriggerIntensity);

//         UpdateTargetRotation();
//         UpdateTriggerSize();
//     }

//     private void SetupTrigger()
//     {
//         triggerCollider = GetComponent<BoxCollider>();
//         if (triggerCollider == null)
//         {
//             triggerCollider = gameObject.AddComponent<BoxCollider>();
//         }

//         triggerCollider.isTrigger = true;
//         UpdateTriggerSize();
//     }

//     private void UpdateTriggerSize()
//     {
//         if (triggerCollider != null)
//         {
//             triggerCollider.size = new Vector3(triggerWidth, triggerHeight, triggerLength);
//         }
//     }

//     private void UpdateTargetRotation()
//     {
//         targetRotation = transform.rotation * Quaternion.Euler(0f, targetAngle, 0f);
//     }

//     void OnTriggerEnter(Collider other)
//     {
//         CarController car = other.GetComponent<CarController>();
//         if (car != null)
//         {
//             activeCar = car;
//             carTransform = other.transform;
//             isCarInTrigger = true;
//             hasCompletedTurn = false;
//             hasDriftTriggered = false;

//             activeCar.SetTurnTriggerActive(true);

//             UpdateTargetRotation();
//         }
//     }

//     void Update()
//     {
//         if (activeCar != null && isCarInTrigger && !hasCompletedTurn)
//         {
//             if (!requiresMovement || activeCar.IsMoving())
//             {
//                 RotateCarTowardsTarget();
//             }
//         }
//     }

//     private void RotateCarTowardsTarget()
//     {
//         float angleDifference = Quaternion.Angle(carTransform.rotation, targetRotation);

//         // Trigger drift once when angle difference is significant
//         if (!hasDriftTriggered && enableDriftOnTrigger && angleDifference > 30f)
//         {
//             // Determine drift direction based on which way we need to turn
//             Vector3 cross = Vector3.Cross(carTransform.forward, targetRotation * Vector3.forward);
//             float driftDirection = Mathf.Sign(cross.y);
            
//             activeCar.TriggerDrift(driftDirection * driftTriggerIntensity);
//             hasDriftTriggered = true;
//         }

//         if (angleDifference < alignmentThreshold)
//         {
//             // Force rotation to exact target
//             activeCar.ForceRotation(targetRotation);
//             activeCar.ResetTilt();
//             hasCompletedTurn = true;
//             activeCar.SetTurnInput(0f);
//             return;
//         }

//         // Smoothly rotate towards target
//         Quaternion newRotation = Quaternion.RotateTowards(
//             carTransform.rotation,
//             targetRotation,
//             rotationSpeed * Time.deltaTime
//         );

//         activeCar.ForceRotation(newRotation);

//         // Calculate turn direction for visual tilt effect
//         Vector3 cross2 = Vector3.Cross(carTransform.forward, targetRotation * Vector3.forward);
//         float turnDirection = Mathf.Sign(cross2.y);

//         float turnIntensity = Mathf.Clamp01(angleDifference / 90f);

//         activeCar.SetTurnInput(turnDirection * turnIntensity);
//     }

//     void OnTriggerExit(Collider other)
//     {
//         CarController car = other.GetComponent<CarController>();
//         if (car != null && car == activeCar)
//         {
//             if (!hasCompletedTurn)
//             {
//                 activeCar.ForceRotation(targetRotation);
//             }

//             activeCar.ResetTilt();
//             activeCar.SetTurnInput(0f);
//             activeCar.SetTurnTriggerActive(false);

//             activeCar = null;
//             carTransform = null;
//             isCarInTrigger = false;
//             hasCompletedTurn = false;
//             hasDriftTriggered = false;
//         }
//     }

//     /// <summary>
//     /// Public method to change trigger size at runtime
//     /// </summary>
//     public void SetTriggerSize(float width, float height, float length)
//     {
//         triggerWidth = Mathf.Max(0.1f, width);
//         triggerHeight = Mathf.Max(0.1f, height);
//         triggerLength = Mathf.Max(0.1f, length);
//         UpdateTriggerSize();
//     }

//     /// <summary>
//     /// Public method to change target angle at runtime
//     /// </summary>
//     public void SetTargetAngle(float angle)
//     {
//         targetAngle = angle;
//         UpdateTargetRotation();
//     }

//     /// <summary>
//     /// Public method to change rotation speed at runtime
//     /// </summary>
//     public void SetRotationSpeed(float speed)
//     {
//         rotationSpeed = Mathf.Max(1f, speed);
//     }

//     /// <summary>
//     /// Get current trigger dimensions
//     /// </summary>
//     public Vector3 GetTriggerSize()
//     {
//         return new Vector3(triggerWidth, triggerHeight, triggerLength);
//     }

//     void OnDrawGizmos()
//     {
//         if (!showTriggerVisuals)
//             return;

//         UpdateTargetRotation();

//         // Draw trigger box fill
//         Gizmos.color = triggerFillColor;
//         Gizmos.matrix = transform.localToWorldMatrix;
//         Gizmos.DrawCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

//         // Draw trigger box wireframe
//         Gizmos.color = triggerWireColor;
//         Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));

//         // Reset matrix for world-space drawing
//         Gizmos.matrix = Matrix4x4.identity;

//         // Draw target direction arrow
//         Vector3 arrowStart = transform.position + Vector3.up * 2f;
//         Vector3 arrowDirection = targetRotation * Vector3.forward;
//         Vector3 arrowEnd = arrowStart + arrowDirection * 3f;

//         // Main arrow line
//         Gizmos.color = arrowColor;
//         Gizmos.DrawLine(arrowStart, arrowEnd);

//         // Arrow head
//         Vector3 right = targetRotation * Vector3.right;
//         Vector3 arrowHead1 = arrowEnd - arrowDirection * 0.5f + right * 0.3f;
//         Vector3 arrowHead2 = arrowEnd - arrowDirection * 0.5f - right * 0.3f;
//         Gizmos.DrawLine(arrowEnd, arrowHead1);
//         Gizmos.DrawLine(arrowEnd, arrowHead2);

//         // Draw angle indicator
//         Gizmos.color = Color.white;
//         Vector3 forwardIndicator = transform.position + transform.forward * 2f + Vector3.up * 2f;
//         Gizmos.DrawLine(arrowStart, forwardIndicator);
//     }

//     void OnDrawGizmosSelected()
//     {
//         if (!showTriggerVisuals)
//             return;

//         Gizmos.color = selectedTriggerColor;
//         Gizmos.matrix = transform.localToWorldMatrix;
//         Gizmos.DrawWireCube(Vector3.zero, new Vector3(triggerWidth, triggerHeight, triggerLength));
//     }
// }
