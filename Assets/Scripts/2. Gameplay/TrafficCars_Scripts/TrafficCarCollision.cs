using UnityEngine;

public class TrafficCarCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficVehicle vehicleController;

    [Header("Collision Settings")]
    [SerializeField] private bool playCollisionSound = true;

    private bool hasCollided = false;

    private void Start()
    {
        if (vehicleController == null)
        {
            vehicleController = GetComponentInParent<TrafficVehicle>();
        }

        if (vehicleController == null)
        {
            LogHelper.LogError($"TrafficCarCollision on {gameObject.name} could not find TrafficVehicle component in parent");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        bool isPlayerCarBody = false;

        if (collision.gameObject.CompareTag("Car"))
        {
            Transform parent = collision.transform.parent;
            if (parent != null && parent.CompareTag("Player"))
            {
                isPlayerCarBody = true;
            }
        }

        if (isPlayerCarBody)
        {
            hasCollided = true;

            if (playCollisionSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("CarCrash");
            }

            CarCollision playerCarCollision = collision.transform.parent.GetComponent<CarCollision>();
            if (playerCarCollision != null)
            {
                playerCarCollision.OnCarCollision(collision);
            }

            if (vehicleController != null)
            {
                vehicleController.DisabledVehicle();
            }

            LogHelper.Log($"Traffic car {transform.parent?.name ?? gameObject.name} CRASHED with player!");
        }
    }

    public void ResetCollision()
    {
        hasCollided = false;
    }
}

// using UnityEngine;

// public class TrafficCarCollision : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private TrafficVehicle vehicleController;

//     [Header("Collision Settings")]
//     [SerializeField] private float collisionStopDelay = 0.1f;
//     [SerializeField] private bool disableOnPlayerCollision = true;
//     [SerializeField] private bool playCollisionSound = true;

//     private bool hasCollided = false;

//     private void Start()
//     {
//         if (vehicleController == null)
//         {
//             vehicleController = GetComponentInParent<TrafficVehicle>();
//         }

//         if (vehicleController == null)
//         {
//             LogHelper.LogError($"TrafficCarCollision on {gameObject.name} could not find TrafficVehicle component in parent");
//         }
//     }

//     private void OnCollisionEnter(Collision collision)
//     {
//         if (hasCollided) return;

//         bool isPlayerCar = false;

//         if (collision.gameObject.CompareTag("Car"))
//         {
//             Transform parent = collision.transform.parent;
//             if (parent != null && parent.CompareTag("Player"))
//             {
//                 isPlayerCar = true;
//             }
//         }
//         else if (collision.gameObject.CompareTag("Player"))
//         {
//             isPlayerCar = true;
//         }

//         if (isPlayerCar)
//         {
//             hasCollided = true;

//             if (playCollisionSound && AudioManager.Instance != null)
//             {
//                 AudioManager.Instance.PlaySFX("CarCrash");
//             }

//             if (vehicleController != null && disableOnPlayerCollision)
//             {
//                 if (collisionStopDelay > 0)
//                 {
//                     Invoke(nameof(DisableVehicleDelayed), collisionStopDelay);
//                 }
//                 else
//                 {
//                     vehicleController.DisabledVehicle();
//                 }

//                 LogHelper.Log($"Traffic car {transform.parent?.name ?? gameObject.name} collided with player");
//             }
//         }
//     }

//     private void DisableVehicleDelayed()
//     {
//         if (vehicleController != null)
//         {
//             vehicleController.DisabledVehicle();
//         }
//     }

//     public void ResetCollision()
//     {
//         hasCollided = false;
//     }
// }