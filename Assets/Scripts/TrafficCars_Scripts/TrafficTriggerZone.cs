using System.Collections.Generic;
using UnityEngine;

public class TrafficTriggerZone : MonoBehaviour
{

    [Header("Controlled traffic cars")]
    [Tooltip("Assign the traffic car parent GameObjects (with TrafficVehicle component")]
    [SerializeField] private List<TrafficVehicle> controlledCars = new List<TrafficVehicle>();

    [Header("Trigger Settings")]
    [SerializeField] private bool oneTimeActivation = true;
    // [SerializeField] private bool stopCarsOnExit = false;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (oneTimeActivation && hasBeenTriggered)
            return;

        ActivateTraffic();
        hasBeenTriggered = true;

        LogHelper.Log($"Trigger {gameObject.name} activated by player. Moving {controlledCars.Count} cars.");
    }

    //   private void OnTriggerExit(Collider other)
    //     {
    //         if (stopCarsOnExit && other.CompareTag("Player"))
    //         {
    //             DeactivateTraffic();
    //         }
    //     }

    public void ActivateTraffic()
    {
        foreach (TrafficVehicle car in controlledCars)
        {
            if (car != null)
            {
                car.StartMoving();
                LogHelper.Log("traffic cars activated");
            }
        }
    }
    // public void DeactivateTraffic()
    // {
    //      foreach(TrafficVehicle car in controlledCars)
    //     {
    //         if(car != null)
    //         {
    //             car.StopMoving();
    //         }
    //     }
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw lines to controlled cars when selected
        Gizmos.color = Color.yellow;
        foreach (TrafficVehicle car in controlledCars)
        {
            if (car != null)
            {
                Gizmos.DrawLine(transform.position, car.transform.position);
            }
        }
    }
}
