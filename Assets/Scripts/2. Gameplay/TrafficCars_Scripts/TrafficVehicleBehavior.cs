using UnityEngine;

public class TrafficVehicleBehavior : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 10f;

    [Header("Horn Settings")]
    //[SerializeField] private AudioSource hornAudioSource;
    // [SerializeField] private float hornCooldown = 3f;

    [SerializeField] private float hornInterval = 1f;

    [Header("References")]
    [SerializeField] private TrafficVehicle trafficVehicle;
    [SerializeField] private ForwardTrafficCar forwardTrafficCar;

    // private float lastHornTime = 0f;
    private bool isStopped = false;
    private bool isSlowing = false;
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

        // if (hornAudioSource == null)
        // {
        //     hornAudioSource = GetComponent<AudioSource>();
        // }

        LogHelper.Log($"TrafficVehicleBehavior initialized on {gameObject.name}");
    }

    public void StopForPlayer()
    {
        if (isStopped) return;

        isStopped = true;
        isSlowing = false;

        if (trafficVehicle != null)
        {
            trafficVehicle.StopMoving();
            LogHelper.Log($"✓ {gameObject.name} STOPPED");
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
        //PlayHorn();

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

    bool canwork;
    public void ResumeNormalSpeed()
    {

        if (!isStopped && !isSlowing) return;

        bool wasStopped = isStopped;
        isStopped = false;
        isSlowing = false;

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

    private System.Collections.IEnumerator HornRepeatedly()
    {
        while (isStopped)
        {
            PlayHorn();
            yield return new WaitForSeconds(hornInterval);
        }
    }

    private void PlayHorn()
    {
        //if (Time.time - lastHornTime < hornCooldown) return;

        AudioManager.Instance.PlaySFX("CarBeep");
        //lastHornTime = Time.time;

        LogHelper.Log($"Horn played (AudioManager) on {gameObject.name}");

        // if (hornAudioSource != null && hornClip != null)
        // {
        //     hornAudioSource.PlayOneShot(hornClip);
        //     lastHornTime = Time.time;
        //     LogHelper.Log($"Horn played on {gameObject.name}");
        // }
        // else if (AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.PlaySFX("CarBeep");
        //     lastHornTime = Time.time;
        // }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 forward = transform.forward * 10f;
        Gizmos.DrawLine(transform.position, transform.position + forward);

        // Draw detection cone
        Vector3 rightBound = Quaternion.Euler(0, 70f, 0) * forward;
        Vector3 leftBound = Quaternion.Euler(0, -70f, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
    }
}


// using UnityEngine;

// public class TrafficVehicleBehavior : MonoBehaviour
// {
//     [Header("Speed Settings")]
//     [SerializeField] private float normalSpeed = 10f;

//     [Header("Horn Settings")]
//     [SerializeField] private AudioSource hornAudioSource;
//     [SerializeField] private AudioClip hornClip;
//     [SerializeField] private float hornCooldown = 3f;

//     [Header("References")]
//     [SerializeField] private TrafficVehicle trafficVehicle;
//     [SerializeField] private ForwardTrafficCar forwardTrafficCar;

//     private float lastHornTime = 0f;
//     private bool isStopped = false;
//     private bool isSlowing = false;

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

//         if (hornAudioSource == null)
//         {
//             hornAudioSource = GetComponent<AudioSource>();
//         }
//     }

//     public void StopForPlayer()
//     {
//         if (isStopped) return;

//         isStopped = true;
//         isSlowing = false;

//         if (trafficVehicle != null)
//         {
//             trafficVehicle.StopMoving();
//         }

//         PlayHorn();
//         LogHelper.Log($"{gameObject.name} stopped for player");
//     }

//     public void SlowForPlayer(float slowSpeed)
//     {
//         if (isStopped || isSlowing) return;

//         isSlowing = true;

//         if (forwardTrafficCar != null)
//         {
//             forwardTrafficCar.SetSpeed(slowSpeed);
//         }

//         LogHelper.Log($"{gameObject.name} slowing down");
//     }

//     public void ResumeNormalSpeed()
//     {
//         if (!isStopped && !isSlowing) return;

//         isStopped = false;
//         isSlowing = false;

//         if (trafficVehicle != null)
//         {
//             trafficVehicle.StartMoving();
//         }

//         if (forwardTrafficCar != null)
//         {
//             forwardTrafficCar.SetSpeed(normalSpeed);
//         }

//         LogHelper.Log($"{gameObject.name} resumed normal speed");
//     }

//     private void PlayHorn()
//     {
//         if (Time.time - lastHornTime < hornCooldown) return;

//         if (hornAudioSource != null && hornClip != null)
//         {
//             hornAudioSource.PlayOneShot(hornClip);
//             lastHornTime = Time.time;
//         }
//         else if (AudioManager.Instance != null)
//         {
//             AudioManager.Instance.PlaySFX("CarHorn");
//             lastHornTime = Time.time;
//         }
//     }
// }
