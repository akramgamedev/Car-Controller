using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CarDoorPoint : MonoBehaviour
{
    [Header("Visualization")]
    public bool showGizmo = true;
    public Color gizmoColor = Color.green;
    public float gizmoRadius = 0.3f;
    
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        
        LogHelper.Log($"CarDoorPoint '{gameObject.name}' initialized as trigger");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        LogHelper.Log($"CarDoorPoint triggered by: {other.gameObject.name}");

        Passenger passenger = other.GetComponent<Passenger>();
        if (passenger != null && passenger.currentState == Passenger.PassengerState.MovingToCar)
        {
            LogHelper.Log($"Passenger reached door point!");
            
            passenger.OnReachedDoor();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        }
    }
}