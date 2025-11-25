using System.Collections;
//using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DropoffPoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] VibrationController vibrationController;
    public Passenger passenger;
    public Transform carDoorPoint;
   // public Transform exitWalkTarget;
    public Transform marker;
    public Transform carBody;

    [Header("Settings")]
    public float dropoffDelay = 1f;
    public float markerRotateSpeed = 90f;
    public float markerRotateDuration = 2f;
    public bool isFinalDropoffPoint = false;

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

        PassengerCarrier car = other.GetComponent<PassengerCarrier>();

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

            AudioManager.Instance?.PlayUI("DropOff");

            vibrationController.SuccessVibration();
            LogHelper.Log("drop off point vibration called");

            isCarInRange = true;
            currentCar = car;

            car.RequestStop();

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

    private void StartDropoffSequence()
    {
        LogHelper.Log($"StartDropoffSequence called. InRange: {isCarInRange}, Dropped: {passengerDropped}");

        if (!isCarInRange || passengerDropped || currentCar == null) return;

        onCarStopped?.Invoke();

        var splineCar = currentCar.GetComponent<SplineCarController>();
        if (splineCar != null)
            splineCar.SetTouchEnabled(false);

        AudioManager.Instance?.PlaySFX("OpenDoor");

        if (carDoorPoint != null && passenger != null)
        {
            LogHelper.Log($"Preparing passenger to exit car");

            passenger.ExitCar(carDoorPoint.position);
            passenger.SetDropoffPoint(this);

            // if (exitWalkTarget != null)
            //     passenger.exitWalkTarget = exitWalkTarget;
        }
        else
        {
            LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
        }
    }

    public void OnPassengerReachedExit()
    {
        LogHelper.Log("Passenger reached exit point — completing dropoff!");

        if (passenger == null || currentCar == null || passengerDropped) return;

        CompleteDropoff();

        if (isFinalDropoffPoint)
        {
            if (carBody != null)
            {
                StartCoroutine(DriveAwayForever(carBody, 10f));
            }
            else
            {
                LogHelper.LogWarning("CarBody not assigned in Inspector!");
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
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

        currentCar.SetPassenger(false);
        currentCar.ResumeFromPickup();

        passengerDropped = true;

        if (marker != null)
        {
            MarkerAnimationHelper.AnimateMarkerDisappearance(marker);
            onPassengerDroppedOff?.Invoke();
        }
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
        if (isMarkerRotating && marker != null)
        {
            markerRotationTimer += Time.deltaTime;

            if (markerRotationTimer < markerRotateDuration)
            {
                marker.Rotate(Vector3.forward * markerRotateSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                marker.rotation = Quaternion.Slerp(marker.rotation, markerOriginalRotation, Time.deltaTime * 5f);

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
        Gizmos.color = new Color(0f, 1.5f, 2f, 0.3f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}