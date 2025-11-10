using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages passenger pickup/dropoff tracking for level completion
/// Works with SEPARATE passenger objects at pickup and dropoff points
/// </summary>
public class PassengerLevelManager : MonoBehaviour
{
    [Header("Passenger Configuration")]
    [Tooltip("All pickup-dropoff pairs in this level")]
    public List<PassengerPair> passengers = new List<PassengerPair>();

    [Header("UI References (Optional)")]
    public TMPro.TextMeshProUGUI passengerCountText;

    [Header("Events")]
    public UnityEvent onLevelStart;
    public UnityEvent onLevelSuccess;
    public UnityEvent onLevelFail;
    public UnityEvent<int, int> onPassengerCountChanged; // delivered, total

    // Tracking
    private int totalPassengers;
    private int passengersDelivered;
    private bool levelActive = false;
    private bool levelCompleted = false;

    // Track which passenger is currently picked up
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

        // Register listeners for ALL pickup and dropoff points
        foreach (var pair in passengers)
        {
            if (pair.pickupPoint != null && pair.dropoffPoint != null)
            {
                pair.isPickedUp = false;
                pair.isDelivered = false;

                // Listen to pickup event
                pair.pickupPoint.onPassengerPickedUp.RemoveListener(() => OnPassengerPickedUp(pair));
                pair.pickupPoint.onPassengerPickedUp.AddListener(() => OnPassengerPickedUp(pair));

                // Listen to dropoff event
                pair.dropoffPoint.onPassengerDroppedOff.RemoveListener(() => OnPassengerDroppedOff(pair));
                pair.dropoffPoint.onPassengerDroppedOff.AddListener(() => OnPassengerDroppedOff(pair));
            }
            else
            {
                LogHelper.LogError($"[PassengerLevelManager] Invalid passenger pair: {pair.pairName}");
            }
        }

        UpdatePassengerUI();
        onLevelStart?.Invoke();
    }

    private void CleanupLevel()
    {
        // Remove all listeners
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

    // Called when player picks up passenger at pickup point
    private void OnPassengerPickedUp(PassengerPair pair)
    {
        if (levelCompleted || pair.isPickedUp) return;

        pair.isPickedUp = true;
        currentPickedUpPair = pair;

        LogHelper.Log($"[PassengerLevelManager] Passenger picked up from: {pair.pickupPoint.name}");
        AudioManager.Instance?.PlayUI("PassengerPickup");
    }

    // Called when player drops off passenger at dropoff point
    private void OnPassengerDroppedOff(PassengerPair pair)
    {
        if (levelCompleted) return;

        // Check if this passenger was already delivered
        if (pair.isDelivered)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Passenger already delivered at {pair.dropoffPoint.name}!");
            return;
        }

        // Check if passenger was picked up first
        if (!pair.isPickedUp)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Trying to dropoff without pickup at {pair.dropoffPoint.name}!");
            return;
        }

        // OPTIONAL: Verify this is the correct dropoff for current passenger
        if (currentPickedUpPair != pair)
        {
            LogHelper.LogWarning($"[PassengerLevelManager] Wrong dropoff point! Expected {currentPickedUpPair?.dropoffPoint.name}, got {pair.dropoffPoint.name}");
        }

        // Mark as delivered
        pair.isDelivered = true;
        passengersDelivered++;
        currentPickedUpPair = null;

        LogHelper.Log($"[PassengerLevelManager] Passenger delivered! Progress: {passengersDelivered}/{totalPassengers}");

        AudioManager.Instance?.PlayUI("PassengerDropoff");
        UpdatePassengerUI();
        onPassengerCountChanged?.Invoke(passengersDelivered, totalPassengers);

        // Check if all passengers delivered
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

        AudioManager.Instance?.PlayUI("LevelComplete");
        onLevelSuccess?.Invoke();

       // Invoke(nameof(LoadNextLevel), 2f);
    }

    // private void LoadNextLevel()
    // {
    //     if (LevelManager_Temporary.Instance != null)
    //     {
    //         LevelManager_Temporary.Instance.LoadNextLevel();
    //     }
    // }

    public void FailLevel(string reason = "Level Failed")
    {
        if (levelCompleted) return;

        levelCompleted = true;
        levelActive = false;

        LogHelper.Log($"[PassengerLevelManager] LEVEL FAILED: {reason}");

        AudioManager.Instance?.PlayUI("LevelFailed");
        onLevelFail?.Invoke();

        Invoke(nameof(ReloadLevel), 2f);
    }

    private void ReloadLevel()
    {
        if (LevelManager_Temporary.Instance != null)
        {
            LevelManager_Temporary.Instance.ReloadcurrentLevel();
        }
    }

    public void OnCarDestroyed()
    {
        FailLevel("Car destroyed!");
    }

    private void UpdatePassengerUI()
    {
        if (passengerCountText != null)
        {
            passengerCountText.text = $"Passengers: {passengersDelivered}/{totalPassengers}";
        }
    }

    // Public getters
    public int GetPassengersDelivered() => passengersDelivered;
    public int GetTotalPassengers() => totalPassengers;
    public bool IsLevelComplete() => levelCompleted;
    public PassengerPair GetCurrentPickedUpPair() => currentPickedUpPair;

    // Helper to check if player has a passenger
    public bool HasPassenger() => currentPickedUpPair != null;

    [ContextMenu("Auto-Setup Pairs by Index")]
    private void AutoSetupPairsByIndex()
    {
        passengers.Clear();

        PickupPoint[] pickups = GetComponentsInChildren<PickupPoint>();
        DropoffPoint[] dropoffs = GetComponentsInChildren<DropoffPoint>();

        LogHelper.Log($"Found {pickups.Length} pickups and {dropoffs.Length} dropoffs");

        // Match by index (Pickup 0 -> Dropoff 0, Pickup 1 -> Dropoff 1, etc.)
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
