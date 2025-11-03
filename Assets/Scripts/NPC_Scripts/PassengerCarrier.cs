using UnityEngine;

public class PassengerCarrier : MonoBehaviour
{
    [Header("Passenger System")]
    private bool hasPassenger = false;
    public float stoppedSpeedThreshold = 0.3f;
    
    private SplineCarController splineController;
    private bool shouldStopForPickup = false;
    
    private void Awake()
    {
        splineController = GetComponent<SplineCarController>();
        
        if (splineController == null)
        {
            LogHelper.LogError("PassengerCarrier requires SplineCarController component!");
        }
    }
    
    private void Update()
    {
        // Control the forceStopped flag on SplineCarController
        if (splineController != null)
        {
            splineController.forceStopped = shouldStopForPickup;
        }
    }
    
    public bool IsStopped()
    {
        if (splineController != null)
        {
            return splineController.CurrentSpeed < stoppedSpeedThreshold;
        }
        
        return false;
    }
    
    public void RequestStop()
    {
        LogHelper.Log("Car stop requested!");
        shouldStopForPickup = true;
    }
    
    public void ResumeFromPickup()
    {
        LogHelper.Log("Car resuming!");
        shouldStopForPickup = false;
    }
    
    public bool HasPassenger()
    {
        return hasPassenger;
    }
    
    public void SetPassenger(bool hasPassengerValue)
    {
        hasPassenger = hasPassengerValue;
        LogHelper.Log($"Passenger status: {(hasPassengerValue ? "On board" : "Empty")}");
    }
}