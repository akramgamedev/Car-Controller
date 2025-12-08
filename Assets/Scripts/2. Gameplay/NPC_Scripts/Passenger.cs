using UnityEngine;

public class Passenger : MonoBehaviour
{
    [Header("Passenger Settings")]
    public float moveSpeed = 3f;
    public Transform exitWalkTarget;
    public float waveDetectionRadius = 15f;
    public float rotationSpeed = 3f;

    [Header("States")]
    public PassengerState currentState = PassengerState.WaitingForPickup;

    [Header("Animation (Optional)")]
    public Animator animator;

    [Header("Animation Clip Names")]
    public string waveAnimationName = "Idle_Waving"; // MUST match your Animator state name exactly!

    private Transform targetDoor;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private PickupPoint currentPickupPoint;
    private DropoffPoint currentDropoffPoint;
    public Transform playerCar;
    private bool isWaving = false;
    private PassengerState previousState;

    private Quaternion targetWaveRotation;
    private bool hasSetWaveRotation = false;

    public enum PassengerState
    {
        WaitingForPickup,
        Waving,
        MovingToCar,
        InCar,
        Dropped,
        WalkingAway
    }
    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        previousState = currentState;
        OnStateChanged();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        GameObject carObject = GameObject.FindGameObjectWithTag("Player");
        if (carObject != null)
        {
            playerCar = carObject.transform;
            LogHelper.Log($"Player car found: {playerCar.name}");
        }
        else
        {
            LogHelper.LogWarning("No Player car found!");
        }
        LogHelper.Log($"Passenger '{gameObject.name}' initialized. State: {currentState}");
    }
    private void Update()
    {
        // if (currentState != previousState)
        // {
        //     OnStateChanged();
        //     previousState = currentState;
        // }

        PassengerStateTransition();
    }

    private void OnStateChanged()
    {
        if (animator == null) return;

        hasSetWaveRotation = false;

        switch (currentState)
        {
            case PassengerState.WaitingForPickup:
                animator.SetBool("Wave", false);
                animator.SetFloat("Speed", 0f);
                LogHelper.Log("Animator: Wave=false, Speed=0 (Idle)");
                break;

            case PassengerState.Waving:
                // NUCLEAR OPTION: Force play the wave animation directly
                animator.SetBool("Wave", true);
                animator.SetFloat("Speed", 0f);

                // This bypasses ALL transitions and FORCES the animation to play
                animator.Play(waveAnimationName, 0, 0f);

                LogHelper.Log($"FORCED Wave animation to play: {waveAnimationName}");
                break;

            case PassengerState.MovingToCar:
                animator.SetBool("Wave", false);
                animator.SetFloat("Speed", moveSpeed / 3f);
                LogHelper.Log($"Animator: Wave=false, Speed={moveSpeed / 3f} (Walking)");
                break;

            case PassengerState.WalkingAway:
                animator.SetBool("Wave", false);
                animator.SetFloat("Speed", moveSpeed / 3f);
                LogHelper.Log($"Animator: Wave=false, Speed={moveSpeed / 3f} (Walking Away)");
                break;

            case PassengerState.InCar:
            case PassengerState.Dropped:
                animator.SetBool("Wave", false);
                animator.SetFloat("Speed", 0f);
                LogHelper.Log("Animator: Wave=false, Speed=0 (Stopped)");
                break;
        }
    }

    public void PassengerStateTransition()
    {
        switch (currentState)
        {
            case PassengerState.WaitingForPickup:
                CheckForCarAndWave();
                break;
            case PassengerState.Waving:
                KeepWaving();
                // KeepWaveAnimationPlaying(); // NEW: Keep forcing wave to play
                break;
            case PassengerState.MovingToCar:
                MoveTowardsDoor();
                break;
            case PassengerState.WalkingAway:
                WalkAway();
                break;
        }
    }

    // NEW: This runs every frame during Waving state to prevent Idle from taking over
    // private void KeepWaveAnimationPlaying()
    // {
    //     if (animator == null) return;

    //     // Check if animator has switched to a different animation
    //     AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

    //     // If not playing wave animation, force it back
    //     if (!stateInfo.IsName(waveAnimationName))
    //     {
    //         LogHelper.LogWarning($"Wave animation interrupted! Current: {stateInfo.fullPathHash}. Forcing back to Wave.");
    //         // animator.Play(waveAnimationName, 0, stateInfo.normalizedTime);
    //     }
    // }

    private void KeepWaving()
    {
        if (playerCar == null) return;

        if (!hasSetWaveRotation)
        {
            Vector3 directionToCar = (playerCar.position - transform.position).normalized;
            directionToCar.y = 0;

            if (directionToCar != Vector3.zero)
            {
                targetWaveRotation = Quaternion.LookRotation(directionToCar);
                hasSetWaveRotation = true;
            }
        }

        if (hasSetWaveRotation && Quaternion.Angle(transform.rotation, targetWaveRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWaveRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void CheckForCarAndWave()
    {
        if (playerCar == null || isWaving) return;

        float distanceToCar = Vector3.Distance(transform.position, playerCar.position);

        if (distanceToCar <= waveDetectionRadius)
        {
            StartWaving();
        }
    }

    private void StartWaving()
    {
        if (isWaving) return;

        isWaving = true;
        currentState = PassengerState.Waving;
        OnStateChanged();
        hasSetWaveRotation = false;

        if (playerCar != null)
        {
            Vector3 directionToCar = (playerCar.position - transform.position).normalized;
            directionToCar.y = 0;
            if (directionToCar != Vector3.zero)
            {
                targetWaveRotation = Quaternion.LookRotation(directionToCar);
            }
        }

        LogHelper.Log("Passenger started waving at the car");
    }

    private void StopWaving()
    {
        if (!isWaving) return;

        isWaving = false;
        hasSetWaveRotation = false;

        LogHelper.Log("Passenger stopped waving");
    }

    public void StartMovingToCar(Transform doorTransform)
    {
        LogHelper.Log($"StartMovingToCar called. Current State: {currentState}, Door: {doorTransform != null}");

        if (currentState != PassengerState.WaitingForPickup && currentState != PassengerState.Waving)
        {
            LogHelper.LogWarning($"Cannot start moving - wrong state: {currentState}");
            return;
        }

        if (doorTransform == null)
        {
            LogHelper.LogError("Door transform is NULL!");
            return;
        }

        StopWaving();

        targetDoor = doorTransform;
        currentState = PassengerState.MovingToCar;
        OnStateChanged();
        LogHelper.Log($"Passenger started moving to car door at {doorTransform.position}");
    }

    public void SetPickupPoint(PickupPoint pickupPoint)
    {
        currentPickupPoint = pickupPoint;
        LogHelper.Log($"Pickup point assigned to passenger");
    }

    public void SetDropoffPoint(DropoffPoint dropoff)
    {
        currentDropoffPoint = dropoff;
        LogHelper.Log($"Dropoff point assigned to passenger");
    }

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

        Vector3 direction = (targetDoor.position - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

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
        OnStateChanged(); gameObject.SetActive(false);
    }

    public void ExitCar(Vector3 dropPosition)
    {
        LogHelper.Log($"Passenger exiting car at {dropPosition}");

        transform.position = dropPosition;
        gameObject.SetActive(true);
        currentState = PassengerState.Dropped;
        OnStateChanged();
        Invoke(nameof(StartWalkingAway), 0.5f);
    }

    private void StartWalkingAway()
    {
        LogHelper.Log($"Passenger starting to walk away");
        currentState = PassengerState.WalkingAway;
        OnStateChanged();
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

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        float distance = Vector3.Distance(transform.position, exitWalkTarget.position);
        if (distance < 0.5f)
        {
            LogHelper.Log($"Passenger reached exit destination");

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            if (currentDropoffPoint != null)
            {
                currentDropoffPoint.OnPassengerReachedExit();
            }
        }
    }

    public void ResetPassenger()
    {
        LogHelper.Log($"Resetting passenger");

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        currentState = PassengerState.WaitingForPickup;
        previousState = PassengerState.WaitingForPickup;
        OnStateChanged();
        isWaving = false;
        hasSetWaveRotation = false;
        gameObject.SetActive(true);

        if (animator != null)
        {
            animator.SetBool("Wave", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, waveDetectionRadius);
    }
}

// using UnityEngine;

// public class Passenger : MonoBehaviour
// {
//     [Header("Passenger Settings")]
//     public float moveSpeed = 3f;
//     public Transform exitWalkTarget;
//     public float waveDetectionRadius = 15f;

//     [Header("States")]
//     public PassengerState currentState = PassengerState.WaitingForPickup;

//     [Header("Animation (Optional)")]
//     public Animator animator;

//     private Transform targetDoor;
//     private Vector3 originalPosition;
//     private Quaternion originalRotation;
//     private PickupPoint currentPickupPoint;
//     private DropoffPoint currentDropoffPoint;
//     public Transform playerCar;
//     private bool isWaving = false;

//     public enum PassengerState
//     {
//         WaitingForPickup,
//         Waving,
//         MovingToCar,
//         InCar,
//         Dropped,
//         WalkingAway
//     }

//     private void Start()
//     {
//         originalPosition = transform.position;
//         originalRotation = transform.rotation;

//         if (animator == null)
//         {
//             animator = GetComponent<Animator>();
//         }

//         GameObject carObject = GameObject.FindGameObjectWithTag("Player");
//         if (carObject != null)
//         {
//             playerCar = carObject.transform;
//         }
//         LogHelper.Log($"Passenger '{gameObject.name}' initialized. State: {currentState}");
//     }

//      void PassengerStateTransition()
//     {
//         switch (currentState)
//         {
//             case PassengerState.WaitingForPickup:
//                 CheckForCarAndWave();
//                 break;
//             case PassengerState.Waving:
//                 KeepWaving();
//                 break;
//             case PassengerState.MovingToCar:
//                 MoveTowardsDoor();
//                 break;
//             case PassengerState.WalkingAway:
//                 WalkAway();
//                 break;
//         }
//     }
//     private void KeepWaving()
//     {
//         // Smoothly rotate to keep facing the car while waving
//         if (playerCar != null)
//         {
//             Vector3 directionToCar = (playerCar.position - transform.position).normalized;
//             directionToCar.y = 0;

//             if (directionToCar != Vector3.zero)
//             {
//                 Quaternion targetRotation = Quaternion.LookRotation(directionToCar);
//                 transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
//             }
//         }
//     }

//     private void CheckForCarAndWave()
//     {
//         if (playerCar == null || isWaving) return;

//         float distanceToCar = Vector3.Distance(transform.position, playerCar.position);

//         if (distanceToCar <= waveDetectionRadius)
//         {
//             StartWaving();
//         }
//     }

//     private void StartWaving()
//     {
//         if (isWaving) return;

//         isWaving = true;
//         currentState = PassengerState.Waving;

//         if (playerCar != null)
//         {
//             Vector3 directionToCar = (playerCar.position - transform.position).normalized;
//             directionToCar.y = 0;
//             if (directionToCar != Vector3.zero)
//             {
//                 transform.rotation = Quaternion.LookRotation(directionToCar);
//             }
//         }

//         if (animator != null)
//         {
//             animator.SetBool("Wave", true);
//             animator.SetFloat("Speed", 0f);
//         }

//         LogHelper.Log("Passenger started waving at the car");
//     }

//     private void StopWaving()
//     {
//         if (!isWaving) return;

//         isWaving = false;

//         if (animator != null)
//         {
//             animator.SetBool("Wave", false);
//         }
//         LogHelper.Log("Passenger stopped waving");
//     }

//     public void StartMovingToCar(Transform doorTransform)
//     {
//         LogHelper.Log($"StartMovingToCar called. Current State: {currentState}, Door: {doorTransform != null}");

//         if (currentState != PassengerState.WaitingForPickup && currentState != PassengerState.Waving)
//         {
//             LogHelper.LogWarning($"Cannot start moving - wrong state: {currentState}");
//             return;
//         }

//         if (doorTransform == null)
//         {
//             LogHelper.LogError("Door transform is NULL!");
//             return;
//         }

//         StopWaving();

//         targetDoor = doorTransform;
//         currentState = PassengerState.MovingToCar;

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", moveSpeed / 3f);
//         }

//         LogHelper.Log($"Passenger started moving to car door at {doorTransform.position}");
//     }

//     public void SetPickupPoint(PickupPoint pickupPoint)
//     {
//         currentPickupPoint = pickupPoint;
//         LogHelper.Log($"Pickup point assigned to passenger");
//     }

//     public void SetDropoffPoint(DropoffPoint dropoff)
//     {
//         currentDropoffPoint = dropoff;
//         LogHelper.Log($"Dropoff point assigned to passenger");
//     }

//     public void OnReachedDoor()
//     {
//         LogHelper.Log($"Passenger reached door - notifying pickup point!");

//         if (currentPickupPoint != null)
//         {
//             currentPickupPoint.OnPassengerReachedDoor();
//         }
//         else
//         {
//             LogHelper.LogError("No pickup point assigned to passenger!");
//         }
//     }

//     private void MoveTowardsDoor()
//     {
//         if (targetDoor == null)
//         {
//             LogHelper.LogWarning("Target door is null in MoveTowardsDoor!");
//             return;
//         }

//         Vector3 direction = (targetDoor.position - transform.position).normalized;
//         direction.y = 0;

//         transform.position += direction * moveSpeed * Time.deltaTime;

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", moveSpeed / 3f);
//         }

//         if (direction != Vector3.zero)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(direction);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//         }

//         float distance = Vector3.Distance(transform.position, targetDoor.position);
//         if (distance < 1f)
//         {
//             LogHelper.Log($"Passenger reached car door! Distance: {distance}");
//         }
//     }

//     public void EnterCar()
//     {
//         LogHelper.Log($"Passenger entering car");

//         currentState = PassengerState.InCar;

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", 0f);
//             animator.SetBool("Wave", false);
//         }

//         gameObject.SetActive(false);
//     }

//     public void ExitCar(Vector3 dropPosition)
//     {
//         LogHelper.Log($"Passenger exiting car at {dropPosition}");

//         transform.position = dropPosition;
//         gameObject.SetActive(true);
//         currentState = PassengerState.Dropped;

//         Invoke(nameof(StartWalkingAway), 0.5f);
//     }

//     private void StartWalkingAway()
//     {
//         LogHelper.Log($"Passenger starting to walk away");
//         currentState = PassengerState.WalkingAway;

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", moveSpeed / 3f);
//         }
//     }

//     private void WalkAway()
//     {
//         if (exitWalkTarget == null)
//         {
//             LogHelper.LogWarning("No exit walk target assigned!");
//             return;
//         }

//         Vector3 direction = (exitWalkTarget.position - transform.position).normalized;
//         direction.y = 0;

//         transform.position += direction * moveSpeed * Time.deltaTime;

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", moveSpeed / 3f);
//         }

//         if (direction != Vector3.zero)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(direction);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//         }

//         float distance = Vector3.Distance(transform.position, exitWalkTarget.position);
//         if (distance < 0.5f)
//         {
//             LogHelper.Log($"Passenger reached exit destination");

//             if (animator != null)
//             {
//                 animator.SetFloat("Speed", 0f);
//             }

//             if (currentDropoffPoint != null)
//             {
//                 currentDropoffPoint.OnPassengerReachedExit();
//             }

//         }
//     }

//     public void ResetPassenger()
//     {
//         LogHelper.Log($"Resetting passenger");

//         transform.position = originalPosition;
//         transform.rotation = originalRotation;
//         currentState = PassengerState.WaitingForPickup;
//         gameObject.SetActive(true);

//         if (animator != null)
//         {
//             animator.SetFloat("Speed", 0f);
//             animator.SetBool("Wave", false);
//         }
//     }
// }