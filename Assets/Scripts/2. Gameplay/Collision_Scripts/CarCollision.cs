//using MoreMountains.NiceVibrations;
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

        if (carRigidbody == null)
        {
            Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in childRigidbodies)
            {
                if (rb.CompareTag("Car") && rb.gameObject.activeInHierarchy)
                {
                    carRigidbody = rb;
                    LogHelper.Log("Car Rigidbody found on child: " + rb.gameObject.name);
                    break;
                }
            }

            if (carRigidbody == null && childRigidbodies.Length > 0)
            {
                carRigidbody = childRigidbodies[0];
                LogHelper.Log("No tagged Rigidbody found. Using first child Rigidbody: " + carRigidbody.gameObject.name);
            }

        }

        if (carRigidbody != null)
        {
            carBodyTransform = carRigidbody.transform;
            ApplyBaseRigidbodySettings();

            CarBodyCollision bodyScript = carRigidbody.gameObject.GetComponent<CarBodyCollision>();
            if (bodyScript == null)
                bodyScript = carRigidbody.gameObject.AddComponent<CarBodyCollision>();

            bodyScript.Initialize(this);
        }
        else
        {
            LogHelper.LogError("No Rigidbody found on car body! Please assign it in the Inspector.");
        }
    }

    public void RefreshCarRigidbody()
    {
        LogHelper.Log("=== RefreshCarRigidbody CALLED ===");
        LogHelper.Log($"Parent object: {gameObject.name}, Active: {gameObject.activeInHierarchy}");

        carRigidbody = null;

        Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        LogHelper.Log($"Found {childRigidbodies.Length} total rigidbodies");

        for (int i = 0; i < childRigidbodies.Length; i++)
        {
            var rb = childRigidbodies[i];
            LogHelper.Log($"RB {i}: {rb.gameObject.name}, Active: {rb.gameObject.activeInHierarchy}, Tag: {rb.tag}");
        }

        foreach (var rb in childRigidbodies)
        {
            if (rb.gameObject.activeInHierarchy)
            {
                if (rb.CompareTag("Car"))
                {
                    carRigidbody = rb;
                    LogHelper.Log($"✓ Found rigidbody with Car tag: {rb.gameObject.name}");
                    break;
                }
            }
        }

        if (carRigidbody == null)
        {
            foreach (var rb in childRigidbodies)
            {
                if (rb.gameObject.activeInHierarchy)
                {
                    carRigidbody = rb;
                    LogHelper.Log($"✓ Found rigidbody (fallback): {carRigidbody.gameObject.name}");
                    break;
                }
            }
        }

        if (carRigidbody == null)
        {
            LogHelper.LogError("NO RIGIDBODY FOUND!");
            return;
        }

        carBodyTransform = carRigidbody.transform;
        ApplyBaseRigidbodySettings();

        CarBodyCollision bodyScript = carRigidbody.gameObject.GetComponent<CarBodyCollision>();
        if (bodyScript == null)
            bodyScript = carRigidbody.gameObject.AddComponent<CarBodyCollision>();

        bodyScript.Initialize(this);
        LogHelper.Log("Rigidbody setup complete");
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
                AudioManager.Instance?.PlaySFX("CarCrash");

                DisableSplineControl();
                ApplyRotationFromCollision(collision);
                hasCrashed = true;

                CarSkidMarks skidMarks = GetComponent<CarSkidMarks>();
                if (skidMarks != null)
                    skidMarks.DisableTrails();

                if (isPlayer && UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLevelFailDelay(resetPanelDelay);
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


