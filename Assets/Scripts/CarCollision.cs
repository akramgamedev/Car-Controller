// using UnityEngine;

// public class CarCollision : MonoBehaviour
// {
//     [Header("Rotation Settings")]
//     [SerializeField] private float rotationForce = 500f;
//     [SerializeField] private float angularDrag = 2f;
    
//     [Header("References")]
//     [SerializeField] private Rigidbody carRigidbody;
//     [SerializeField] private SplineCarController splineController;
    
//     private Transform carBodyTransform;
//     private bool hasCrashed = false;

//     void Start()
//     {
//         // If rigidbody not assigned, try to find it in children
//         if (carRigidbody == null)
//         {
//             carRigidbody = GetComponentInChildren<Rigidbody>();
//         }
        
//         // Get the spline controller
//         if (splineController == null)
//         {
//             splineController = GetComponent<SplineCarController>();
//         }
        
//         if (carRigidbody != null)
//         {
//             carBodyTransform = carRigidbody.transform;
            
//             // Configure Rigidbody for better collision behavior
//             carRigidbody.linearDamping = 0.5f;
//             carRigidbody.angularDamping = angularDrag;
            
//             // Freeze rotation on X and Z axes (keep car upright)
//             carRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
//                                        RigidbodyConstraints.FreezeRotationZ;
            
//             // Add collision handler to the car body
//             CarBodyCollision bodyScript = carRigidbody.gameObject.GetComponent<CarBodyCollision>();
//             if (bodyScript == null)
//             {
//                 bodyScript = carRigidbody.gameObject.AddComponent<CarBodyCollision>();
//             }
//             bodyScript.Initialize(this);
//         }
//         else
//         {
//             Debug.LogError("No Rigidbody found on car body! Please assign it in the Inspector.");
//         }
//     }

//     public void OnCarCollision(Collision collision)
//     {
//         // Check if we hit another car
//         if (collision.gameObject.CompareTag("Car") || 
//             collision.gameObject.transform.parent?.CompareTag("Car") == true)
//         {
//             if (!hasCrashed)
//             {
//                 DisableSplineControl();
//                 ApplyRotationFromCollision(collision);
//                 hasCrashed = true;
//             }
//         }
//     }

//     void DisableSplineControl()
//     {
//         // Disable the spline controller so it stops controlling the car
//         if (splineController != null)
//         {
//             splineController.enabled = false;
//             Debug.Log("Spline control disabled - car is now in crash physics mode!");
//         }
        
//         // Unfreeze all rotation constraints so car can flip/rotate freely
//         if (carRigidbody != null)
//         {
//             carRigidbody.constraints = RigidbodyConstraints.None;
//             carRigidbody.useGravity = true;
//         }
//     }

//     void ApplyRotationFromCollision(Collision collision)
//     {
//         if (carRigidbody == null) return;
        
//         // Get collision point and direction
//         ContactPoint contact = collision.contacts[0];
//         Vector3 collisionPoint = contact.point;
//         Vector3 collisionNormal = contact.normal;
        
//         // Calculate direction from car center to collision point
//         Vector3 carCenter = carBodyTransform.position;
//         Vector3 directionToContact = collisionPoint - carCenter;
        
//         // Calculate torque perpendicular to collision
//         Vector3 torque = Vector3.Cross(directionToContact, collisionNormal);
        
//         // Apply rotational force (now can rotate on ALL axes for realistic crash)
//         carRigidbody.AddTorque(torque * rotationForce, ForceMode.Impulse);
        
//         // Add impact force away from collision
//         Vector3 pushDirection = (carBodyTransform.position - collision.contacts[0].point).normalized;
//         carRigidbody.AddForce(pushDirection * collision.relativeVelocity.magnitude * 150f, ForceMode.Impulse);
        
//         // Add some upward force for more dramatic flip
//         carRigidbody.AddForce(Vector3.up * collision.relativeVelocity.magnitude * 50f, ForceMode.Impulse);
//     }
// }


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

    void Start()
    {
        isPlayer = CompareTag("Player");

        if (carRigidbody == null)
            carRigidbody = GetComponentInChildren<Rigidbody>();

        if (splineController == null)
            splineController = GetComponent<SplineCarController>();

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


