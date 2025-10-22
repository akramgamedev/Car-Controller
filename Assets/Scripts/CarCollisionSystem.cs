using UnityEngine;

public class CarCollisionHandler : MonoBehaviour
{
    [Header("Assign Child Rigidbody Automatically")]
    private Rigidbody carRb;

    [Header("Collision Settings")]
    public float collisionForce = 10f;
    public float rotationImpact = 2f;

    void Start()
    {
        // Find Rigidbody in child (car body)
        carRb = GetComponentInChildren<Rigidbody>();

        if (carRb == null)
        {
            Debug.LogError("No Rigidbody found in child! Please add it to the car body.");
            return;
        }

        // Physics setup
        carRb.interpolation = RigidbodyInterpolation.Interpolate;
        carRb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        carRb.mass = 1000f;
        carRb.linearDamping = 1f;
        carRb.angularDamping  = 2f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (carRb == null) return;

        // Check if other car
        if (collision.gameObject.CompareTag("Car"))
        {
            Rigidbody otherRb = collision.gameObject.GetComponentInChildren<Rigidbody>();
            if (otherRb == null) return;

            // Calculate horizontal collision direction
            Vector3 forceDir = (carRb.transform.position - otherRb.transform.position).normalized;
            forceDir.y = 0f;

            // Apply push to both cars
            carRb.AddForce(forceDir * collisionForce, ForceMode.Impulse);
            otherRb.AddForce(-forceDir * collisionForce, ForceMode.Impulse);

            // Add slight rotation (spin effect)
            carRb.AddTorque(Vector3.up * rotationImpact, ForceMode.Impulse);
        }
    }
}




// using UnityEngine;

// public class CarCollisionSystem : MonoBehaviour
// {
//     [Header("Collision Detection")]
//     public LayerMask carLayer;
//     public float collisionForceMultiplier = 1.5f;
    
//     [Header("Rollover Settings")]
//     [Range(0f, 1f)] public float rolloverChance = 0.3f;
//     public float minSpeedForRollover = 15f;
//     public float rolloverTorqueMultiplier = 2f;
//     public float rolloverDuration = 1.5f;
    
//     [Header("Spin-Out Settings")]
//     [Range(0f, 1f)] public float spinOutChance = 0.4f;
//     public float minSpeedForSpinOut = 10f;
//     public float spinForceMultiplier = 3f;
    
//     [Header("Physics Push Settings")]
//     public float pushForceMultiplier = 1.2f;
//     public float minPushSpeed = 5f;
    
//     [Header("Angle-Based Behavior")]
//     public float sideHitAngleThreshold = 45f;
//     public float sideHitRolloverBonus = 0.2f;
    
//     [Header("AI Spline Deviation")]
//     public bool canDeviateFromSpline = true;
//     public float deviationForce = 8f;
//     public float deviationDuration = 2f;
    
//     [Header("References")]
//     public Rigidbody carRigidbody;
//     public Transform carVisual;
    
//     private bool isRollingOver = false;
//     private float rolloverTimer = 0f;
//     private Quaternion targetRollRotation;
//     private Quaternion originalRotation;
    
//     private bool isRecovering = false;
//     private float recoveryTimer = 0f;
//     private float recoveryDuration = 1f;
    
//     private bool isDeviating = false;
//     private float deviationTimer = 0f;
//     private Vector3 deviationDirection;

//     void Start()
//     {
//         if (carRigidbody == null)
//             carRigidbody = GetComponentInChildren<Rigidbody>();
        
//         if (carVisual == null)
//             carVisual = carRigidbody.transform;
//     }

//     void Update()
//     {
//         HandleRolloverAnimation();
//         HandleRecovery();
//     }

//     private float lastCollisionTime = 0f;
//     private float collisionCooldown = 0.5f;

//     void OnCollisionEnter(Collision collision)
//     {
//         if (Time.time - lastCollisionTime < collisionCooldown)
//             return;
        
//         if (((1 << collision.gameObject.layer) & carLayer) != 0)
//         {
//             lastCollisionTime = Time.time;
//             HandleCarCollision(collision);
//         }
//     }

//     void HandleCarCollision(Collision collision)
//     {
//         CarCollisionSystem otherCar = collision.gameObject.GetComponent<CarCollisionSystem>();
//         if (otherCar == null) return;

//         float mySpeed = carRigidbody.linearVelocity.magnitude;
//         float otherSpeed = otherCar.carRigidbody.linearVelocity.magnitude;
//         float relativeSpeed = (carRigidbody.linearVelocity - otherCar.carRigidbody.linearVelocity).magnitude;

//         Vector3 collisionPoint = collision.contacts[0].point;
//         Vector3 collisionNormal = collision.contacts[0].normal;
//         Vector3 relativeVelocity = carRigidbody.linearVelocity - otherCar.carRigidbody.linearVelocity;

//         float hitAngle = Vector3.Angle(transform.forward, -collisionNormal);
//         bool isSideHit = hitAngle > sideHitAngleThreshold && hitAngle < (180f - sideHitAngleThreshold);

//         bool iAmAggressor = mySpeed > otherSpeed;

//         float massRatio = carRigidbody.mass / otherCar.carRigidbody.mass;

//         if (iAmAggressor)
//         {
//             DetermineCollisionOutcome(
//                 otherCar,
//                 relativeSpeed,
//                 collisionNormal,
//                 collisionPoint,
//                 isSideHit,
//                 massRatio
//             );
//         }

//         ApplyCollisionReaction(relativeSpeed * 0.5f, -collisionNormal, isSideHit);

//         if (relativeSpeed > 10f)
//         {
//             RemoveFromSpline();
//         }
//     }
    
//     public void RemoveFromSpline()
//     {
//         SplineCarController splineController = GetComponent<SplineCarController>();
//         if(splineController != null)
//         {
//             splineController.enabled = false;
//             carRigidbody.constraints &= ~RigidbodyConstraints.FreezePosition;
//         }
//     }

//     void DetermineCollisionOutcome(
//         CarCollisionSystem targetCar, 
//         float impactSpeed, 
//         Vector3 direction,
//         Vector3 impactPoint,
//         bool isSideHit,
//         float massRatio)
//     {
//         float adjustedRolloverChance = rolloverChance;
//         float adjustedSpinChance = spinOutChance;
        
//         if (isSideHit)
//         {
//             adjustedRolloverChance += sideHitRolloverBonus;
//         }
        
//         float speedFactor = Mathf.Clamp01(impactSpeed / 30f);
//         adjustedRolloverChance *= (1f + speedFactor);
//         adjustedSpinChance *= (1f + speedFactor * 0.5f);
        
//         adjustedRolloverChance *= (2f - massRatio);
        
//         float roll = Random.Range(0f, 1f);
        
//         if (impactSpeed >= minSpeedForRollover && roll < adjustedRolloverChance)
//         {
//             targetCar.InitiateRollover(direction, impactPoint, impactSpeed);
//         }
//         else if (impactSpeed >= minSpeedForSpinOut && roll < (adjustedRolloverChance + adjustedSpinChance))
//         {
//             targetCar.InitiateSpinOut(direction, impactSpeed);
//         }
//         else if (impactSpeed >= minPushSpeed)
//         {
//             targetCar.InitiatePhysicsPush(direction, impactSpeed);
//         }
        
//         if (canDeviateFromSpline && impactSpeed > 5f)
//         {
//             targetCar.InitiateSplineDeviation(direction, impactSpeed);
//         }
//     }

//     public void InitiateRollover(Vector3 direction, Vector3 impactPoint, float force)
//     {
//         if (isRollingOver) return;
        
//         isRollingOver = true;
//         rolloverTimer = 0f;
//         originalRotation = carVisual.localRotation;
        
//         Vector3 localImpact = transform.InverseTransformPoint(impactPoint);
//         bool rollLeft = localImpact.x < 0;
        
//         float rollAmount = Random.Range(90f, 180f) * (rollLeft ? -1 : 1);
//         targetRollRotation = Quaternion.Euler(0, 0, rollAmount) * originalRotation;
        
//         if (carRigidbody != null)
//         {
//             carRigidbody.AddForce(Vector3.up * force * rolloverTorqueMultiplier, ForceMode.Impulse);
//             Vector3 torqueAxis = rollLeft ? -transform.forward : transform.forward;
//             carRigidbody.AddTorque(torqueAxis * force * rolloverTorqueMultiplier, ForceMode.Impulse);
//         }
//     }

//     public void InitiateSpinOut(Vector3 direction, float force)
//     {
//         if (carRigidbody == null) return;
        
//         float spinDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
//         Vector3 spinTorque = Vector3.up * force * spinForceMultiplier * spinDirection;
//         carRigidbody.AddTorque(spinTorque, ForceMode.Impulse);
        
//         Vector3 lateralDirection = Vector3.Cross(Vector3.up, direction).normalized;
//         carRigidbody.AddForce(lateralDirection * force * pushForceMultiplier, ForceMode.Impulse);
//     }

//     public void InitiatePhysicsPush(Vector3 direction, float force)
//     {
//         if (carRigidbody == null) return;
        
//         Vector3 pushDirection = direction.normalized;
//         pushDirection += new Vector3(
//             Random.Range(-0.2f, 0.2f), 
//             0, 
//             Random.Range(-0.2f, 0.2f)
//         );
        
//         carRigidbody.AddForce(pushDirection * force * pushForceMultiplier, ForceMode.Impulse);
//     }

//     public void InitiateSplineDeviation(Vector3 direction, float force)
//     {
//         isDeviating = true;
//         deviationTimer = 0f;
//         deviationDirection = direction.normalized;
        
//         if (carRigidbody != null)
//         {
//             carRigidbody.AddForce(deviationDirection * deviationForce * force * 0.1f, ForceMode.Impulse);
//         }
//     }

//     void HandleRolloverAnimation()
//     {
//         if (!isRollingOver) return;
        
//         rolloverTimer += Time.deltaTime;
//         float progress = rolloverTimer / rolloverDuration;
        
//         if (progress < 1f)
//         {
//             carVisual.localRotation = Quaternion.Slerp(
//                 originalRotation, 
//                 targetRollRotation, 
//                 progress
//             );
//         }
//         else
//         {
//             isRollingOver = false;
//             isRecovering = true;
//             recoveryTimer = 0f;
//             originalRotation = carVisual.localRotation;
//         }
//     }

//     void HandleRecovery()
//     {
//         if (!isRecovering) return;
        
//         recoveryTimer += Time.deltaTime;
//         float progress = recoveryTimer / recoveryDuration;
        
//         if (progress < 1f)
//         {
//             carVisual.localRotation = Quaternion.Slerp(
//                 originalRotation,
//                 Quaternion.identity,
//                 progress
//             );
//         }
//         else
//         {
//             carVisual.localRotation = Quaternion.identity;
//             isRecovering = false;
//         }
//     }

//     void ApplyCollisionReaction(float force, Vector3 direction, bool isSideHit)
//     {
//         if (carRigidbody == null) return;
        
//         float reactionMultiplier = isSideHit ? 0.7f : 0.5f;
//         carRigidbody.AddForce(direction * force * reactionMultiplier, ForceMode.Impulse);
//     }

//     public bool IsDeviating() => isDeviating;
//     public Vector3 GetDeviationDirection() => deviationDirection;
//     public float GetDeviationProgress() => deviationTimer / deviationDuration;
    
//     public void CompleteDeviation()
//     {
//         if (deviationTimer >= deviationDuration)
//         {
//             isDeviating = false;
//             deviationTimer = 0f;
//         }
//     }

//     void FixedUpdate()
//     {
//         if (isDeviating)
//         {
//             deviationTimer += Time.fixedDeltaTime;
//             if (deviationTimer >= deviationDuration)
//             {
//                 isDeviating = false;
//             }
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (isDeviating)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawRay(transform.position, deviationDirection * 3f);
//         }
//     }
// }