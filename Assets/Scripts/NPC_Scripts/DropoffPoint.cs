using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DropoffPoint : MonoBehaviour
{
    [Header("References")]
    public Passenger passenger;
    public Transform carDoorPoint;
    public Transform exitWalkTarget;
    public Transform marker;
    public Transform carBody;

    [Header("Settings")]
    public float dropoffDelay = 1f;
    public float markerRotateSpeed = 90f;
    public float markerRotateDuration = 2f;

    [Header("Events")]
    public UnityEvent onCarStopped;
    public UnityEvent onPassengerDroppedOff;

    private bool isCarInRange = false;
    private bool passengerDropped = false;
    private PassengerCarrier currentCar;
    private bool isMarkerRotating = false;
    private Quaternion markerOriginalRotation;
    private float markerRotationTimer = 0f;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Hide the passenger initially (they're in the car)
        if (passenger != null)
        {
            passenger.gameObject.SetActive(false);
        }

        if (marker != null)
        {
            markerOriginalRotation = marker.rotation;
        }

        LogHelper.Log($"DropoffPoint '{gameObject.name}' initialized. Trigger: {col.isTrigger}");
    }

    private void OnTriggerEnter(Collider other)
    {
        LogHelper.Log($"OnTriggerEnter detected: {other.gameObject.name} (Tag: {other.tag})");

        // Check parent for PassengerCarrier since collider might be on child
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

        if (car != null && !passengerDropped && car.HasPassenger())
        {
            LogHelper.Log($"Car with passenger detected! Requesting stop...");

            isCarInRange = true;
            currentCar = car;

            // Request the car to stop
            car.RequestStop();

            // Find the door point on the car if not manually assigned
            if (carDoorPoint == null)
            {
                CarDoorPoint doorPoint = car.GetComponentInChildren<CarDoorPoint>();
                if (doorPoint != null)
                {
                    carDoorPoint = doorPoint.transform;
                    LogHelper.Log($"Found car door point: {carDoorPoint.name}");
                }
            }

            if (marker != null)
            {
                isMarkerRotating = true;
                markerRotationTimer = 0f;
            }

            // Start the dropoff sequence after a short delay
            Invoke(nameof(StartDropoffSequence), 0.5f);
        }
        else if (car == null)
        {
            LogHelper.Log($"No PassengerCarrier component found on {other.gameObject.name} or its parent");
        }
        else if (!car.HasPassenger())
        {
            LogHelper.Log("Car has no passenger to drop off");
        }
        else if (passengerDropped)
        {
            LogHelper.Log("Passenger already dropped off");
        }
    }

    // private void StartDropoffSequence()
    // {
    //     LogHelper.Log($"StartDropoffSequence called. InRange: {isCarInRange}, Dropped: {passengerDropped}");

    //     if (!isCarInRange || passengerDropped || currentCar == null) return;

    //     onCarStopped?.Invoke();

    //     var splineCar = currentCar.GetComponent<SplineCarController>();
    //     if (splineCar != null) splineCar.SetTouchEnabled(false);

    //     // Prepare passenger for exit
    //     if (carDoorPoint != null && passenger != null)
    //     {
    //         LogHelper.Log($"Preparing passenger to exit car");

    //         // Tell the passenger to exit at the door position
    //         passenger.ExitCar(carDoorPoint.position);

    //         // Set the dropoff point reference and exit target
    //         passenger.SetDropoffPoint(this);

    //         if (exitWalkTarget != null)
    //             passenger.exitWalkTarget = exitWalkTarget;

    //             if(carBody != null)
    //         {
    //             StartCoroutine(DriveAway(carBody, 8f, 3f));
    //         }


    //     }
    //     else
    //     {
    //         LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
    //     }
    // }

    private void StartDropoffSequence()
    {
        LogHelper.Log($"StartDropoffSequence called. InRange: {isCarInRange}, Dropped: {passengerDropped}");

        if (!isCarInRange || passengerDropped || currentCar == null) return;

        onCarStopped?.Invoke();

        // 🟥 Disable player touch permanently
        var splineCar = currentCar.GetComponent<SplineCarController>();
        if (splineCar != null)
            splineCar.SetTouchEnabled(false);

        // Prepare passenger for exit
        if (carDoorPoint != null && passenger != null)
        {
            LogHelper.Log($"Preparing passenger to exit car");

            passenger.ExitCar(carDoorPoint.position);
            passenger.SetDropoffPoint(this);

            if (exitWalkTarget != null)
                passenger.exitWalkTarget = exitWalkTarget;
        }
        else
        {
            LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
        }
    }


    // Called by the passenger when they reach the exit point
    // public void OnPassengerReachedExit()
    // {
    //     LogHelper.Log($"Passenger reached exit point - completing dropoff!");

    //     if (passenger == null || currentCar == null || passengerDropped) return;

    //     CompleteDropoff();
    // }
    public void OnPassengerReachedExit()
    {
        LogHelper.Log("Passenger reached exit point — completing dropoff!");

        if (passenger == null || currentCar == null || passengerDropped) return;

        CompleteDropoff();

        if (carBody != null)
        {
            StartCoroutine(DriveAwayForever(carBody, 10f));
        }
        else
        {
            LogHelper.LogWarning("CarBody not assigned in Inspector!");
        }
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
            LogHelper.Log($"Car exited dropoff zone");
            isCarInRange = false;
            currentCar = null;
            isMarkerRotating = false;
        }
    }

    private void CompleteDropoff()
    {
        LogHelper.Log($"CompleteDropoff called - Passenger fully exited!");

        if (passenger == null || currentCar == null) return;

        // Remove passenger from car
        currentCar.SetPassenger(false);
        currentCar.ResumeFromPickup();

        passengerDropped = true;

        // var splineCar = currentCar.GetComponent<SplineCarController>();
        // if (splineCar != null) splineCar.SetTouchEnabled(true);

        if (marker != null)
        {
            marker.gameObject.SetActive(false);
            isMarkerRotating = false;
        }

        onPassengerDroppedOff?.Invoke();
    }

    public void ResetDropoffPoint()
    {
        passengerDropped = false;
        isCarInRange = false;
        currentCar = null;
        isMarkerRotating = false;

        if (passenger != null)
        {
            passenger.gameObject.SetActive(false);
        }
        if (marker != null)
        {
            marker.rotation = markerOriginalRotation;
            marker.gameObject.SetActive(true);
        }

        LogHelper.Log("Dropoff point reset");
    }

    // private IEnumerator DriveAway(Transform carBody, float moveDistance=8f, float duration = 3f)
    // {
    //     Vector3 startPos = carBody.position;
    //     Vector3 endPos = startPos + carBody.forward * moveDistance;

    //     float elapsed = 0f;

    //     while(elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / duration;
    //         carBody.position = Vector3.Lerp(startPos, endPos, t);
    //         yield return null;
    //     }
    // }
    private IEnumerator DriveAwayForever(Transform carBody, float speed = 50f)
    {
        while (true)
        {
            carBody.position += carBody.forward * speed * Time.deltaTime;
            yield return null;
        }
    }



    void Update()
    {
        // if (isMarkerRotating && marker != null)
        // {
        //     marker.Rotate(Vector3.forward * markerRotateSpeed * Time.deltaTime, Space.Self);
        // }

        if (isMarkerRotating && marker != null)
        {
            markerRotationTimer += Time.deltaTime;

            // Rotate for the specified duration
            if (markerRotationTimer < markerRotateDuration)
            {
                marker.Rotate(Vector3.forward * markerRotateSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                // After duration, smoothly return to original rotation
                marker.rotation = Quaternion.Slerp(marker.rotation, markerOriginalRotation, Time.deltaTime * 5f);

                // Stop rotating once close enough to original rotation
                if (Quaternion.Angle(marker.rotation, markerOriginalRotation) < 1f)
                {
                    marker.rotation = markerOriginalRotation;
                    isMarkerRotating = false;
                    markerRotationTimer = 0f;
                }
            }
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1.5f, 2f, 0.3f); // Blue-ish color to differentiate from pickup
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}



// using UnityEngine;
// using UnityEngine.Events;

// [RequireComponent(typeof(Collider))]
// public class DropoffPoint : MonoBehaviour
// {
//     [Header("References")]
//     public Passenger passenger;
//     public Transform carDoorPoint;
//     public Transform exitWalkTarget;
//     public Transform marker;

//     [Header("Settings")]
//     public float dropoffDelay = 1f;
//     public float markerRotateSpeed = 90f;

//     [Header("Events")]
//     public UnityEvent onCarStopped;
//     public UnityEvent onPassengerDroppedOff;

//     private bool isCarInRange = false;
//     private bool passengerDropped = false;
//     private PassengerCarrier currentCar;
//     private bool isMarkerRotating = false;

//     private void Start()
//     {
//         Collider col = GetComponent<Collider>();
//         col.isTrigger = true;
//         if (passenger != null)
//         {
//             passenger.gameObject.SetActive(false);
//         }

//         LogHelper.Log($"DropoffPoint '{gameObject.name}' initialized. Trigger: {col.isTrigger}");

//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         LogHelper.Log($"OnTriggerEnter detected: {other.gameObject.name} (Tag: {other.tag})");

//         // Check parent for PassengerCarrier since collider might be on child
//         PassengerCarrier car = other.GetComponent<PassengerCarrier>();

//         // If not found on this GameObject, check parent
//         if (car == null)
//         {
//             car = other.GetComponentInParent<PassengerCarrier>();
//             if (car != null)
//             {
//                 LogHelper.Log($"Found PassengerCarrier on parent: {car.gameObject.name}");
//             }
//         }

//         if (car != null && !passengerDropped && car.HasPassenger())
//         {
//             LogHelper.Log($"Car with passenger detected! Requesting stop...");

//             isCarInRange = true;
//             currentCar = car;

//             // Request the car to stop
//             car.RequestStop();

//             // Find the door point on the car if not manually assigned
//             if (carDoorPoint == null)
//             {
//                 CarDoorPoint doorPoint = car.GetComponentInChildren<CarDoorPoint>();
//                 if (doorPoint != null)
//                 {
//                     carDoorPoint = doorPoint.transform;
//                     LogHelper.Log($"Found car door point: {carDoorPoint.name}");
//                 }
//             }

//             if (marker != null)
//             {
//                 isMarkerRotating = true;
//             }

//             // Start the dropoff sequence after a short delay
//             Invoke(nameof(StartDropoffSequence), 0.5f);
//         }
//         else if (car == null)
//         {
//             LogHelper.Log($"No PassengerCarrier component found on {other.gameObject.name} or its parent");
//         }
//         else if (!car.HasPassenger())
//         {
//             LogHelper.Log("Car has no passenger to drop off");
//         }
//         else if (passengerDropped)
//         {
//             LogHelper.Log("Passenger already dropped off");
//         }
//     }

//     private void StartDropoffSequence()
//     {
//         LogHelper.Log($"StartDropoffSequence called. InRange: {isCarInRange}, Dropped: {passengerDropped}");

//         if (!isCarInRange || passengerDropped || currentCar == null) return;

//         onCarStopped?.Invoke();

//         // Activate the passenger and position them at the car door
//         if (carDoorPoint != null && passenger != null)
//         {
//             LogHelper.Log($"Activating passenger at car door");

//             // Activate and position passenger at door
//             passenger.gameObject.SetActive(true);
//             passenger.transform.position = carDoorPoint.position;

//             // Tell the passenger to walk to exit point
//             if (exitWalkTarget != null)
//             {
//                 passenger.SetDropoffPoint(this);
//                 passenger.StartMovingToExit(exitWalkTarget);
//             }
//             else
//             {
//                 LogHelper.LogError("Exit walk target not assigned!");
//             }
//         }
//         else
//         {
//             LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
//         }
//     }

//     // Called by the passenger when they reach the exit point
//     public void OnPassengerReachedExit()
//     {
//         LogHelper.Log($"Passenger reached exit point - completing dropoff!");

//         if (passenger == null || currentCar == null || passengerDropped) return;

//         CompleteDropoff();
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         // Check both the collider and its parent
//         PassengerCarrier car = other.GetComponent<PassengerCarrier>();
//         if (car == null)
//         {
//             car = other.GetComponentInParent<PassengerCarrier>();
//         }

//         if (car != null)
//         {
//             LogHelper.Log($"Car exited dropoff zone");
//             isCarInRange = false;
//             currentCar = null;
//             isMarkerRotating = false;
//         }
//     }

//     private void CompleteDropoff()
//     {
//         LogHelper.Log($"CompleteDropoff called - Passenger exited car!");

//         if (passenger == null || currentCar == null) return;

//         // Remove passenger from car
//         currentCar.SetPassenger(false);
//         currentCar.ResumeFromPickup();

//         passengerDropped = true;

//         if (marker != null)
//         {
//             marker.gameObject.SetActive(false);
//             isMarkerRotating = false;
//         }

//         onPassengerDroppedOff?.Invoke();

//         // Optionally disable/hide passenger after some time
//         Invoke(nameof(HidePassenger), 2f);
//     }

//     private void HidePassenger()
//     {
//         if (passenger != null)
//         {
//             passenger.gameObject.SetActive(false);
//         }
//     }

//     public void ResetDropoffPoint()
//     {
//         passengerDropped = false;
//         isCarInRange = false;
//         currentCar = null;
//         isMarkerRotating = false;

//         if (passenger != null)
//         {
//             passenger.gameObject.SetActive(false);
//         }

//         LogHelper.Log("Dropoff point reset");
//     }

//     void Update()
//     {
//         if (isMarkerRotating && marker != null)
//         {
//             marker.Rotate(Vector3.forward * markerRotateSpeed * Time.deltaTime, Space.Self);
//         }
//     }

//     void OnDrawGizmos()
//     {
//         Gizmos.color = new Color(0f, 1.5f, 2f, 0.3f); // Blue-ish color to differentiate from pickup
//         BoxCollider box = GetComponent<BoxCollider>();
//         if (box != null)
//         {
//             Gizmos.matrix = transform.localToWorldMatrix;
//             Gizmos.DrawCube(box.center, box.size);
//         }
//     }

// }

// using UnityEngine;
// using UnityEngine.Events;

// [RequireComponent(typeof(Collider))]
// public class DropoffPoint : MonoBehaviour
// {
//     [Header("References")]
//     public Passenger passenger;
//     public Transform dropPosition; 
//     public Transform exitWalkTarget; 

//     [Header("Settings")]
//     public float dropoffDelay = 1f;

//     [Header("Events")]
//     public UnityEvent onCarStopped;
//     public UnityEvent onPassengerDroppedOff;

//     private bool isCarInRange = false;
//     private bool passengerDropped = false;

//     private void Start()
//     {
//         GetComponent<Collider>().isTrigger = true;

//         // Set drop position to this object's position if not assigned
//         if (dropPosition == null)
//         {
//             dropPosition = transform;
//         }

//         // Hide the passenger initially
//         if (passenger != null)
//         {
//             passenger.gameObject.SetActive(false);
//             if (exitWalkTarget != null)
//             {
//                 passenger.exitWalkTarget = exitWalkTarget;
//             }
//         }
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Car") || other.GetComponent<PassengerCarrier>() != null)
//         {
//             isCarInRange = true;
//         }
//     }

//     private void OnTriggerStay(Collider other)
//     {
//         if (!isCarInRange || passengerDropped) return;

//         // Check if car has stopped
//         PassengerCarrier car = other.GetComponent<PassengerCarrier>();
//         if (car != null && car.IsStopped() && car.HasPassenger())
//         {
//             OnCarStopped();
//         }
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Car") || other.GetComponent<PassengerCarrier>() != null)
//         {
//             isCarInRange = false;
//         }
//     }

//     private void OnCarStopped()
//     {
//         if (passengerDropped) return;

//         onCarStopped?.Invoke();
//         Invoke(nameof(CompleteDropoff), dropoffDelay);
//     }

//     private void CompleteDropoff()
//     {
//         if (passenger != null)
//         {
//             passenger.ExitCar(dropPosition.position);
//             passengerDropped = true;
//             onPassengerDroppedOff?.Invoke();
//         }
//     }

//     public void ResetDropoffPoint()
//     {
//         passengerDropped = false;
//         isCarInRange = false;

//         if (passenger != null)
//         {
//             passenger.gameObject.SetActive(false);
//         }
//     }

//      void OnDrawGizmos()
//     {
//         Gizmos.color = new Color(2f, 1.5f, 0f, 0.3f);
//         BoxCollider box = GetComponent<BoxCollider>();
//         if (box != null)
//         {
//             Gizmos.matrix = transform.localToWorldMatrix;
//             Gizmos.DrawCube(box.center, box.size);
//         }
//     }
// }