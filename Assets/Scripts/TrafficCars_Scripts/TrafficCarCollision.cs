using UnityEngine;

public class TrafficCarCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrafficVehicle vehicleController;

    private void Start()
    {
        if (vehicleController == null)
        {
            vehicleController = GetComponentInParent<TrafficVehicle>();
        }

        if (vehicleController == null)
        {
            LogHelper.LogError($"TrafficCarCollision on {gameObject} could not find TrafficVehicle componenet in parent");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {

            Transform parent = collision.transform.parent;
            if (parent != null && parent.CompareTag("Player"))
            {
                if (vehicleController != null)
                {
                    vehicleController.DisabledVehicle();
                    LogHelper.Log($"Traffic car {transform.parent.name} disabled after collision with player");
                }
            }
        }
    }
}
