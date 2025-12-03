using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CloseCallSystem : MonoBehaviour
{
    [Header("Close Call Settings")]
    [SerializeField] private float proDistance = 4f;
    [SerializeField] private float greatDistance = 3f;
    [SerializeField] private float whoahDistance = 2f;
    [SerializeField] private float dangerDistance = 1.5f;

    [Header("Traffic Control Settings")]
    [SerializeField] private float trafficSlowDistance = 6f;
    [SerializeField] private float trafficStopDistance = 4f;
    [SerializeField] private float trafficResumeDistance = 10f;
    // [SerializeField] private float lateralSafetyDistance = 3f;
    [SerializeField] private float slowSpeed = 3f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI closeCallText;
    [SerializeField] private CanvasGroup closeCallCanvasGroup;
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float detectionAngle = 180f;
    [SerializeField] private float trafficDetectionAngle = 70f;

    private float messageTimer = 0f;
    private bool isShowingMessage = false;
    private float lastCloseCallTime = 0f;
    private float closeCallCooldown = 0.5f;

    private void Start()
    {
        if (closeCallText != null)
        {
            closeCallText.enabled = false;
        }

        if (closeCallCanvasGroup != null)
        {
            closeCallCanvasGroup.alpha = 0f;
        }

        LogHelper.Log("CloseCallSystem initialized on Player");
    }

    private void Update()
    {
        DetectTrafficCars();
        UpdateMessageDisplay();
    }

    private void DetectTrafficCars()
    {
        TrafficVehicleBehavior[] allTrafficCars = FindObjectsOfType<TrafficVehicleBehavior>();

        LogHelper.Log($"=== DETECTION FRAME === Found {allTrafficCars.Length} traffic cars");

        foreach (TrafficVehicleBehavior trafficBehavior in allTrafficCars)
        {
            if (trafficBehavior == null) continue;

            Vector3 trafficPosition = trafficBehavior.transform.position;
            Vector3 playerPosition = transform.position;

            // Calculate distance
            float distance = Vector3.Distance(playerPosition, trafficPosition);

            LogHelper.Log($"└─ {trafficBehavior.name}: Distance = {distance:F2}m");

            // Skip if too far
            if (distance > detectionRadius)
            {
                LogHelper.Log($"   └─ TOO FAR (>{detectionRadius}m) - Skipping");
                trafficBehavior.ResumeNormalSpeed();
                continue;
            }

            // Check if player is in front of traffic car
            Vector3 trafficToPlayer = (playerPosition - trafficPosition).normalized;
            Vector3 trafficForward = trafficBehavior.transform.forward;
            float angleFromTraffic = Vector3.Angle(trafficForward, trafficToPlayer);
            bool isPlayerInFrontOfTraffic = angleFromTraffic < trafficDetectionAngle;

            //LogHelper.Log($"Traffic: {trafficBehavior.name}, Distance: {distance:F2}, Angle: {angleFromTraffic:F2}, InFront: {isPlayerInFrontOfTraffic}");
            LogHelper.Log($"   ├─ Angle from traffic: {angleFromTraffic:F2}° (threshold: {trafficDetectionAngle}°)");
            LogHelper.Log($"   ├─ Player in front? {isPlayerInFrontOfTraffic}");
            LogHelper.Log($"   ├─ Traffic forward: {trafficForward}");
            LogHelper.Log($"   └─ Traffic to player: {trafficToPlayer}");



            // Control traffic based on player position
            if (isPlayerInFrontOfTraffic)
            {
                if (distance <= trafficStopDistance)
                {
                    LogHelper.Log($"STOPPING {trafficBehavior.name} - Distance: {distance:F2}");
                    trafficBehavior.StopForPlayer();
                }
                else if (distance <= trafficSlowDistance)
                {
                    LogHelper.Log($"SLOWING {trafficBehavior.name} - Distance: {distance:F2}");
                    trafficBehavior.SlowForPlayer(slowSpeed);
                }
                else
                {

                    trafficBehavior.ResumeNormalSpeed();
                }
            }
            else
            {
                LogHelper.Log($"   └─ Player NOT in front - Resuming");
                // Player not in front, always resume
                trafficBehavior.ResumeNormalSpeed();
                // carsInRange.Remove(trafficBehavior);
            }

            // Handle close call UI (separate check)
            Vector3 directionToTraffic = (trafficPosition - playerPosition).normalized;
            float angleToTraffic = Vector3.Angle(transform.forward, directionToTraffic);
            bool isInPlayerView = angleToTraffic < detectionAngle / 2f;

            if (isInPlayerView && Time.time - lastCloseCallTime >= closeCallCooldown)
            {
                EvaluateCloseCall(distance);
            }
        }
    }


    private void EvaluateCloseCall(float distance)
    {
        string message = "";
        bool horn = false;

        if (distance <= dangerDistance)
        {
            message = "DANGER!";
            horn = true;
        }
        else if (distance <= whoahDistance)
        {
            message = "WHOAH!";
            horn = true;
        }
        else if (distance <= greatDistance)
        {
            message = "GREAT!";
        }
        else if (distance <= proDistance)
        {
            message = "PRO!";
        }

        if (!string.IsNullOrEmpty(message))
        {
            ShowCloseCallMessage(message);
            lastCloseCallTime = Time.time;
        }
    }

    private void ShowCloseCallMessage(string message)
    {
        if (closeCallText == null) return;

        messageTimer = messageDuration;
        isShowingMessage = true;

        closeCallText.text = message;
        closeCallText.enabled = true;

        switch (message)
        {
            case "PRO!":
                closeCallText.color = new Color(0.2f, 1f, 0.2f);
                break;
            case "GREAT!":
                closeCallText.color = new Color(0.3f, 0.8f, 1f);
                break;
            case "WHOAH!":
                closeCallText.color = new Color(1f, 0.8f, 0f);
                break;
            case "DANGER!":
                closeCallText.color = new Color(1f, 0.2f, 0.2f);
                break;
        }
    }

    private void UpdateMessageDisplay()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;

            if (closeCallCanvasGroup != null)
            {
                closeCallCanvasGroup.alpha = Mathf.Lerp(closeCallCanvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }

            if (messageTimer <= 0)
            {
                isShowingMessage = false;
            }
        }
        else if (!isShowingMessage)
        {
            if (closeCallCanvasGroup != null)
            {
                closeCallCanvasGroup.alpha = Mathf.Lerp(closeCallCanvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);

                if (closeCallCanvasGroup.alpha < 0.01f && closeCallText != null)
                {
                    closeCallText.enabled = false;
                }
            }
            else if (closeCallText != null)
            {
                closeCallText.enabled = false;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trafficStopDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, trafficSlowDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, trafficResumeDistance);

    }
}

#region Commented
// private void DetectTrafficCars()
// {
//     TrafficVehicleBehavior[] allTrafficCars = FindObjectsOfType<TrafficVehicleBehavior>();
//     LogHelper.Log($"=== DETECTION FRAME === Found {allTrafficCars.Length} traffic cars");

//     foreach (TrafficVehicleBehavior trafficBehavior in allTrafficCars)
//     {
//         if (trafficBehavior == null) continue;

//         Vector3 trafficPosition = trafficBehavior.transform.position;
//         Vector3 playerPosition = transform.position;

//         float distance = Vector3.Distance(playerPosition, trafficPosition);

//         LogHelper.Log($"└─ {trafficBehavior.name}: Distance = {distance:F2}m");

//         if (distance > detectionRadius)
//         {
//             LogHelper.Log($"   └─ TOO FAR (>{detectionRadius}m) - Skipping");
//             trafficBehavior.ResumeNormalSpeed();
//             continue;
//         }

//         // ✅ NEW: Check if player is ahead using dot product
//         Vector3 trafficToPlayer = playerPosition - trafficPosition;
//         Vector3 trafficForward = trafficBehavior.transform.forward;
//         float dotProduct = Vector3.Dot(trafficForward.normalized, trafficToPlayer.normalized);

//         // Dot product > 0 means player is in front (any angle less than 90°)
//         // You can increase threshold (e.g., 0.3) to make it more strict
//         bool isPlayerInFrontOfTraffic = dotProduct > 0f;

//         LogHelper.Log($"   ├─ Dot product: {dotProduct:F2} (>0 = in front)");
//         LogHelper.Log($"   ├─ Player in front? {isPlayerInFrontOfTraffic}");

//         // Control traffic based on player position
//         if (isPlayerInFrontOfTraffic)
//         {
//             if (distance <= trafficStopDistance)
//             {
//                 LogHelper.Log($"STOPPING {trafficBehavior.name} - Distance: {distance:F2}");
//                 trafficBehavior.StopForPlayer();
//             }
//             else if (distance <= trafficSlowDistance)
//             {
//                 LogHelper.Log($"SLOWING {trafficBehavior.name} - Distance: {distance:F2}");
//                 trafficBehavior.SlowForPlayer(slowSpeed);
//             }
//             else
//             {
//                 trafficBehavior.ResumeNormalSpeed();
//             }
//         }
//         else
//         {
//             LogHelper.Log($"   └─ Player NOT in front - Resuming");
//             trafficBehavior.ResumeNormalSpeed();
//         }

//         // Handle close call UI (separate check)
//         Vector3 directionToTraffic = (trafficPosition - playerPosition).normalized;
//         float angleToTraffic = Vector3.Angle(transform.forward, directionToTraffic);
//         bool isInPlayerView = angleToTraffic < detectionAngle / 2f;

//         if (isInPlayerView && Time.time - lastCloseCallTime >= closeCallCooldown)
//         {
//             EvaluateCloseCall(distance);
//         }
//     }
// }

// private void DetectTrafficCars()
// {
//     TrafficVehicleBehavior[] allTrafficCars = FindObjectsOfType<TrafficVehicleBehavior>();
//     LogHelper.Log($"=== DETECTION FRAME === Found {allTrafficCars.Length} traffic cars");

//     foreach (TrafficVehicleBehavior trafficBehavior in allTrafficCars)
//     {
//         if (trafficBehavior == null) continue;

//         Vector3 trafficPosition = trafficBehavior.transform.position;
//         Vector3 playerPosition = transform.position;

//         // Calculate 3D distance
//         float distance = Vector3.Distance(playerPosition, trafficPosition);

//         // ✅ Calculate distances in traffic car's coordinate system
//         Vector3 trafficToPlayer = playerPosition - trafficPosition;
//         Vector3 trafficForward = trafficBehavior.transform.forward;
//         Vector3 trafficRight = trafficBehavior.transform.right;

//         // Forward distance (how far ahead is player)
//         float forwardDistance = Vector3.Dot(trafficToPlayer, trafficForward);

//         // Lateral distance (how far to the side is player)
//         float lateralDistance = Mathf.Abs(Vector3.Dot(trafficToPlayer, trafficRight));

//         LogHelper.Log($"└─ {trafficBehavior.name}:");
//         LogHelper.Log($"   ├─ 3D Distance: {distance:F2}m");
//         LogHelper.Log($"   ├─ Forward Distance: {forwardDistance:F2}m");
//         LogHelper.Log($"   └─ Lateral Distance: {lateralDistance:F2}m");

//         // Skip if too far
//         if (distance > detectionRadius)
//         {
//             LogHelper.Log($"   └─ TOO FAR - Resuming");
//             trafficBehavior.ResumeNormalSpeed();
//             continue;
//         }

//         // ✅ Determine traffic car state based on position
//         bool isPlayerAhead = forwardDistance > 0f;
//         bool isPlayerClose = distance <= trafficStopDistance;
//         bool isPlayerModerateDistance = distance <= trafficSlowDistance;
//         bool isPlayerClearAhead = forwardDistance > trafficResumeDistance && lateralDistance < lateralSafetyDistance;

//         LogHelper.Log($"   ├─ Player Ahead? {isPlayerAhead}");
//         LogHelper.Log($"   ├─ Player Close? {isPlayerClose}");
//         LogHelper.Log($"   └─ Player Clear Ahead? {isPlayerClearAhead}");

//         // ✅ DECISION LOGIC
//         if (isPlayerClearAhead)
//         {
//             // Player is far ahead and laterally clear - RESUME
//             LogHelper.Log($"   ✓ RESUMING - Player clear ahead");
//             trafficBehavior.ResumeNormalSpeed();
//         }
//         else if (isPlayerAhead && isPlayerClose)
//         {
//             // Player is ahead but close - STOP
//             LogHelper.Log($"   ✓ STOPPING - Player ahead and close");
//             trafficBehavior.StopForPlayer();
//         }
//         else if (isPlayerAhead && isPlayerModerateDistance)
//         {
//             // Player is ahead at moderate distance - SLOW
//             LogHelper.Log($"   ✓ SLOWING - Player ahead at moderate distance");
//             trafficBehavior.SlowForPlayer(slowSpeed);
//         }
//         else if (!isPlayerAhead)
//         {
//             // Player is behind - RESUME
//             LogHelper.Log($"   ✓ RESUMING - Player behind");
//             trafficBehavior.ResumeNormalSpeed();
//         }

//         // Handle close call UI (separate check)
//         Vector3 directionToTraffic = (trafficPosition - playerPosition).normalized;
//         float angleToTraffic = Vector3.Angle(transform.forward, directionToTraffic);
//         bool isInPlayerView = angleToTraffic < detectionAngle / 2f;

//         if (isInPlayerView && Time.time - lastCloseCallTime >= closeCallCooldown)
//         {
//             EvaluateCloseCall(distance);
//         }
//     }
// }
// using UnityEngine;
// using TMPro;

// public class CloseCallSystem : MonoBehaviour
// {
//     [Header("Close Call Settings")]
//     [SerializeField] private float proDistance = 4f;
//     [SerializeField] private float greatDistance = 3f;
//     [SerializeField] private float whoahDistance = 2f;
//     [SerializeField] private float dangerDistance = 1.5f;

//     [Header("Traffic Control Settings")]
//     [SerializeField] private float trafficSlowDistance = 4f;
//     [SerializeField] private float trafficStopDistance = 2f;
//     [SerializeField] private float trafficResumeDistance = 5f;
//     [SerializeField] private float slowSpeed = 3f;

//     [Header("UI References")]
//     [SerializeField] private TextMeshProUGUI closeCallText;
//     [SerializeField] private CanvasGroup closeCallCanvasGroup;
//     [SerializeField] private float messageDuration = 1.5f;
//     [SerializeField] private float fadeSpeed = 3f;

//     [Header("Audio")]
//     [SerializeField] private AudioSource playerHornSource;
//     [SerializeField] private AudioClip[] hornClips;

//     [Header("Detection Settings")]
//     [SerializeField] private LayerMask trafficCarLayer;
//     [SerializeField] private float detectionRadius = 8f;
//     [SerializeField] private float detectionAngle = 180f;

//     private float messageTimer = 0f;
//     private bool isShowingMessage = false;
//     private float lastCloseCallTime = 0f;
//     private float closeCallCooldown = 0.5f;

//     private void Start()
//     {
//         if (closeCallText != null)
//         {
//             closeCallText.enabled = false;
//         }

//         if (closeCallCanvasGroup != null)
//         {
//             closeCallCanvasGroup.alpha = 0f;
//         }
//     }

//     private void Update()
//     {
//         DetectTrafficCars();
//         UpdateMessageDisplay();
//     }

//     private void DetectTrafficCars()
//     {
//         Collider[] nearbyTraffic = Physics.OverlapSphere(transform.position, detectionRadius, trafficCarLayer);

//         foreach (Collider col in nearbyTraffic)
//         {
//             Vector3 directionToTarget = (col.transform.position - transform.position);
//             float distance = directionToTarget.magnitude;
//             Vector3 normalizedDirection = directionToTarget.normalized;

//             // Check angle to player's forward (for close calls)
//             float angleToTarget = Vector3.Angle(transform.forward, normalizedDirection);
//             bool isInPlayerView = angleToTarget < detectionAngle / 2f;

//             // Check if player is in front of traffic car (for traffic stopping)
//             Vector3 trafficToPlayer = (transform.position - col.transform.position).normalized;
//             float angleFromTraffic = Vector3.Angle(col.transform.forward, trafficToPlayer);
//             bool isPlayerInFrontOfTraffic = angleFromTraffic < 45f; // Traffic car's detection angle

//             // Handle close call UI messages
//             if (isInPlayerView && Time.time - lastCloseCallTime >= closeCallCooldown)
//             {
//                 EvaluateCloseCall(distance);
//             }

//             // Handle traffic car behavior
//             if (isPlayerInFrontOfTraffic && distance <= detectionRadius)
//             {
//                 TrafficVehicleBehavior trafficBehavior = col.GetComponentInParent<TrafficVehicleBehavior>();
//                 if (trafficBehavior != null)
//                 {
//                     if (distance <= trafficStopDistance)
//                     {
//                         trafficBehavior.StopForPlayer();
//                     }
//                     else if (distance <= trafficSlowDistance)
//                     {
//                         trafficBehavior.SlowForPlayer(slowSpeed);
//                     }
//                     else if (distance > trafficResumeDistance)
//                     {
//                         trafficBehavior.ResumeNormalSpeed();
//                     }
//                 }
//             }
//             else
//             {
//                 // Player not in front, resume traffic
//                 TrafficVehicleBehavior trafficBehavior = col.GetComponentInParent<TrafficVehicleBehavior>();
//                 if (trafficBehavior != null && distance > trafficResumeDistance)
//                 {
//                     trafficBehavior.ResumeNormalSpeed();
//                 }
//             }
//         }
//     }

//     private void EvaluateCloseCall(float distance)
//     {
//         string message = "";
//         bool shouldPlayHorn = false;

//         if (distance <= dangerDistance)
//         {
//             message = "DANGER!";
//             shouldPlayHorn = true;
//         }
//         else if (distance <= whoahDistance)
//         {
//             message = "WHOAH!";
//             shouldPlayHorn = true;
//         }
//         else if (distance <= greatDistance)
//         {
//             message = "GREAT!";
//         }
//         else if (distance <= proDistance)
//         {
//             message = "PRO!";
//         }

//         if (!string.IsNullOrEmpty(message))
//         {
//             ShowCloseCallMessage(message);
//             lastCloseCallTime = Time.time;

//             if (shouldPlayHorn)
//             {
//                 PlayHorn();
//             }
//         }
//     }

//     private void ShowCloseCallMessage(string message)
//     {
//         if (closeCallText == null) return;

//         messageTimer = messageDuration;
//         isShowingMessage = true;

//         closeCallText.text = message;
//         closeCallText.enabled = true;

//         switch (message)
//         {
//             case "PRO!":
//                 closeCallText.color = new Color(0.2f, 1f, 0.2f);
//                 break;
//             case "GREAT!":
//                 closeCallText.color = new Color(0.3f, 0.8f, 1f);
//                 break;
//             case "WHOAH!":
//                 closeCallText.color = new Color(1f, 0.8f, 0f);
//                 break;
//             case "DANGER!":
//                 closeCallText.color = new Color(1f, 0.2f, 0.2f);
//                 break;
//         }
//     }

//     private void UpdateMessageDisplay()
//     {
//         if (messageTimer > 0)
//         {
//             messageTimer -= Time.deltaTime;

//             if (closeCallCanvasGroup != null)
//             {
//                 closeCallCanvasGroup.alpha = Mathf.Lerp(closeCallCanvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
//             }

//             if (messageTimer <= 0)
//             {
//                 isShowingMessage = false;
//             }
//         }
//         else if (!isShowingMessage)
//         {
//             if (closeCallCanvasGroup != null)
//             {
//                 closeCallCanvasGroup.alpha = Mathf.Lerp(closeCallCanvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);

//                 if (closeCallCanvasGroup.alpha < 0.01f && closeCallText != null)
//                 {
//                     closeCallText.enabled = false;
//                 }
//             }
//             else if (closeCallText != null)
//             {
//                 closeCallText.enabled = false;
//             }
//         }
//     }

//     private void PlayHorn()
//     {
//         if (playerHornSource == null) return;

//         if (hornClips != null && hornClips.Length > 0)
//         {
//             AudioClip clip = hornClips[Random.Range(0, hornClips.Length)];
//             if (clip != null)
//             {
//                 playerHornSource.PlayOneShot(clip);
//             }
//         }
//         else if (!playerHornSource.isPlaying)
//         {
//             playerHornSource.Play();
//         }
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, detectionRadius);

//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(transform.position, proDistance);

//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, greatDistance);

//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, whoahDistance);

//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, dangerDistance);
//     }
// }
#endregion