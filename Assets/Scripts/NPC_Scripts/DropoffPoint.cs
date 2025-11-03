using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DropoffPoint : MonoBehaviour
{
    [Header("References")]
    public Passenger passenger;
    public Transform dropPosition; 
    public Transform exitWalkTarget; 
    
    [Header("Settings")]
    public float dropoffDelay = 1f;
    
    [Header("Events")]
    public UnityEvent onCarStopped;
    public UnityEvent onPassengerDroppedOff;
    
    private bool isCarInRange = false;
    private bool passengerDropped = false;
    
    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        
        // Set drop position to this object's position if not assigned
        if (dropPosition == null)
        {
            dropPosition = transform;
        }
        
        // Hide the passenger initially
        if (passenger != null)
        {
            passenger.gameObject.SetActive(false);
            if (exitWalkTarget != null)
            {
                passenger.exitWalkTarget = exitWalkTarget;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") || other.GetComponent<PassengerCarrier>() != null)
        {
            isCarInRange = true;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!isCarInRange || passengerDropped) return;
        
        // Check if car has stopped
        PassengerCarrier car = other.GetComponent<PassengerCarrier>();
        if (car != null && car.IsStopped() && car.HasPassenger())
        {
            OnCarStopped();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car") || other.GetComponent<PassengerCarrier>() != null)
        {
            isCarInRange = false;
        }
    }
    
    private void OnCarStopped()
    {
        if (passengerDropped) return;
        
        onCarStopped?.Invoke();
        Invoke(nameof(CompleteDropoff), dropoffDelay);
    }
    
    private void CompleteDropoff()
    {
        if (passenger != null)
        {
            passenger.ExitCar(dropPosition.position);
            passengerDropped = true;
            onPassengerDroppedOff?.Invoke();
        }
    }
    
    public void ResetDropoffPoint()
    {
        passengerDropped = false;
        isCarInRange = false;
        
        if (passenger != null)
        {
            passenger.gameObject.SetActive(false);
        }
    }
}