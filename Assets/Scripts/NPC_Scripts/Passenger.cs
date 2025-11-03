using UnityEngine;

public class Passenger : MonoBehaviour
{
    [Header("Passenger Settings")]
    public float moveSpeed = 3f;
    public Transform exitWalkTarget;
    
    [Header("States")]
    public PassengerState currentState = PassengerState.WaitingForPickup;
    
    [Header("Animation (Optional)")]
    public Animator animator;
    
    private Transform targetDoor;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private PickupPoint currentPickupPoint;
    
    public enum PassengerState
    {
        WaitingForPickup,
        MovingToCar,
        InCar,
        Dropped,
        WalkingAway
    }
    
    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Try to get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        LogHelper.Log($"Passenger '{gameObject.name}' initialized. State: {currentState}");
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case PassengerState.MovingToCar:
                MoveTowardsDoor();
                break;
            case PassengerState.WalkingAway:
                WalkAway();
                break;
        }
    }
    
    public void StartMovingToCar(Transform doorTransform)
    {
        LogHelper.Log($"StartMovingToCar called. Current State: {currentState}, Door: {doorTransform != null}");
        
        if (currentState != PassengerState.WaitingForPickup)
        {
            LogHelper.LogWarning($"Cannot start moving - wrong state: {currentState}");
            return;
        }
        
        if (doorTransform == null)
        {
            LogHelper.LogError("Door transform is NULL!");
            return;
        }
        
        targetDoor = doorTransform;
        currentState = PassengerState.MovingToCar;
        
        // Set animator to walk
        if (animator != null)
        {
            animator.SetFloat("Speed", moveSpeed / 3f);
        }
        
        LogHelper.Log($"Passenger started moving to car door at {doorTransform.position}");
    }
    
    // NEW METHOD - Set which pickup point to notify
    public void SetPickupPoint(PickupPoint pickupPoint)
    {
        currentPickupPoint = pickupPoint;
        LogHelper.Log($"Pickup point assigned to passenger");
    }
    
    // NEW METHOD - Called when passenger triggers with door
    public void OnReachedDoor()
    {
        LogHelper.Log($"Passenger reached door - notifying pickup point!");
        
        if (currentPickupPoint != null)
        {
            currentPickupPoint.OnPassengerReachedDoor();
        }
        else
        {
            LogHelper.LogError("No pickup point assigned to passenger!");
        }
    }
    
    
    private void MoveTowardsDoor()
    {
        if (targetDoor == null)
        {
            LogHelper.LogWarning("Target door is null in MoveTowardsDoor!");
            return;
        }
        
        // Calculate direction
        Vector3 direction = (targetDoor.position - transform.position).normalized;
        direction.y = 0; // Keep movement on ground level
        
        // Move towards the door
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Update animator with speed
        if (animator != null)
        {
            animator.SetFloat("Speed", moveSpeed / 3f);
        }
        
        // Rotate to face the door
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Check if reached the door
        float distance = Vector3.Distance(transform.position, targetDoor.position);
        if (distance < 1f)
        {
            LogHelper.Log($"Passenger reached car door! Distance: {distance}");
        }
    }
    
    public void EnterCar()
    {
        LogHelper.Log($"Passenger entering car");
        
        currentState = PassengerState.InCar;
        
        // Stop animation
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
        
        gameObject.SetActive(false);
    }
    
    public void ExitCar(Vector3 dropPosition)
    {
        LogHelper.Log($"Passenger exiting car at {dropPosition}");
        
        transform.position = dropPosition;
        gameObject.SetActive(true);
        currentState = PassengerState.Dropped;
        
        // Start walking away after a brief moment
        Invoke(nameof(StartWalkingAway), 0.5f);
    }
    
    private void StartWalkingAway()
    {
        LogHelper.Log($"Passenger starting to walk away");
        currentState = PassengerState.WalkingAway;
        
        if (animator != null)
        {
            animator.SetFloat("Speed", moveSpeed / 3f);
        }
    }
    
    private void WalkAway()
    {
        if (exitWalkTarget == null)
        {
            LogHelper.LogWarning("No exit walk target assigned!");
            return;
        }
        
        Vector3 direction = (exitWalkTarget.position - transform.position).normalized;
        direction.y = 0;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Update animator while walking
        if (animator != null)
        {
            animator.SetFloat("Speed", moveSpeed / 3f);
        }
        
        // Rotate to face walking direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Destroy or disable when reached destination
        float distance = Vector3.Distance(transform.position, exitWalkTarget.position);
        if (distance < 0.5f)
        {
            LogHelper.Log($"Passenger reached exit destination");
            
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            
            gameObject.SetActive(false);
        }
    }
    
    public void ResetPassenger()
    {
        LogHelper.Log($"Resetting passenger");
        
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        currentState = PassengerState.WaitingForPickup;
        gameObject.SetActive(true);
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}