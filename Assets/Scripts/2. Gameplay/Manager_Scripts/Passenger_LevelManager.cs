using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;

public class PassengerLevelManager : MonoBehaviour
{
    [Header("Passenger Configuration")]
    [Tooltip("All pickup-dropoff pairs in this level")]
    public List<PassengerPair> passengers = new List<PassengerPair>();

    //[Header("UI References (Optional)")]
    //public TMPro.TextMeshProUGUI passengerCountText;

//Change To actions if not needed in inspector 
// Move to static events class
    [Header("Events")]
    public UnityEvent onLevelStart;
    public UnityEvent onLevelSuccess;
    public UnityEvent onLevelFail;
    public UnityEvent<int, int> onPassengerCountChanged;

    private int totalPassengers;
    private int passengersDelivered;
    private bool levelActive = false;
    private bool levelCompleted = false;

    private PassengerPair currentPickedUpPair = null;

    private void OnEnable()
    {
        InitializeLevel();
    }

    private void OnDisable()
    {
        CleanupLevel();
    }

    private void InitializeLevel()
    {
        totalPassengers = passengers.Count;
        passengersDelivered = 0;
        levelActive = true;
        levelCompleted = false;
        currentPickedUpPair = null;

        LogHelper.Log($"[PassengerLevelManager] Level initialized with {totalPassengers} passengers");

        foreach (var pair in passengers)
        {
            if (pair.pickupPoint != null && pair.dropoffPoint != null)
            {
                pair.isPickedUp = false;
                pair.isDelivered = false;

                pair.pickupPoint.onPassengerPickedUp.RemoveListener(() => OnPassengerPickedUp(pair));
                pair.pickupPoint.onPassengerPickedUp.AddListener(() => OnPassengerPickedUp(pair));

                pair.dropoffPoint.onPassengerDroppedOff.RemoveListener(() => OnPassengerDroppedOff(pair));
                pair.dropoffPoint.onPassengerDroppedOff.AddListener(() => OnPassengerDroppedOff(pair));
            }
            else
            {
                LogHelper.LogError($"[PassengerLevelManager] Invalid passenger pair: {pair.pairName}");
            }
        }

        //UpdatePassengerUI();
        onLevelStart?.Invoke();
    }

    private void CleanupLevel()
    {
        foreach (var pair in passengers)
        {
            if (pair.pickupPoint != null)
            {
                pair.pickupPoint.onPassengerPickedUp.RemoveAllListeners();
            }
            if (pair.dropoffPoint != null)
            {
                pair.dropoffPoint.onPassengerDroppedOff.RemoveAllListeners();
            }
        }
    }

    private void OnPassengerPickedUp(PassengerPair pair)
    {
        if (levelCompleted || pair.isPickedUp) return;

        pair.isPickedUp = true;
        currentPickedUpPair = pair;

        LogHelper.Log($"[PassengerLevelManager] Passenger picked up from: {pair.pickupPoint.name}");
        AudioManager.Instance?.PlayUI("PassengerPickup");
    }

    private void OnPassengerDroppedOff(PassengerPair pair)
    {
        if (levelCompleted) return;

        if (pair.isDelivered)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Passenger already delivered at {pair.dropoffPoint.name}!");
            return;
        }

        if (!pair.isPickedUp)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Trying to dropoff without pickup at {pair.dropoffPoint.name}!");
            return;
        }

        if (currentPickedUpPair != pair)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Wrong dropoff point! Expected {currentPickedUpPair?.dropoffPoint.name}, got {pair.dropoffPoint.name}");
        }

        pair.isDelivered = true;
        passengersDelivered++;
        currentPickedUpPair = null;

        LogHelper.Log($"[PassengerLevelManager] Passenger delivered! Progress: {passengersDelivered}/{totalPassengers}");

        AudioManager.Instance?.PlayUI("PassengerDropoff");
       // UpdatePassengerUI();
        onPassengerCountChanged?.Invoke(passengersDelivered, totalPassengers);

        if (passengersDelivered >= totalPassengers)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        levelActive = false;

        LogHelper.Log("[PassengerLevelManager] LEVEL SUCCESS! All passengers delivered!");
        StaticEvents.GameEvents.OnGameWin?.Invoke();
        // Check if this sound me be played using OnGameWin subscription in AudioManager
        AudioManager.Instance?.PlayUI("LevelComplete");
    }

    public void FailLevel(string reason = "Level Failed")
    {
        if (levelCompleted) return;

        levelCompleted = true;
        levelActive = false;

        LogHelper.Log($"[PassengerLevelManager] LEVEL FAILED: {reason}");
        StaticEvents.GameEvents.OnGameLoose?.Invoke();
        // Check if this sound me be played using OnGameLoose subscription in AudioManager
        AudioManager.Instance?.PlayUI("LevelFailed");

    }
    public void OnCarDestroyed()
    {
        FailLevel("Car destroyed!");
    }

    // private void UpdatePassengerUI()
    // {
    //     if (passengerCountText != null)
    //     {
    //         passengerCountText.text = $"Passengers: {passengersDelivered}/{totalPassengers}";
    //     }
    // }

    public int GetPassengersDelivered() => passengersDelivered;
    public int GetTotalPassengers() => totalPassengers;
    public bool IsLevelComplete() => levelCompleted;
    public PassengerPair GetCurrentPickedUpPair() => currentPickedUpPair;

    public bool HasPassenger() => currentPickedUpPair != null;

    [ContextMenu("Auto-Setup Pairs by Index")]
    private void AutoSetupPairsByIndex()
    {
        passengers.Clear();

        PickupPoint[] pickups = GetComponentsInChildren<PickupPoint>();
        DropoffPoint[] dropoffs = GetComponentsInChildren<DropoffPoint>();

        LogHelper.Log($"Found {pickups.Length} pickups and {dropoffs.Length} dropoffs");

        int pairCount = Mathf.Min(pickups.Length, dropoffs.Length);

        for (int i = 0; i < pairCount; i++)
        {
            passengers.Add(new PassengerPair
            {
                pickupPoint = pickups[i],
                dropoffPoint = dropoffs[i],
                pairName = $"Pair {i + 1}: {pickups[i].name} → {dropoffs[i].name}"
            });

            LogHelper.Log($"Created Pair {i + 1}: {pickups[i].name} → {dropoffs[i].name}");
        }

        LogHelper.Log($"Created {passengers.Count} passenger pairs");
    }
}

[System.Serializable]
public class PassengerPair
{
    [Tooltip("Give this pair a descriptive name")]
    public string pairName = "Passenger Pair";

    [Tooltip("Where the passenger is picked up")]
    public PickupPoint pickupPoint;

    [Tooltip("Where the passenger should be dropped off")]
    public DropoffPoint dropoffPoint;

    [HideInInspector] public bool isPickedUp = false;
    [HideInInspector] public bool isDelivered = false;
}
