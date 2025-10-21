using UnityEngine;

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

    public void SnapToTarget()
    {
        if (target == null) return;

        camTransform.position = target.position + worldOffset;
        camTransform.rotation = fixedRotation;
        velocity = Vector3.zero;
    }
}