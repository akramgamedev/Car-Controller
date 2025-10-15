using UnityEngine;

/// <summary>
/// Camera follows the car’s position but keeps a fixed world-space rotation.
/// Perfect for showing drifts — camera never rotates when the car turns.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Position Settings")]
    [Tooltip("Offset in world space (x=side, y=height, z=behind)")]
    public Vector3 worldOffset = new Vector3(0f, 8f, -10f);

    [Tooltip("How smoothly the camera follows the car position")]
    public float followSmoothSpeed = 5f;

    [Tooltip("Use SmoothDamp instead of Lerp for smoother motion")]
    public bool useSmoothDamp = true;

    // Internal smooth damp velocity
    private Vector3 velocity;

    // Cache transform
    private Transform camTransform;

    // Store the initial rotation to keep it fixed
    private Quaternion fixedRotation;

    void Awake()
    {
        camTransform = transform;
        fixedRotation = camTransform.rotation; // store initial rotation
    }

    void LateUpdate()
    {
        if (target == null) return;

        FollowPositionOnly();
    }

    private void FollowPositionOnly()
    {
        // Calculate desired position (world space)
        Vector3 desiredPosition = target.position + worldOffset;

        // Smoothly move to position
        if (useSmoothDamp)
        {
            camTransform.position = Vector3.SmoothDamp(
                camTransform.position,
                desiredPosition,
                ref velocity,
                1f / followSmoothSpeed
            );
        }
        else
        {
            camTransform.position = Vector3.Lerp(
                camTransform.position,
                desiredPosition,
                followSmoothSpeed * Time.deltaTime
            );
        }

        // Keep rotation constant (no rotation with car)
        camTransform.rotation = fixedRotation;
    }

    /// <summary>
    /// Instantly snap the camera to the target position
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        camTransform.position = target.position + worldOffset;
        camTransform.rotation = fixedRotation;
        velocity = Vector3.zero;
    }
}






// using UnityEngine;

// /// <summary>
// /// Dynamic camera that follows the car and rotates with it
// /// Maintains offset relative to car's local space (not world space)
// /// Optimized for mobile racing games
// /// </summary>
// public class FollowCamera : MonoBehaviour
// {
//     [Header("Target Settings")]
//     public Transform target;

//     [Header("Position Settings")]
//     [Tooltip("Offset relative to the car's local space (x=side, y=height, z=behind)")]
//     public Vector3 localOffset = new Vector3(0f, 8f, -6f);

//     [Tooltip("How smoothly the camera follows the car position")]
//     public float positionSmoothSpeed = 5f;

//     [Header("Rotation Settings")]
//     [Tooltip("How smoothly the camera rotates with the car")]
//     public float rotationSmoothSpeed = 8f;

//     [Tooltip("Additional look-ahead distance in front of the car")]
//     public float lookAheadDistance = 2f;

//     [Header("Advanced")]
//     [Tooltip("Height above car to look at")]
//     public float lookAtHeight = 1f;

//     [Tooltip("Use smooth damping instead of lerp (more natural feel)")]
//     public bool useSmoothDamp = true;

//     // Smooth damp velocity (for position)
//     private Vector3 currentVelocity;

//     // Cached transform for performance
//     private Transform cachedTransform;

//     void Awake()
//     {
//         cachedTransform = transform;
//     }

//     void LateUpdate()
//     {
//         if (target != null)
//         {
//             UpdateCameraPosition();
//             UpdateCameraRotation();
//         }
//     }

//     /// <summary>
//     /// Updates camera position to follow the car
//     /// Uses local space offset so camera rotates with the car
//     /// </summary>
//     private void UpdateCameraPosition()
//     {
//         // Calculate desired position in world space
//         // This makes the offset relative to the car's rotation
//         Vector3 desiredPosition = target.position + target.TransformDirection(localOffset);

//         // Smooth position transition
//         if (useSmoothDamp)
//         {
//             // SmoothDamp provides more natural, physics-like movement
//             cachedTransform.position = Vector3.SmoothDamp(
//                 cachedTransform.position,
//                 desiredPosition,
//                 ref currentVelocity,
//                 1f / positionSmoothSpeed
//             );
//         }
//         else
//         {
//             // Lerp is simpler and more predictable
//             cachedTransform.position = Vector3.Lerp(
//                 cachedTransform.position,
//                 desiredPosition,
//                 positionSmoothSpeed * Time.deltaTime
//             );
//         }
//     }

//     /// <summary>
//     /// Updates camera rotation to look at the car with look-ahead
//     /// Smoothly rotates to follow car's direction
//     /// </summary>
//     private void UpdateCameraRotation()
//     {
//         // Calculate look target with look-ahead
//         Vector3 lookAtPoint = target.position
//             + Vector3.up * lookAtHeight
//             + target.forward * lookAheadDistance;

//         // Calculate desired rotation
//         Vector3 direction = lookAtPoint - cachedTransform.position;
//         Quaternion desiredRotation = Quaternion.LookRotation(direction);

//         // Smooth rotation transition
//         cachedTransform.rotation = Quaternion.Slerp(
//             cachedTransform.rotation,
//             desiredRotation,
//             rotationSmoothSpeed * Time.deltaTime
//         );
//     }

//     /// <summary>
//     /// Call this to instantly snap camera to target (useful for respawning)
//     /// </summary>
//     public void SnapToTarget()
//     {
//         if (target != null)
//         {
//             // Set position instantly
//             cachedTransform.position = target.position + target.TransformDirection(localOffset);

//             // Set rotation instantly
//             Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight + target.forward * lookAheadDistance;
//             cachedTransform.LookAt(lookAtPoint);

//             // Reset velocity
//             currentVelocity = Vector3.zero;
//         }
//     }

//     /// <summary>
//     /// Visualize camera setup in Scene view
//     /// </summary>
//     void OnDrawGizmosSelected()
//     {
//         if (target != null)
//         {
//             // Draw line from camera to target
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawLine(transform.position, target.position);

//             // Draw target look-at point
//             Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight + target.forward * lookAheadDistance;
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(lookAtPoint, 0.5f);
//             Gizmos.DrawLine(transform.position, lookAtPoint);

//             // Draw offset indicator
//             Gizmos.color = Color.cyan;
//             Vector3 offsetPos = target.position + target.TransformDirection(localOffset);
//             Gizmos.DrawWireSphere(offsetPos, 0.3f);
//             Gizmos.DrawLine(target.position, offsetPos);
//         }
//     }
// }