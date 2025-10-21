using UnityEngine;

public class CarCollisionSystem : MonoBehaviour
{
    [Header("Collision Detection")]
    public LayerMask carLayer;
    public float collisionForceMultiplier = 1.5f;
    
    [Header("Rollover Settings")]
    [Range(0f, 1f)] public float rolloverChance = 0.3f;
    public float minSpeedForRollover = 15f;
    public float rolloverTorqueMultiplier = 2f;
    public float rolloverDuration = 1.5f;
    
    [Header("Spin-Out Settings")]
    [Range(0f, 1f)] public float spinOutChance = 0.4f;
    public float minSpeedForSpinOut = 10f;
    public float spinForceMultiplier = 3f;
    
    [Header("Physics Push Settings")]
    public float pushForceMultiplier = 1.2f;
    public float minPushSpeed = 5f;
    
    [Header("Angle-Based Behavior")]
    public float sideHitAngleThreshold = 45f; // Degrees from head-on
    public float sideHitRolloverBonus = 0.2f;
    
    [Header("AI Spline Deviation")]
    public bool canDeviateFromSpline = true;
    public float deviationForce = 8f;
    public float deviationDuration = 2f;
    
    [Header("References")]
    public Rigidbody carRigidbody;
    public Transform carVisual;
    
    // State tracking
    private bool isRollingOver = false;
    private float rolloverTimer = 0f;
    private Quaternion targetRollRotation;
    private Quaternion originalRotation;
    
    private bool isRecovering = false;
    private float recoveryTimer = 0f;
    private float recoveryDuration = 1f;
    
    // AI spline deviation tracking
    private bool isDeviating = false;
    private float deviationTimer = 0f;
    private Vector3 deviationDirection;

    void Start()
    {
        if (carRigidbody == null)
            carRigidbody = GetComponent<Rigidbody>();
        
        if (carVisual == null)
            carVisual = transform.GetChild(0);
        
        if (carRigidbody != null)
        {
            carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
        }
    }

    void Update()
    {
        HandleRolloverAnimation();
        HandleRecovery();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & carLayer) != 0)
        {
            HandleCarCollision(collision);
        }
    }

    void HandleCarCollision(Collision collision)
    {
        CarCollisionSystem otherCar = collision.gameObject.GetComponent<CarCollisionSystem>();
        if (otherCar == null) return;

        // Calculate collision parameters
        float mySpeed = carRigidbody.velocity.magnitude;
        float otherSpeed = otherCar.carRigidbody.velocity.magnitude;
        float relativeSpeed = (carRigidbody.velocity - otherCar.carRigidbody.velocity).magnitude;
        
        // Get collision point and direction
        Vector3 collisionPoint = collision.contacts[0].point;
        Vector3 collisionNormal = collision.contacts[0].normal;
        Vector3 relativeVelocity = carRigidbody.velocity - otherCar.carRigidbody.velocity;
        
        // Calculate hit angle (0 = head-on, 90 = side hit)
        float hitAngle = Vector3.Angle(transform.forward, -collisionNormal);
        bool isSideHit = hitAngle > sideHitAngleThreshold && hitAngle < (180f - sideHitAngleThreshold);
        
        // Determine who is the aggressor (faster car)
        bool iAmAggressor = mySpeed > otherSpeed;
        
        // Calculate mass ratio for physics
        float massRatio = carRigidbody.mass / otherCar.carRigidbody.mass;
        
        // Determine collision outcome for OTHER car
        if (iAmAggressor)
        {
            DetermineCollisionOutcome(
                otherCar, 
                relativeSpeed, 
                collisionNormal, 
                collisionPoint,
                isSideHit,
                massRatio
            );
        }
        
        // Apply reaction force to this car (reduced compared to other car)
        ApplyCollisionReaction(relativeSpeed * 0.5f, -collisionNormal, isSideHit);
    }

    void DetermineCollisionOutcome(
        CarCollisionSystem targetCar, 
        float impactSpeed, 
        Vector3 direction,
        Vector3 impactPoint,
        bool isSideHit,
        float massRatio)
    {
        // Calculate adjusted probabilities based on conditions
        float adjustedRolloverChance = rolloverChance;
        float adjustedSpinChance = spinOutChance;
        
        // Increase rollover chance for side hits
        if (isSideHit)
        {
            adjustedRolloverChance += sideHitRolloverBonus;
        }
        
        // Speed factors
        float speedFactor = Mathf.Clamp01(impactSpeed / 30f);
        adjustedRolloverChance *= (1f + speedFactor);
        adjustedSpinChance *= (1f + speedFactor * 0.5f);
        
        // Mass ratio influence (lighter cars rollover easier)
        adjustedRolloverChance *= (2f - massRatio);
        
        // Decide outcome
        float roll = Random.Range(0f, 1f);
        
        if (impactSpeed >= minSpeedForRollover && roll < adjustedRolloverChance)
        {
            // ROLLOVER
            targetCar.InitiateRollover(direction, impactPoint, impactSpeed);
        }
        else if (impactSpeed >= minSpeedForSpinOut && roll < (adjustedRolloverChance + adjustedSpinChance))
        {
            // SPIN-OUT
            targetCar.InitiateSpinOut(direction, impactSpeed);
        }
        else if (impactSpeed >= minPushSpeed)
        {
            // PHYSICS PUSH
            targetCar.InitiatePhysicsPush(direction, impactSpeed);
        }
        
        // AI spline deviation (always happens on significant impact)
        if (canDeviateFromSpline && impactSpeed > 5f)
        {
            targetCar.InitiateSplineDeviation(direction, impactSpeed);
        }
    }

    public void InitiateRollover(Vector3 direction, Vector3 impactPoint, float force)
    {
        if (isRollingOver) return;
        
        isRollingOver = true;
        rolloverTimer = 0f;
        originalRotation = carVisual.localRotation;
        
        // Determine rollover axis based on impact direction
        Vector3 localImpact = transform.InverseTransformPoint(impactPoint);
        bool rollLeft = localImpact.x < 0;
        
        // Create dramatic roll rotation (90-180 degrees)
        float rollAmount = Random.Range(90f, 180f) * (rollLeft ? -1 : 1);
        targetRollRotation = Quaternion.Euler(0, 0, rollAmount) * originalRotation;
        
        // Apply upward and rotational force
        if (carRigidbody != null)
        {
            carRigidbody.AddForce(Vector3.up * force * rolloverTorqueMultiplier, ForceMode.Impulse);
            Vector3 torqueAxis = rollLeft ? -transform.forward : transform.forward;
            carRigidbody.AddTorque(torqueAxis * force * rolloverTorqueMultiplier, ForceMode.Impulse);
        }
    }

    public void InitiateSpinOut(Vector3 direction, float force)
    {
        if (carRigidbody == null) return;
        
        // Apply spinning torque around Y-axis
        float spinDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
        Vector3 spinTorque = Vector3.up * force * spinForceMultiplier * spinDirection;
        carRigidbody.AddTorque(spinTorque, ForceMode.Impulse);
        
        // Add lateral push
        Vector3 lateralDirection = Vector3.Cross(Vector3.up, direction).normalized;
        carRigidbody.AddForce(lateralDirection * force * pushForceMultiplier, ForceMode.Impulse);
    }

    public void InitiatePhysicsPush(Vector3 direction, float force)
    {
        if (carRigidbody == null) return;
        
        // Simple physics push with slight randomization
        Vector3 pushDirection = direction.normalized;
        pushDirection += new Vector3(
            Random.Range(-0.2f, 0.2f), 
            0, 
            Random.Range(-0.2f, 0.2f)
        );
        
        carRigidbody.AddForce(pushDirection * force * pushForceMultiplier, ForceMode.Impulse);
    }

    public void InitiateSplineDeviation(Vector3 direction, float force)
    {
        isDeviating = true;
        deviationTimer = 0f;
        deviationDirection = direction.normalized;
        
        // Add immediate deviation force
        if (carRigidbody != null)
        {
            carRigidbody.AddForce(deviationDirection * deviationForce * force * 0.1f, ForceMode.Impulse);
        }
    }

    void HandleRolloverAnimation()
    {
        if (!isRollingOver) return;
        
        rolloverTimer += Time.deltaTime;
        float progress = rolloverTimer / rolloverDuration;
        
        if (progress < 1f)
        {
            // Animate the rollover using visual transform
            carVisual.localRotation = Quaternion.Slerp(
                originalRotation, 
                targetRollRotation, 
                progress
            );
        }
        else
        {
            // Rollover complete, start recovery
            isRollingOver = false;
            isRecovering = true;
            recoveryTimer = 0f;
            originalRotation = carVisual.localRotation;
        }
    }

    void HandleRecovery()
    {
        if (!isRecovering) return;
        
        recoveryTimer += Time.deltaTime;
        float progress = recoveryTimer / recoveryDuration;
        
        if (progress < 1f)
        {
            // Smoothly return to upright position
            carVisual.localRotation = Quaternion.Slerp(
                originalRotation,
                Quaternion.identity,
                progress
            );
        }
        else
        {
            carVisual.localRotation = Quaternion.identity;
            isRecovering = false;
        }
    }

    void ApplyCollisionReaction(float force, Vector3 direction, bool isSideHit)
    {
        if (carRigidbody == null) return;
        
        // Less dramatic reaction for the aggressor
        float reactionMultiplier = isSideHit ? 0.7f : 0.5f;
        carRigidbody.AddForce(direction * force * reactionMultiplier, ForceMode.Impulse);
    }

    // Public getters for AI controllers
    public bool IsDeviating() => isDeviating;
    public Vector3 GetDeviationDirection() => deviationDirection;
    public float GetDeviationProgress() => deviationTimer / deviationDuration;
    
    public void CompleteDeviation()
    {
        if (deviationTimer >= deviationDuration)
        {
            isDeviating = false;
            deviationTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        // Update deviation timer
        if (isDeviating)
        {
            deviationTimer += Time.fixedDeltaTime;
            if (deviationTimer >= deviationDuration)
            {
                isDeviating = false;
            }
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (isDeviating)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, deviationDirection * 3f);
        }
    }
}