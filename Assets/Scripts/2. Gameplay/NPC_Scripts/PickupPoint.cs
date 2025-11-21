using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
//using MoreMountains.NiceVibrations;

[RequireComponent(typeof(Collider))]
public class PickupPoint : MonoBehaviour
{
    [Header("References")]
    public Passenger passenger;
    public Transform carDoorPoint;
    public Transform marker;

    [Header("Settings")]
    public float pickupDelay = 2f;
    public float markerRotateSpeed = 90f;
    public float markerRotateDuration = 2f;

    [Header("Events")]
    public UnityEvent onCarStopped;
    public UnityEvent onPassengerPickedUp;

    private bool isCarInRange = false;
    private bool passengerPickedUp = false;
    private PassengerCarrier currentCar;
    private bool isMarkerRotating = false;
    private Quaternion markerOriginalRotation;
    private float markerRotationTimer = 0f;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (marker != null)
        {
            markerOriginalRotation = marker.rotation;
        }

        LogHelper.Log($"PickupPoint '{gameObject.name}' initialized. Trigger: {col.isTrigger}");
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

        if (car != null && !passengerPickedUp)
        {
            LogHelper.Log($"Car detected! Requesting stop...");

            AudioManager.Instance?.PlayUI("Pickup");

          //  MMVibrationManager.Haptic(HapticTypes.MediumImpact);

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

        if (carDoorPoint != null && passenger != null)
        {
            LogHelper.Log($"Starting passenger movement to door");

            passenger.SetPickupPoint(this);
            passenger.StartMovingToCar(carDoorPoint);
        }
        else
        {
            LogHelper.LogError($"Missing references - Door: {carDoorPoint != null}, Passenger: {passenger != null}");
        }
    }

    public void OnPassengerReachedDoor()
    {
        LogHelper.Log($"Passenger reached door - entering car!");

        if (passenger == null || currentCar == null || passengerPickedUp) return;

         AudioManager.Instance?.PlaySFX("CloseDoor");


        CompletePickup();
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
            LogHelper.Log($"Car exited pickup zone");
            isCarInRange = false;
            currentCar = null;
            isMarkerRotating = false;
        }
    }

    private void CompletePickup()
    {
        LogHelper.Log($"CompletePickup called - Passenger entering car!");

        if (passenger == null || currentCar == null) return;

        passenger.EnterCar();
        passengerPickedUp = true;

        currentCar.SetPassenger(true);
        currentCar.ResumeFromPickup();

        if (marker != null)
        {
            isMarkerRotating = false;
            MarkerAnimationHelper.AnimateMarkerDisappearance(marker);
        }

        onPassengerPickedUp?.Invoke();
    }

    public void ResetPickupPoint()
    {
        passengerPickedUp = false;
        isCarInRange = false;
        currentCar = null;
        isMarkerRotating = false;
        markerRotationTimer = 0f;

        // Reset marker rotation and show it
        if (marker != null)
        {
            marker.rotation = markerOriginalRotation;
            marker.gameObject.SetActive(true);
        }

        LogHelper.Log("Pickup point reset");
    }

    void Update()
    {
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
        Gizmos.color = new Color(2f, 1.5f, 0f, 0.3f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}