using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class CloseCallSystem : MonoBehaviour
{
    [Header("Close Call Settings")]
    [SerializeField] private float proDistance = 5.58f;
    [SerializeField] private float greatDistance = 4.9f;
    [SerializeField] private float whoahDistance = 4.04f;
    [SerializeField] private float dangerDistance = 3.49f;

    [Header("Traffic Control Settings")]
    [SerializeField] private float trafficSlowDistance = 8f;
    [SerializeField] private float trafficStopDistance = 5f;
    [SerializeField] private float trafficResumeDistance = 6.2f;
    [SerializeField] private float slowSpeed = 3f;

    [Header("UI References")]
    [SerializeField] private Image closeCallImage;
    [SerializeField] private Sprite proSprite;
    [SerializeField] private Sprite greatSprite;
    [SerializeField] private Sprite whoahSprite;
    [SerializeField] private Sprite dangerSprite;
    [SerializeField] private CanvasGroup closeCallCanvasGroup;
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Emoji System")]
    [SerializeField] private CloseCallEmojiSystem emojiSystem;
    [SerializeField] private bool useEmojis = true;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float detectionAngle = 180f;
    [SerializeField] private float trafficDetectionAngle = 70f;

    private float messageTimer = 0f;
    private bool isShowingMessage = false;
    private float lastCloseCallTime = 0f;
    private float closeCallCooldown = 0.5f;

    private string currentBestMessage = "";
    private float closestDangerDistance = float.MaxValue;
    private TrafficVehicleBehavior bestTriggerCar;


    private Dictionary<TrafficVehicleBehavior, float> carCloseCallTimers = new Dictionary<TrafficVehicleBehavior, float>();
    private Dictionary<TrafficVehicleBehavior, string> carLastZone = new Dictionary<TrafficVehicleBehavior, string>();
    private float perCarCooldown = 1.0f;

    private bool hasPlayerCrashed = false;

    private void Start()
    {
        if (closeCallImage != null)
        {
            closeCallImage.enabled = false;
        }

        if (closeCallCanvasGroup != null)
        {
            closeCallCanvasGroup.alpha = 0f;
        }

        if (emojiSystem == null && useEmojis)
        {
            LogHelper.LogWarning("CloseCallEmojiSystem not assigned! Emojis will not be shown.");
        }

        LogHelper.Log("CloseCallSystem initialized on Player");
    }

    private void Update()
    {
        if (!hasPlayerCrashed)
        {
            DetectTrafficCars();
        }
        UpdateMessageDisplay();
        UpdateCarCooldowns();
    }

    public void OnPlayerCrash()
    {
        hasPlayerCrashed = true;
        HideCloseCallMessage();
        LogHelper.Log("CloseCallSystem: Player crashed - stopping close call detection");

    }

    public void ResetCrashState()
    {
        hasPlayerCrashed = false;
        LogHelper.Log("CloseCallSystem: Crash state reset - close calls enabled again");
    }

    private void UpdateCarCooldowns()
    {
        List<TrafficVehicleBehavior> carsToRemove = new List<TrafficVehicleBehavior>();

        foreach (var kvp in carCloseCallTimers)
        {
            if (Time.time - kvp.Value >= perCarCooldown)
            {
                carsToRemove.Add(kvp.Key);
            }
        }
        foreach (var car in carsToRemove)
        {
            carCloseCallTimers.Remove(car);
            carLastZone.Remove(car);
        }

    }

    private void DetectTrafficCars()
    {
        TrafficVehicleBehavior[] allTrafficCars = FindObjectsOfType<TrafficVehicleBehavior>();

        currentBestMessage = "";
        closestDangerDistance = float.MaxValue;
        bestTriggerCar = null;

        foreach (TrafficVehicleBehavior trafficBehavior in allTrafficCars)
        {
            if (trafficBehavior == null) continue;

            Vector3 trafficPosition = trafficBehavior.transform.position;
            Vector3 playerPosition = transform.position;
            float distance = Vector3.Distance(playerPosition, trafficPosition);

            if (distance > detectionRadius)
            {
                trafficBehavior.ResumeNormalSpeed();
                continue;
            }

            Vector3 trafficToPlayer = (playerPosition - trafficPosition).normalized;
            Vector3 trafficForward = trafficBehavior.transform.forward;
            float angleFromTraffic = Vector3.Angle(trafficForward, trafficToPlayer);
            bool isPlayerInFrontOfTraffic = angleFromTraffic < trafficDetectionAngle;

            if (isPlayerInFrontOfTraffic)
            {
                if (distance <= trafficStopDistance)
                {
                    trafficBehavior.StopForPlayer();
                }
                else if (distance <= trafficSlowDistance)
                {
                    trafficBehavior.SlowForPlayer(slowSpeed);
                }
                else
                {
                    trafficBehavior.ResumeNormalSpeed();
                }
            }
            else
            {
                trafficBehavior.ResumeNormalSpeed();
            }

            Vector3 directionToTraffic = (trafficPosition - playerPosition).normalized;
            float angleToTraffic = Vector3.Angle(transform.forward, directionToTraffic);
            bool isInPlayerView = angleToTraffic < detectionAngle / 2f;

            bool canTriggerCloseCall = !carCloseCallTimers.ContainsKey(trafficBehavior) ||
                                       (Time.time - carCloseCallTimers[trafficBehavior] >= perCarCooldown);

            if (isInPlayerView && canTriggerCloseCall && distance <= proDistance)
            {
                string thisZone = DetermineZone(distance);
                if (!string.IsNullOrEmpty(thisZone))
                {
                    int thisDanger = GetZoneDangerLevel(thisZone);

                    int bestDangerSoFar = string.IsNullOrEmpty(currentBestMessage) ? 0 : GetZoneDangerLevel(currentBestMessage);
                    if (thisDanger > bestDangerSoFar ||
                        (thisDanger == bestDangerSoFar && distance < closestDangerDistance))
                    {
                        currentBestMessage = thisZone;
                        closestDangerDistance = distance;
                        bestTriggerCar = trafficBehavior;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(currentBestMessage) && Time.time - lastCloseCallTime >= closeCallCooldown)
        {
            ShowCloseCallMessage(currentBestMessage);

            if (useEmojis && emojiSystem != null)
            {
                emojiSystem.ShowEmoji(currentBestMessage);
            }

            lastCloseCallTime = Time.time;
            if (bestTriggerCar != null)
            {
                carCloseCallTimers[bestTriggerCar] = Time.time;
                carLastZone[bestTriggerCar] = currentBestMessage;
            }
        }
    }

    //********************************************************

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

    //         Vector3 trafficToPlayer = (playerPosition - trafficPosition).normalized;
    //         Vector3 trafficForward = trafficBehavior.transform.forward;
    //         float angleFromTraffic = Vector3.Angle(trafficForward, trafficToPlayer);
    //         bool isPlayerInFrontOfTraffic = angleFromTraffic < trafficDetectionAngle;

    //         LogHelper.Log($"   ├─ Angle from traffic: {angleFromTraffic:F2}° (threshold: {trafficDetectionAngle}°)");
    //         LogHelper.Log($"   ├─ Player in front? {isPlayerInFrontOfTraffic}");
    //         LogHelper.Log($"   ├─ Traffic forward: {trafficForward}");
    //         LogHelper.Log($"   └─ Traffic to player: {trafficToPlayer}");



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

    //         Vector3 directionToTraffic = (trafficPosition - playerPosition).normalized;
    //         float angleToTraffic = Vector3.Angle(transform.forward, directionToTraffic);
    //         bool isInPlayerView = angleToTraffic < detectionAngle / 2f;

    //         bool canTriggerCloseCall = !carCloseCallTimers.ContainsKey(trafficBehavior) ||
    //         (Time.time - carCloseCallTimers[trafficBehavior] >= perCarCooldown);

    //         if (isInPlayerView && canTriggerCloseCall)
    //         {
    //             EvaluateCloseCall(distance, trafficBehavior, trafficPosition, trafficForward);
    //         }
    //     }
    // }


    // private void EvaluateCloseCall(float distance, TrafficVehicleBehavior trafficCar, Vector3 trafficPosition, Vector3 trafficForward)
    // {

    //     string currentZone = DetermineZone(distance);

    //     if (!string.IsNullOrEmpty(currentZone))
    //     {
    //         bool shouldTrigger = false;

    //         if (!carLastZone.ContainsKey(trafficCar))
    //         {
    //             shouldTrigger = true;
    //             LogHelper.Log($" └─ NEW CAR in {currentZone} zone");
    //         }

    //         else if (carLastZone[trafficCar] != currentZone)
    //         {
    //             int oldZoneDanger = GetZoneDangerLevel(carLastZone[trafficCar]);
    //             int newZoneDanger = GetZoneDangerLevel(currentZone);

    //             if (newZoneDanger > oldZoneDanger)
    //             {
    //                 shouldTrigger = true;
    //                 LogHelper.Log($"   └─ ZONE CHANGE: {carLastZone[trafficCar]} → {currentZone} (more dangerous!)");
    //             }
    //             else
    //             {
    //                 LogHelper.Log($"   └─ ZONE CHANGE IGNORED: {carLastZone[trafficCar]} → {currentZone} (less dangerous)");
    //             }
    //         }

    //         if (shouldTrigger)
    //         {
    //             ShowCloseCallMessage(currentZone);

    //             if (useEmojis && emojiSystem != null)
    //             {
    //                 emojiSystem.ShowEmoji(currentZone);
    //             }

    //             lastCloseCallTime = Time.time;
    //             carCloseCallTimers[trafficCar] = Time.time;
    //             carLastZone[trafficCar] = currentZone;
    //         }
    //     }

    // }

    private string DetermineZone(float distance)
    {
        if (distance <= dangerDistance)
            return "DANGER!";
        else if (distance <= whoahDistance)
            return "WHOAH!";
        else if (distance <= greatDistance)
            return "GREAT!";
        else if (distance <= proDistance)
            return "PRO!";

        return "";
    }
    private int GetZoneDangerLevel(string zone)
    {
        switch (zone)
        {
            case "DANGER!": return 4;
            case "WHOAH!": return 3;
            case "GREAT!": return 2;
            case "PRO!": return 1;
            default: return 0;
        }
    }

    private void ShowCloseCallMessage(string message)
    {
        if (closeCallImage == null) return;

        messageTimer = messageDuration;
        isShowingMessage = true;

        closeCallImage.enabled = true;

        switch (message)
        {
            case "PRO!":
                closeCallImage.sprite = proSprite;
                break;
            case "GREAT!":
                closeCallImage.sprite = greatSprite;
                break;
            case "WHOAH!":
                closeCallImage.sprite = whoahSprite;
                break;
            case "DANGER!":
                closeCallImage.sprite = dangerSprite;
                break;
        }
    }

    private void HideCloseCallMessage()
    {
        messageTimer = 0f;
        isShowingMessage = false;

        if (closeCallCanvasGroup != null)
        {
            closeCallCanvasGroup.alpha = 0f;
        }

        if (closeCallImage != null)
        {
            closeCallImage.enabled = false;
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

                if (closeCallCanvasGroup.alpha < 0.01f && closeCallImage != null)
                {
                    closeCallImage.enabled = false;
                }
            }
            else if (closeCallImage != null)
            {
                closeCallImage.enabled = false;
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

        Gizmos.color = new Color(0f, 0.7f, 1f);
        Gizmos.DrawWireSphere(transform.position, proDistance);

        Gizmos.color = new Color(0.3f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, greatDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, whoahDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);

    }
}