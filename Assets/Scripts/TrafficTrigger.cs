using UnityEngine;

public class TrafficTrigger : MonoBehaviour
{
    [Header("Traffic Cars to Activate")]
    [SerializeField] private TrafficCarBase[] trafficCars;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") || other.CompareTag("Car"))
        {
            hasTriggered = true;
            foreach (var car in trafficCars)
            {
                car.StartMoving();
            }
        }
    }

}
