using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PickupPoint : MonoBehaviour
{
    [Header("References")]
    public Passenger passenger;
    public Transform carDoorPoint;
    
    [Header("Settings")]
    public float pickupDelay = 2f;
    
    [Header("Events")]
    public UnityEvent onCarStopped;
    public UnityEvent onPassengerPickedUp;
    
    private bool isCarInRange = false;
    private bool passengerPickedUp = false;
    private PassengerCarrier currentCar;
    
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        
        LogHelper.Log($"PickupPoint '{gameObject.name}' initialized. Trigger: {col.isTrigger}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        LogHelper.Log($"OnTriggerEnter detected: {other.gameObject.name} (Tag: {other.tag})");
        
        // IMPORTANT: Check parent for PassengerCarrier since collider is on child
        PassengerCarrier car = other.GetComponent<PassengerCarrier>();
        
        // If not found on this GameObject, check parent
        if (car == null)
        {
            car = other.GetComponentInParent<PassengerCarrier>();
            if (car != null)
            {
                LogHelper.Log($"Found PassengerCarrier on parent: {car.gameObject.name}");
            }
        }
        
        if (car != null && !passengerPickedUp)
        {
            LogHelper.Log($"Car detected! Requesting stop...");
            
            isCarInRange = true;
            currentCar = car;
            
            // Request the car to stop
            car.RequestStop();
            
            // Find the door point on the car if not manually assigned
            if (carDoorPoint == null)
            {
                // Search in parent's children for door point
                CarDoorPoint doorPoint = car.GetComponentInChildren<CarDoorPoint>();
                if (doorPoint != null)
                {
                    carDoorPoint = doorPoint.transform;
                    LogHelper.Log($"Found car door point: {carDoorPoint.name}");
                }
                else
                {
                    LogHelper.LogWarning("No CarDoorPoint found on car!");
                }
            }
            
            // Start the pickup sequence after a short delay
            Invoke(nameof(StartPickupSequence), 0.5f);
        }
        else if (car == null)
        {
            LogHelper.Log($"No PassengerCarrier component found on {other.gameObject.name} or its parent");
        }
        else if (passengerPickedUp)
        {
            LogHelper.Log("Passenger already picked up");
        }
    }
    
    private void StartPickupSequence()
    {
        LogHelper.Log($"StartPickupSequence called. InRange: {isCarInRange}, PickedUp: {passengerPickedUp}");
        
        if (!isCarInRange || passengerPickedUp || currentCar == null) return;
        
        onCarStopped?.Invoke();
        
        // Start passenger movement to car
        if (carDoorPoint != null && passenger != null)
        {
            LogHelper.Log($"Starting passenger movement to door");
            
            // Tell the passenger which door to walk to and which pickup point to notify
            passenger.SetPickupPoint(this);
            passenger.StartMovingToCar(carDoorPoint);
        }
        else
        {
            LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
        }
    }
    
    // Called by CarDoorPoint when passenger triggers with it
    public void OnPassengerReachedDoor()
    {
        LogHelper.Log($"Passenger reached door - entering car!");
        
        if (passenger == null || currentCar == null || passengerPickedUp) return;
        
        CompletePickup();
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check both the collider and its parent
        PassengerCarrier car = other.GetComponent<PassengerCarrier>();
        if (car == null)
        {
            car = other.GetComponentInParent<PassengerCarrier>();
        }
        
        if (car != null)
        {
            LogHelper.Log($"Car exited pickup zone");
            isCarInRange = false;
            currentCar = null;
        }
    }
    
    private void CompletePickup()
    {
        LogHelper.Log($"CompletePickup called - Passenger entering car!");
        
        if (passenger == null || currentCar == null) return;
        
        // Enter the car
        passenger.EnterCar();
        passengerPickedUp = true;
        
        // Set passenger in car and resume movement
        currentCar.SetPassenger(true);
        currentCar.ResumeFromPickup();
        
        onPassengerPickedUp?.Invoke();
    }

    public void ResetPickupPoint()
    {
        passengerPickedUp = false;
        isCarInRange = false;
        currentCar = null;
        LogHelper.Log("Pickup point reset");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(2f, 1.5f, 0f, 0.3f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}