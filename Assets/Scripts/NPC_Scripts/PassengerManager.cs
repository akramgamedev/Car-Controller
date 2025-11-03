using UnityEngine;
using UnityEngine.Events;

public class PassengerManager : MonoBehaviour
{
    [Header("References")]
    public PassengerCarrier playerCar;
    public PickupPoint[] pickupPoints;
    public DropoffPoint[] dropoffPoints;
    
    [Header("Events")]
    public UnityEvent onPassengerPickedUp;
    public UnityEvent onPassengerDroppedOff;
    
    private int passengersPickedUp = 0;
    private int passengersDroppedOff = 0;
    
    private void Start()
    {
        // Subscribe to pickup and dropoff events
        foreach (var pickup in pickupPoints)
        {
            pickup.onPassengerPickedUp.AddListener(OnPassengerPickedUp);
        }
        
        foreach (var dropoff in dropoffPoints)
        {
            dropoff.onPassengerDroppedOff.AddListener(OnPassengerDroppedOff);
        }
    }
    
    private void OnPassengerPickedUp()
    {
        passengersPickedUp++;
        
        if (playerCar != null)
        {
            playerCar.SetPassenger(true);
        }
        
        onPassengerPickedUp?.Invoke();
        Debug.Log($"Passenger picked up! Total: {passengersPickedUp}");
    }
    
    private void OnPassengerDroppedOff()
    {
        passengersDroppedOff++;
        
        if (playerCar != null)
        {
            playerCar.SetPassenger(false);
        }
        
        onPassengerDroppedOff?.Invoke();
        Debug.Log($"Passenger dropped off! Total: {passengersDroppedOff}");
    }
    
    public void ResetAllPoints()
    {
        foreach (var pickup in pickupPoints)
        {
            pickup.ResetPickupPoint();
            if (pickup.passenger != null)
            {
                pickup.passenger.ResetPassenger();
            }
        }
        
        foreach (var dropoff in dropoffPoints)
        {
            dropoff.ResetDropoffPoint();
        }
        
        passengersPickedUp = 0;
        passengersDroppedOff = 0;
        
        if (playerCar != null)
        {
            playerCar.SetPassenger(false);
        }
    }
    
    // Optional: Display stats
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 40, 300, 30), $"Picked up: {passengersPickedUp} | Dropped off: {passengersDroppedOff}");
    }
}
