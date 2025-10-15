//************* working code *******************
using UnityEngine;
using UnityEngine.Splines;

public class SplineCarController : MonoBehaviour
{
    [Header("Spline Settings")]
    public SplineContainer splineContainer;
    public bool loopSpline = false; // Set to false to stop at end
    private float splineProgress = 0f; // 0 to 1 along the spline
    private bool reachedEnd = false;

    [Header("Movement Settings")]
    public float maxSpeed = 15f;
    public float acceleration = 5f;
    public float deceleration = 8f; // Increased for quicker stop
    private float currentSpeed = 0f;

    [Header("Drift Settings")]
    public float driftThreshold = 8f;
    public float maxDriftAngle = 35f; // Increased for more dramatic drift
    public float driftSmoothness = 8f; // Faster drift response
    private float currentDriftAngle = 0f;

    [Header("Turn Detection")]
    public float lookAheadDistance = 0.08f; // Increased for better turn prediction
    public float rotationSpeed = 15f; // New: Control rotation smoothness

    [Header("Visual Settings")]
    public Transform carVisual;
    public ParticleSystem driftParticles;

    private Rigidbody rb;
    private float totalSplineLength;
    private bool isTouching = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (splineContainer == null)
        {
            Debug.LogError("Spline Container not assigned!");
            return;
        }

        totalSplineLength = splineContainer.Spline.GetLength();

        Vector3 startPos = splineContainer.EvaluatePosition(0f);
        transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);

        if (carVisual == null)
            carVisual = transform;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleDrift();
        HandleDriftParticles();
    }

    void HandleInput()
    {
        // Mobile touch input - hold to move
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isTouching = (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved);
        }
        else
        {
            isTouching = false;
        }

        // Desktop testing - hold mouse or space
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            isTouching = true;
        }
#endif
    }

    void HandleMovement()
    {
        // Stop if reached end and not looping
        if (reachedEnd && !loopSpline)
        {
            currentSpeed = 0f;
            return;
        }

        // Accelerate when touching, decelerate when not
        if (isTouching)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        if (currentSpeed > 0.01f)
        {
            // Move along spline
            float speedOnSpline = currentSpeed / totalSplineLength;
            splineProgress += speedOnSpline * Time.deltaTime;

            // Handle end of spline
            if (splineProgress >= 1f)
            {
                if (loopSpline)
                {
                    // Loop back to start
                    splineProgress -= 1f;
                    reachedEnd = false;
                }
                else
                {
                    // Stop at end
                    splineProgress = 1f;
                    reachedEnd = true;
                    currentSpeed = 0f;
                }
            }

            // Get position on spline
            Vector3 posOnSpline = splineContainer.EvaluatePosition(splineProgress);
            transform.position = new Vector3(posOnSpline.x, transform.position.y, posOnSpline.z);

            // Get direction - NO ROTATION HERE, drift handles it all
            // We only update position on spline, rotation is handled by drift
        }
    }


    void HandleDrift()
    {
        // Get current spline direction
        Vector3 currentTangent = splineContainer.EvaluateTangent(splineProgress);
        currentTangent.y = 0;
        currentTangent.Normalize();

        // Calculate turn sharpness by looking ahead
        float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
        Vector3 futureTangent = splineContainer.EvaluateTangent(lookAhead);
        futureTangent.y = 0;
        futureTangent.Normalize();

        // Calculate angle difference (turn sharpness)
        float angleChange = Vector3.SignedAngle(currentTangent, futureTangent, Vector3.up);

        // MAIN CAR ROTATION - Always follow spline direction
        if (currentTangent != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentTangent);
            float rotSpeed = rotationSpeed * Mathf.Clamp01(currentSpeed / maxSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
        }

        // Calculate VISUAL drift based on speed and turn sharpness
        float targetDrift = 0f;

        if (currentSpeed > driftThreshold)
        {
            // Speed factor for drift intensity
            float speedFactor = Mathf.Clamp01((currentSpeed - driftThreshold) / (maxSpeed - driftThreshold));

            // Turn sharpness with better scaling
            float turnSharpness = angleChange / (lookAheadDistance * 50f); // Adjusted scale

            // Calculate target drift angle
            targetDrift = turnSharpness * speedFactor * maxDriftAngle;
            targetDrift = Mathf.Clamp(targetDrift, -maxDriftAngle, maxDriftAngle);
        }

        // Smooth drift angle transition
        currentDriftAngle = Mathf.Lerp(currentDriftAngle, targetDrift, Time.deltaTime * driftSmoothness);

        // Apply drift to visual ONLY (not the main car transform)
        if (carVisual != null && carVisual != transform)
        {
            carVisual.localRotation = Quaternion.Euler(0, currentDriftAngle, 0);
        }
    }

    void HandleDriftParticles()
    {
        if (driftParticles != null)
        {
            // Enable particles when drifting
            if (Mathf.Abs(currentDriftAngle) > 5f && currentSpeed > driftThreshold)
            {
                if (!driftParticles.isPlaying)
                    driftParticles.Play();
            }
            else
            {
                if (driftParticles.isPlaying)
                    driftParticles.Stop();
            }
        }
    }

    // Visualize the spline path in editor
    void OnDrawGizmos()
    {
        if (splineContainer != null && splineContainer.Spline != null)
        {
            Gizmos.color = Color.yellow;
            int segments = 50;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 pos = splineContainer.EvaluatePosition(t);
                Gizmos.DrawSphere(pos, 0.2f);

                if (i > 0)
                {
                    float prevT = (float)(i - 1) / segments;
                    Vector3 prevPos = splineContainer.EvaluatePosition(prevT);
                    Gizmos.DrawLine(prevPos, pos);
                }
            }

            // Draw look ahead point
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                float lookAhead = Mathf.Clamp01(splineProgress + lookAheadDistance);
                Vector3 lookAheadPos = splineContainer.EvaluatePosition(lookAhead);
                Gizmos.DrawSphere(lookAheadPos, 0.3f);
            }
        }
    }
}
