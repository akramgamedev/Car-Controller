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

    private Vector3 velocity;

    private Transform camTransform;

    private Quaternion fixedRotation;

    void Awake()
    {
        camTransform = transform;
        fixedRotation = camTransform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        FollowPositionOnly();
    }

    private void FollowPositionOnly()
    {
        Vector3 desiredPosition = target.position + worldOffset;

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