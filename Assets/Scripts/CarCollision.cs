using UnityEngine;

public class CarCollision : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationForce = 500f;

    [Header("Linear Damping Settings")]
    [SerializeField] private float linearDampingPlayer = 5f;
    [SerializeField] private float linearDampingAI = 0.5f;

    [Header("References")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private SplineCarController splineController;

    private Transform carBodyTransform;
    private bool hasCrashed = false;
    private bool isPlayer = false;

    public float resetPanelDelay = 5f;

    void Start()
    {
        isPlayer = CompareTag("Player");

        if (carRigidbody == null)
            carRigidbody = GetComponentInChildren<Rigidbody>();

        if (splineController == null)
            splineController = GetComponent<SplineCarController>();

            if(carRigidbody == null)
        {
            Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in childRigidbodies)
            {
                if (rb.CompareTag("Car") && rb.gameObject.activeInHierarchy)
                {
                    carRigidbody = rb;
                    Debug.Log("Car Rigidbody found on child: " + rb.gameObject.name);
                    break;
                }
            }
            
if(carRigidbody == null && childRigidbodies.Length> 0)
            {
                carRigidbody = childRigidbodies[0];
                Debug.Log("No tagged Rigidbody found. Using first child Rigidbody: " + carRigidbody.gameObject.name);
            }

        }

        if (carRigidbody != null)
        {
            carBodyTransform = carRigidbody.transform;
            ApplyBaseRigidbodySettings();

            // Add body collision handler
            CarBodyCollision bodyScript = carRigidbody.gameObject.GetComponent<CarBodyCollision>();
            if (bodyScript == null)
                bodyScript = carRigidbody.gameObject.AddComponent<CarBodyCollision>();

            bodyScript.Initialize(this);
        }
        else
        {
            Debug.LogError("No Rigidbody found on car body! Please assign it in the Inspector.");
        }
    }

    void ApplyBaseRigidbodySettings()
    {
#if UNITY_6000_0_OR_NEWER
        carRigidbody.linearDamping = isPlayer ? linearDampingPlayer : linearDampingAI;
#else
        carRigidbody.drag = isPlayer ? linearDampingPlayer : linearDampingAI;
#endif
        carRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void OnCarCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") ||
            collision.gameObject.transform.parent?.CompareTag("Car") == true)
        {
            if (!hasCrashed)
            {
                DisableSplineControl();
                ApplyRotationFromCollision(collision);
                hasCrashed = true;

                if (isPlayer && UIManager.Instance != null)
                {
                    UIManager.Instance.ShowResetPanelDelayed(resetPanelDelay);
                }
            }
        }
    }

    void DisableSplineControl()
    {
        if (splineController != null)
            splineController.enabled = false;

        if (carRigidbody != null)
        {
            carRigidbody.constraints = RigidbodyConstraints.None;
            carRigidbody.useGravity = true;

            // Re-apply correct linear damping after enabling gravity
#if UNITY_6000_0_OR_NEWER
            carRigidbody.linearDamping = isPlayer ? linearDampingPlayer : linearDampingAI;
#else
            carRigidbody.drag = isPlayer ? linearDampingPlayer : linearDampingAI;
#endif
        }
    }

    void ApplyRotationFromCollision(Collision collision)
    {
        if (carRigidbody == null) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 collisionPoint = contact.point;
        Vector3 collisionNormal = contact.normal;

        Vector3 directionToContact = collisionPoint - carBodyTransform.position;
        Vector3 torque = Vector3.Cross(directionToContact, collisionNormal);

        carRigidbody.AddTorque(torque * rotationForce, ForceMode.Impulse);

        Vector3 pushDirection = (carBodyTransform.position - collisionPoint).normalized;
        carRigidbody.AddForce(pushDirection * collision.relativeVelocity.magnitude * 150f, ForceMode.Impulse);
        carRigidbody.AddForce(Vector3.up * collision.relativeVelocity.magnitude * 50f, ForceMode.Impulse);
    }

    void LateUpdate()
    {
        if (carRigidbody == null) return;

#if UNITY_6000_0_OR_NEWER
        float targetDamping = isPlayer ? linearDampingPlayer : linearDampingAI;
        if (Mathf.Abs(carRigidbody.linearDamping - targetDamping) > 0.01f)
            carRigidbody.linearDamping = targetDamping;
#else
        float targetDrag = isPlayer ? linearDampingPlayer : linearDampingAI;
        if (Mathf.Abs(carRigidbody.drag - targetDrag) > 0.01f)
            carRigidbody.drag = targetDrag;
#endif
    }
}


