using UnityEngine;
using TMPro;
using System.Collections;

public class CloseCallSystem : MonoBehaviour
{
    [Header("Close Call Settings")]
    [SerializeField] private float proDistance = 4f;
    [SerializeField] private float greatDistance = 3f;
    [SerializeField] private float whoahDistance = 2f;
    [SerializeField] private float dangerDistance = 1.5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI closeCallText;
    [SerializeField] private CanvasGroup closeCallCanvasGroup;
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource hornAudioSource;
    [SerializeField] private AudioClip[] hornClips;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask trafficCarLayer;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float detectionAngle = 180f;

    private float messageTimer = 0f;
    private bool isShowingMessage = false;
    private string currentMessage = "";
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
    }

    private void Update()
    {
        DetectCloseCallsWithRaycast();
        UpdateMessageDisplay();
    }

    private void DetectCloseCallsWithRaycast()
    {
        if (Time.time - lastCloseCallTime < closeCallCooldown) return;

        Collider[] nearbyTraffic = Physics.OverlapSphere(transform.position, detectionRadius, trafficCarLayer);

        float closestDistance = float.MaxValue;
        GameObject closestCar = null;

        foreach (Collider col in nearbyTraffic)
        {
            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < detectionAngle / 2f)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCar = col.gameObject;
                }
            }
        }

        if (closestCar != null)
        {
            EvaluateCloseCall(closestDistance, closestCar);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time - lastCloseCallTime < closeCallCooldown) return;

        if (other.CompareTag("Car") || other.transform.parent?.CompareTag("Car") == true)
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            EvaluateCloseCall(distance, other.gameObject);
        }
    }

    private void EvaluateCloseCall(float distance, GameObject trafficCar)
    {
        string message = "";
        bool shouldPlayHorn = false;

        if (distance <= dangerDistance)
        {
            message = "DANGER!";
            shouldPlayHorn = true;
        }
        else if (distance <= whoahDistance)
        {
            message = "WHOAH!";
            shouldPlayHorn = true;
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

            if (shouldPlayHorn)
            {
                PlayHorn();
            }

            // Notify traffic car about close call
            TrafficVehicleController trafficVehicle = trafficCar.GetComponentInParent<TrafficVehicleController>();
            if (trafficVehicle != null)
            {
                trafficVehicle.OnPlayerNearby(distance);
            }
        }
    }

    private void ShowCloseCallMessage(string message)
    {
        if (closeCallText == null) return;

        currentMessage = message;
        messageTimer = messageDuration;
        isShowingMessage = true;

        closeCallText.text = message;
        closeCallText.enabled = true;

        // Set color based on message severity
        switch (message)
        {
            case "PRO!":
                closeCallText.color = new Color(0.2f, 1f, 0.2f); // Green
                break;
            case "GREAT!":
                closeCallText.color = new Color(0.3f, 0.8f, 1f); // Cyan
                break;
            case "WHOAH!":
                closeCallText.color = new Color(1f, 0.8f, 0f); // Yellow
                break;
            case "DANGER!":
                closeCallText.color = new Color(1f, 0.2f, 0.2f); // Red
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

    private void PlayHorn()
    {
        if (hornAudioSource == null) return;

        if (hornClips != null && hornClips.Length > 0)
        {
            AudioClip clip = hornClips[Random.Range(0, hornClips.Length)];
            if (clip != null)
            {
                hornAudioSource.PlayOneShot(clip);
            }
        }
        else if (!hornAudioSource.isPlaying)
        {
            hornAudioSource.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualize close call distances
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, proDistance);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, greatDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, whoahDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);
    }
}