using UnityEngine;
using System.Collections.Generic;

public class CarUnlockManager : MonoBehaviour
{
    public static CarUnlockManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DataManager dataManager;

    [Header("Cash Unlock Pages")]
    [SerializeField] private int cashPage1Cost = 1000;
    [SerializeField] private int cashPage2Cost = 1500;
    [SerializeField] private int cashPage3Cost = 2000;

    [Header("Chest Unlock")]
    [SerializeField] private int chestKeyCost = 3;

    [Header("Progression Unlock")]
    [SerializeField] private int levelsForProgressionUnlock = 2;
    [SerializeField] private float fillAmountPerLevel = 0.5f;

    [Header("Car Identifiers Setup")]
    [SerializeField] private string[] cashPage1CarIDs;
    [SerializeField] private string[] cashPage2CarIDs;
    [SerializeField] private string[] cashPage3CarIDs;
    [SerializeField] private string[] chestCarIDs;
    [SerializeField] private string[] progressionCarIDs;
    [SerializeField] private string[] vipCarIDs;

    private Dictionary<int, BaseCarUnlockHandler> unlockHandlers = new Dictionary<int, BaseCarUnlockHandler>();
    private ProgressionUnlockHandler progressionHandler;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

       
    }

    void OnEnable()
    {
        StaticEvents.GameEvents.OnLevelComplete += OnLevelCompleted;
    }

    void OnDisable()
    {
        StaticEvents.GameEvents.OnLevelComplete -= OnLevelCompleted;
    }

    void Start()
    {
        InitializeCarData();
        InitializeHandlers();
        UnlockDefaultCar();
    }

    void InitializeCarData()
    {
        AddCarsToData(cashPage1CarIDs, GlobalEnums.CarUnlockType.CashUnlock, 0);
        AddCarsToData(cashPage2CarIDs, GlobalEnums.CarUnlockType.CashUnlock, 1);
        AddCarsToData(cashPage3CarIDs, GlobalEnums.CarUnlockType.CashUnlock, 2);
        AddCarsToData(chestCarIDs, GlobalEnums.CarUnlockType.ChestUnlock, 0);
        AddCarsToData(progressionCarIDs, GlobalEnums.CarUnlockType.ProgressionUnlock, 0);
        AddCarsToData(vipCarIDs, GlobalEnums.CarUnlockType.VIPUnlock, 0);

        dataManager.SaveGameData();
    }

    void AddCarsToData(string[] carIDs, GlobalEnums.CarUnlockType unlockType, int pageIndex)
    {
        if (carIDs == null) return;

        foreach (string carID in carIDs)
        {
            bool exists = false;
            foreach (var car in dataManager.gameData.carData.cars)
            {
                if (car.identifier == carID)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                dataManager.gameData.carData.AddCarToList(carID, unlockType, pageIndex);
            }
        }
    }

    void InitializeHandlers()
    {
        unlockHandlers[0] = new CashUnlockHandler(dataManager, 0, cashPage1Cost);

        unlockHandlers[1] = new CashUnlockHandler(dataManager, 1, cashPage2Cost);

        unlockHandlers[2] = new CashUnlockHandler(dataManager, 2, cashPage3Cost);

        unlockHandlers[3] = new ChestUnlockHandler(dataManager, 0, chestKeyCost);

        progressionHandler = new ProgressionUnlockHandler(dataManager, 0, levelsForProgressionUnlock, fillAmountPerLevel);
        unlockHandlers[4] = progressionHandler;

        unlockHandlers[5] = new VIPUnlockHandler(dataManager, 0);
    }

    void UnlockDefaultCar()
    {
        if (dataManager.gameData.carData.cars.Count > 0 && !dataManager.gameData.carData.cars[0].isUnlocked)
        {
            var car = dataManager.gameData.carData.cars[0];
            car.isUnlocked = true;
            dataManager.gameData.carData.cars[0] = car;
            dataManager.SaveGameData();

            LogHelper.Log("Default car unlocked!");
        }
    }

    public void OnCashUnlockButtonPressed(int pageIndex)
    {
        if (unlockHandlers.ContainsKey(pageIndex))
        {
            if (unlockHandlers[pageIndex].TryUnlock(out int unlockedCarIndex))
            {
                LogHelper.Log($"Successfully unlocked car {unlockedCarIndex}!");
            }
            else
            {
                LogHelper.LogWarning("Failed to unlock car from cash page.");
            }
        }
    }

    public void OnChestUnlockButtonPressed()
    {
        if (unlockHandlers.ContainsKey(3))
        {
            if (unlockHandlers[3].TryUnlock(out int unlockedCarIndex))
            {
                LogHelper.Log($"Successfully unlocked chest car {unlockedCarIndex}!");
            }
            else
            {
                LogHelper.LogWarning("Failed to unlock chest car.");
            }
        }
    }

    void OnLevelCompleted()
    {
        if (progressionHandler != null)
        {
            progressionHandler.OnLevelCompleted();
        }
    }

    public void OnVIPPurchased()
    {
        if (unlockHandlers.ContainsKey(5) && unlockHandlers[5] is VIPUnlockHandler vipHandler)
        {
            vipHandler.UnlockAllVIPCars();
        }
    }

    public bool IsCarUnlocked(int carIndex)
    {
        if (carIndex < 0 || carIndex >= dataManager.gameData.carData.cars.Count)
            return false;

        return dataManager.gameData.carData.cars[carIndex].isUnlocked;
    }

    public float GetProgressionFillAmount()
    {
        return dataManager.gameData.carData.progressionFillAmount;
    }
}