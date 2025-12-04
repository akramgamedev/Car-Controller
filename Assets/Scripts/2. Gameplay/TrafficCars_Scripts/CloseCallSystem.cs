using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

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
        if (closeCallImage != null)
        {
            closeCallImage.enabled = false;
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
        if (closeCallImage == null) return;

        messageTimer = messageDuration;
        isShowingMessage = true;

        //closeCallImage.text = message;
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

    }
}