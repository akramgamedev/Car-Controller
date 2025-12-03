using UnityEngine;

public class TrafficVehicleController : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private float detectionDistance = 8f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float resumeDistance = 5f;
    [SerializeField] private float detectionAngle = 45f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private float slowSpeed = 3f;

    [Header("Horn Settings")]
    [SerializeField] private AudioSource hornAudioSource;
    [SerializeField] private AudioClip hornClip;
    [SerializeField] private float hornCooldown = 3f;

    [Header("References")]
    [SerializeField] private TrafficVehicle trafficVehicle;

    private Transform playerTransform;
    private bool isStopped = false;
    private bool isSlowing = false;
    private float lastHornTime = 0f;
    private float originalSpeed;

    private void Start()
    {
        if (trafficVehicle == null)
        {
            trafficVehicle = GetComponent<TrafficVehicle>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (hornAudioSource == null)
        {
            hornAudioSource = GetComponent<AudioSource>();
        }

        originalSpeed = normalSpeed;
    }

    private void Update()
    {
        if (trafficVehicle != null && !trafficVehicle.IsDisabled)
        {
            DetectPlayer();
        }
    }

    private void DetectPlayer()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;
        Vector3 normalizedDirection = directionToPlayer.normalized;

        // Check if player is in front
        float angleToPlayer = Vector3.Angle(transform.forward, normalizedDirection);
        bool isPlayerInFront = angleToPlayer < detectionAngle;

        if (isPlayerInFront && distanceToPlayer <= detectionDistance)
        {
            // Player is approaching
            if (distanceToPlayer <= stopDistance)
            {
                // Stop completely
                if (!isStopped)
                {
                    StopVehicle();
                    PlayHorn();
                }
            }
            else if (distanceToPlayer <= detectionDistance * 0.5f)
            {
                // Slow down
                if (!isSlowing && !isStopped)
                {
                    SlowDownVehicle();
                }
            }
        }
        else if (isStopped || isSlowing)
        {
            // Player moved away or is not in front anymore
            if (distanceToPlayer > resumeDistance || !isPlayerInFront)
            {
                ResumeVehicle();
            }
        }
    }

    private void StopVehicle()
    {
        isStopped = true;
        isSlowing = false;
        
        if (trafficVehicle != null)
        {
            trafficVehicle.StopMoving();
        }
        
        LogHelper.Log($"Traffic vehicle {gameObject.name} stopped for player");
    }

    private void SlowDownVehicle()
    {
        isSlowing = true;
        
        // Access the moveSpeed through reflection or you can make it public
        ForwardTrafficCar forwardCar = trafficVehicle as ForwardTrafficCar;
        if (forwardCar != null)
        {
            forwardCar.SetSpeed(slowSpeed);
        }
        
        LogHelper.Log($"Traffic vehicle {gameObject.name} slowing down");
    }

    private void ResumeVehicle()
    {
        isStopped = false;
        isSlowing = false;
        
        if (trafficVehicle != null)
        {
            trafficVehicle.StartMoving();
            
            ForwardTrafficCar forwardCar = trafficVehicle as ForwardTrafficCar;
            if (forwardCar != null)
            {
                forwardCar.SetSpeed(normalSpeed);
            }
        }
        
        LogHelper.Log($"Traffic vehicle {gameObject.name} resumed normal speed");
    }

    private void PlayHorn()
    {
        if (Time.time - lastHornTime < hornCooldown) return;

        if (hornAudioSource != null && hornClip != null)
        {
            hornAudioSource.PlayOneShot(hornClip);
            lastHornTime = Time.time;
            LogHelper.Log($"Traffic vehicle {gameObject.name} honked horn");
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("CarHorn");
            lastHornTime = Time.time;
        }
    }

    public void OnPlayerNearby(float distance)
    {
        // Called by CloseCallSystem
        if (distance <= stopDistance * 1.2f)
        {
            PlayHorn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        // Stop distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Resume distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, resumeDistance);

        // Detection cone
        Vector3 forward = transform.forward * detectionDistance;
        Vector3 rightBound = Quaternion.Euler(0, detectionAngle, 0) * forward;
        Vector3 leftBound = Quaternion.Euler(0, -detectionAngle, 0) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
    }
}
