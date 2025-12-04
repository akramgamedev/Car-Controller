using UnityEngine;

public class TrafficVehicleBehavior : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 10f;

    [Header("Horn Settings")]
    [SerializeField] private float hornInterval = 1f;

    [Header("References")]
    [SerializeField] private TrafficVehicle trafficVehicle;
    [SerializeField] private ForwardTrafficCar forwardTrafficCar;

    [Header("Car to Car safety")]
    [SerializeField] private float carSafeDistance = 8f; // Increased for better detection
    [SerializeField] private float carBrakeDistance = 5f; // Increased
    [SerializeField] private float emergencyStopDistance = 2f; // NEW: Emergency stop

    public LayerMask trafficCarLayer;

    private bool isStopped = false;
    private bool isSlowing = false;
    private bool stoppedForPlayer = false;
    private Coroutine hornCoroutine;

    private void Start()
    {
        if (trafficVehicle == null)
        {
            trafficVehicle = GetComponent<TrafficVehicle>();
        }

        if (forwardTrafficCar == null)
        {
            forwardTrafficCar = GetComponent<ForwardTrafficCar>();
        }

        // Debug: Check layer mask
        LogHelper.Log($"TrafficVehicleBehavior initialized on {gameObject.name}");
        LogHelper.Log($"Layer Mask Value: {trafficCarLayer.value}");
        LogHelper.Log($"My Layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
    }

    void Update()
    {
        DetectCarAhead();
    }

    public void StopForPlayer()
    {
        if (isStopped) return;

        isStopped = true;
        stoppedForPlayer = true;
        isSlowing = false;

        if (trafficVehicle != null)
        {
            trafficVehicle.StopMoving();
            LogHelper.Log($"✓ {gameObject.name} STOPPED for PLAYER");
        }
        else
        {
            LogHelper.LogError($"TrafficVehicle is NULL on {gameObject.name}");
        }

        if (hornCoroutine != null)
        {
            StopCoroutine(hornCoroutine);
        }
        hornCoroutine = StartCoroutine(HornRepeatedly());
    }

    private void StopForTrafficCar()
    {
        if (isStopped) return;

        isStopped = true;
        stoppedForPlayer = false;
        isSlowing = false;

        if (trafficVehicle != null)
        {
            trafficVehicle.StopMoving();
            LogHelper.Log($"✓ {gameObject.name} STOPPED for TRAFFIC CAR");
        }
        else
        {
            LogHelper.LogError($"TrafficVehicle is NULL on {gameObject.name}");
        }
    }

    public void SlowForPlayer(float slowSpeed)
    {
        if (isStopped) return;
        if (isSlowing) return;

        isSlowing = true;

        if (forwardTrafficCar != null)
        {
            forwardTrafficCar.SetSpeed(slowSpeed);
            LogHelper.Log($"✓ {gameObject.name} SLOWING to {slowSpeed}");
        }
        else
        {
            LogHelper.LogError($"ForwardTrafficCar is NULL on {gameObject.name}");
        }
    }

    public void ResumeNormalSpeed()
    {
        if (!isStopped && !isSlowing) return;

        bool wasStopped = isStopped;
        isStopped = false;
        isSlowing = false;
        stoppedForPlayer = false;

        if (hornCoroutine != null)
        {
            StopCoroutine(hornCoroutine);
            hornCoroutine = null;
        }

        if (trafficVehicle != null && wasStopped)
        {
            trafficVehicle.StartMoving();
            LogHelper.Log($"✓ {gameObject.name} RESUMED - isMoving now: {trafficVehicle.isMoving}");
        }

        if (forwardTrafficCar != null)
        {
            forwardTrafficCar.SetSpeed(normalSpeed);
        }
    }

    private void DetectCarAhead()
    {
        // Cast ray from slightly above the car
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = transform.forward;

        // Use RaycastAll to detect all cars in front
        RaycastHit[] hits = Physics.RaycastAll(rayStart, rayDirection, carSafeDistance, trafficCarLayer);

        // Debug the raycast
        Debug.DrawRay(rayStart, rayDirection * carSafeDistance, Color.green, 0.1f);

        if (hits.Length > 0)
        {
            // Find the closest car ahead (that's not ourselves)
            RaycastHit closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            bool foundCar = false;

            foreach (RaycastHit hit in hits)
            {
                // Skip if it's ourselves
                if (hit.transform.gameObject == gameObject ||
                    hit.transform.IsChildOf(transform) ||
                    transform.IsChildOf(hit.transform))
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                    foundCar = true;
                }
            }

            if (foundCar)
            {
                TrafficVehicleBehavior carAhead = closestHit.transform.GetComponentInParent<TrafficVehicleBehavior>();

                if (carAhead == null)
                {
                    carAhead = closestHit.transform.GetComponent<TrafficVehicleBehavior>();
                }

                if (carAhead != null && carAhead != this)
                {
                    float distance = closestHit.distance;

                    LogHelper.Log($"{gameObject.name} detected {carAhead.gameObject.name} at distance {distance:F2}");

                    // EMERGENCY STOP - Very close!
                    if (distance <= emergencyStopDistance)
                    {
                        LogHelper.LogWarning($"EMERGENCY STOP! {gameObject.name} too close to {carAhead.gameObject.name}");
                        StopForTrafficCar();
                        return;
                    }

                    // If car ahead is stopped, we should stop too
                    if (carAhead.isStopped)
                    {
                        if (distance <= carBrakeDistance)
                        {
                            StopForTrafficCar();
                            return;
                        }
                        else
                        {
                            // Slow down as we approach
                            float slowSpeed = Mathf.Lerp(2f, normalSpeed * 0.3f, distance / carBrakeDistance);
                            SlowForPlayer(slowSpeed);
                            return;
                        }
                    }
                    // If car ahead is slowing, we should slow too
                    else if (carAhead.isSlowing)
                    {
                        SlowForPlayer(forwardTrafficCar == null ? 3f : forwardTrafficCar.moveSpeed * 0.7f);
                        return;
                    }
                    // Car ahead is moving normally but close, slow down a bit
                    else if (distance <= carBrakeDistance)
                    {
                        SlowForPlayer(normalSpeed * 0.8f);
                        return;
                    }
                }
            }
        }

        // Only resume if we're not stopped for player
        if (!stoppedForPlayer && (isStopped || isSlowing))
        {
            ResumeNormalSpeed();
        }
    }

    // Alternative detection using trigger colliders (if raycast fails)
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & trafficCarLayer) != 0)
        {
            TrafficVehicleBehavior otherCar = other.GetComponentInParent<TrafficVehicleBehavior>();
            if (otherCar != null && otherCar != this)
            {
                LogHelper.LogWarning($"TRIGGER COLLISION WARNING: {gameObject.name} entered trigger of {other.gameObject.name}");
                StopForTrafficCar();
            }
        }
    }

    private System.Collections.IEnumerator HornRepeatedly()
    {
        while (isStopped && stoppedForPlayer)
        {
            PlayHorn();
            yield return new WaitForSeconds(hornInterval);
        }
    }

    private void PlayHorn()
    {
        AudioManager.Instance.PlaySFX("CarBeep");
        LogHelper.Log($"Horn played (AudioManager) on {gameObject.name}");
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        // Draw emergency stop distance (RED)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayStart, rayStart + transform.forward * emergencyStopDistance);
        Gizmos.DrawWireSphere(rayStart + transform.forward * emergencyStopDistance, 0.3f);

        // Draw brake distance (YELLOW)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayStart + transform.forward * emergencyStopDistance,
                        rayStart + transform.forward * carBrakeDistance);
        Gizmos.DrawWireSphere(rayStart + transform.forward * carBrakeDistance, 0.3f);

        // Draw safe distance (GREEN)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayStart + transform.forward * carBrakeDistance,
                        rayStart + transform.forward * carSafeDistance);
        Gizmos.DrawWireSphere(rayStart + transform.forward * carSafeDistance, 0.3f);
    }
}


// using UnityEngine;

// public class TrafficVehicleBehavior : MonoBehaviour
// {
//     [Header("Speed Settings")]
//     [SerializeField] private float normalSpeed = 10f;

//     [Header("Horn Settings")]

//     [SerializeField] private float hornInterval = 1f;

//     [Header("References")]
//     [SerializeField] private TrafficVehicle trafficVehicle;
//     [SerializeField] private ForwardTrafficCar forwardTrafficCar;

//     [Header("Car to Car safety")]
//     [SerializeField] private float carSafeDistance = 5f;
//     [SerializeField] private float carBrakeDistance = 3f;

//     public LayerMask trafficCarLayer;

//     private bool isStopped = false;
//     private bool isSlowing = false;
//     private bool stoppedForPlayer = false;
//     private Coroutine hornCoroutine;

//     private void Start()
//     {
//         if (trafficVehicle == null)
//         {
//             trafficVehicle = GetComponent<TrafficVehicle>();
//         }

//         if (forwardTrafficCar == null)
//         {
//             forwardTrafficCar = GetComponent<ForwardTrafficCar>();
//         }


//         LogHelper.Log($"TrafficVehicleBehavior initialized on {gameObject.name}");
//     }

//     void Update()
//     {
//         DetectCarAhead();
//     }

//     public void StopForPlayer()
//     {
//         if (isStopped) return;

//         isStopped = true;
//         stoppedForPlayer = true;
//         isSlowing = false;

//         if (trafficVehicle != null)
//         {
//             trafficVehicle.StopMoving();
//             LogHelper.Log($"✓ {gameObject.name} STOPPED");
//         }
//         else
//         {
//             LogHelper.LogError($"TrafficVehicle is NULL on {gameObject.name}");
//         }

//         if (hornCoroutine != null)
//         {
//             StopCoroutine(hornCoroutine);
//         }
//         hornCoroutine = StartCoroutine(HornRepeatedly());

//     }

//     private void StopForTrafficCar()
//     {
//         if (isStopped) return;

//         isStopped = true;
//         stoppedForPlayer = false; // Not stopped for player
//         isSlowing = false;

//         if (trafficVehicle != null)
//         {
//             trafficVehicle.StopMoving();
//             LogHelper.Log($"✓ {gameObject.name} STOPPED for TRAFFIC CAR");
//         }
//         else
//         {
//             LogHelper.LogError($"TrafficVehicle is NULL on {gameObject.name}");
//         }
//     }

//     public void SlowForPlayer(float slowSpeed)
//     {
//         if (isStopped) return;
//         if (isSlowing) return;

//         isSlowing = true;

//         if (forwardTrafficCar != null)
//         {
//             forwardTrafficCar.SetSpeed(slowSpeed);
//             LogHelper.Log($"✓ {gameObject.name} SLOWING to {slowSpeed}");
//         }
//         else
//         {
//             LogHelper.LogError($"ForwardTrafficCar is NULL on {gameObject.name}");
//         }
//     }

//     bool canwork;
//     public void ResumeNormalSpeed()
//     {

//         if (!isStopped && !isSlowing) return;

//         bool wasStopped = isStopped;
//         isStopped = false;
//         isSlowing = false;
//         stoppedForPlayer = false;

//         if (hornCoroutine != null)
//         {
//             StopCoroutine(hornCoroutine);
//             hornCoroutine = null;
//         }

//         if (trafficVehicle != null && wasStopped)
//         {
//             trafficVehicle.StartMoving();
//             LogHelper.Log($"✓ {gameObject.name} RESUMED - isMoving now: {trafficVehicle.isMoving}");
//         }

//         if (forwardTrafficCar != null)
//         {
//             forwardTrafficCar.SetSpeed(normalSpeed);
//         }
//     }

//     private void DetectCarAhead()
//     {
//         RaycastHit hit;

//         if (Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out hit, carSafeDistance, trafficCarLayer))
//         {
//             TrafficVehicleBehavior carAhead = hit.transform.GetComponentInParent<TrafficVehicleBehavior>();

//             if (carAhead != null)
//             {
//                 // If car ahead is stopped, we should stop too
//                 if (carAhead.isStopped)
//                 {
//                     if (hit.distance <= carBrakeDistance)
//                     {
//                         StopForTrafficCar(); // Use new method without horn
//                         return;
//                     }
//                     else
//                     {
//                         // Slow down if we're not close enough to stop yet
//                         SlowForPlayer(forwardTrafficCar == null ? 3f : forwardTrafficCar.moveSpeed * 0.5f);
//                         return;
//                     }
//                 }
//                 // If car ahead is slowing, we should slow too
//                 else if (carAhead.isSlowing)
//                 {
//                     SlowForPlayer(forwardTrafficCar == null ? 3f : forwardTrafficCar.moveSpeed * 0.5f);
//                     return;
//                 }
//             }
//         }

//         // Only resume if we're not stopped for player
//         // Cars stopped for player should only resume when player moves away
//         if (!stoppedForPlayer && (isStopped || isSlowing))
//         {
//             ResumeNormalSpeed();
//         }
//     }



//     private System.Collections.IEnumerator HornRepeatedly()
//     {
//         while (isStopped && stoppedForPlayer)
//         {
//             PlayHorn();
//             yield return new WaitForSeconds(hornInterval);
//         }
//     }

//     private void PlayHorn()
//     {
//         AudioManager.Instance.PlaySFX("CarBeep");
//         LogHelper.Log($"Horn played (AudioManager) on {gameObject.name}");
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.blue;
//         Vector3 forward = transform.forward * 10f;
//         Gizmos.DrawLine(transform.position, transform.position + forward);

//         // Draw detection cone
//         Vector3 rightBound = Quaternion.Euler(0, 70f, 0) * forward;
//         Vector3 leftBound = Quaternion.Euler(0, -70f, 0) * forward;
//         Gizmos.DrawLine(transform.position, transform.position + rightBound);
//         Gizmos.DrawLine(transform.position, transform.position + leftBound);
//     }
// }