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
    [SerializeField] private string[] cashPage1CarIDs; // e.g., "car_0", "car_1", etc.
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
        // Setup all cars in data if not already done
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
            // Check if car already exists
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
        // Page 0 = Cash Page 1
        unlockHandlers[0] = new CashUnlockHandler(dataManager, 0, cashPage1Cost);

        // Page 1 = Cash Page 2
        unlockHandlers[1] = new CashUnlockHandler(dataManager, 1, cashPage2Cost);

        // Page 2 = Cash Page 3
        unlockHandlers[2] = new CashUnlockHandler(dataManager, 2, cashPage3Cost);

        // Page 3 = Chest Page
        unlockHandlers[3] = new ChestUnlockHandler(dataManager, 0, chestKeyCost);

        // Page 4 = Progression Page
        progressionHandler = new ProgressionUnlockHandler(dataManager, 0, levelsForProgressionUnlock, fillAmountPerLevel);
        unlockHandlers[4] = progressionHandler;

        // Page 5 = VIP Page
        unlockHandlers[5] = new VIPUnlockHandler(dataManager, 0);
    }

    void UnlockDefaultCar()
    {
        // Unlock first car from cash page 1 by default
        if (dataManager.gameData.carData.cars.Count > 0 && !dataManager.gameData.carData.cars[0].isUnlocked)
        {
            var car = dataManager.gameData.carData.cars[0];
            car.isUnlocked = true;
            dataManager.gameData.carData.cars[0] = car;
            dataManager.SaveGameData();

            LogHelper.Log("Default car unlocked!");
        }
    }

    // Called from UI Button for Cash Pages
    public void OnCashUnlockButtonPressed(int pageIndex)
    {
        if (unlockHandlers.ContainsKey(pageIndex))
        {
            if (unlockHandlers[pageIndex].TryUnlock(out int unlockedCarIndex))
            {
                // Success - show notification or update UI
                LogHelper.Log($"Successfully unlocked car {unlockedCarIndex}!");
            }
            else
            {
                LogHelper.LogWarning("Failed to unlock car from cash page.");
            }
        }
    }

    // Called from UI Button for Chest Page
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

    // Called when level is completed
    void OnLevelCompleted()
    {
        if (progressionHandler != null)
        {
            progressionHandler.OnLevelCompleted();
        }
    }

    // Called when VIP is purchased
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


//***************** working code *******************

// using UnityEngine;
// using System;

// public class CarUnlockManager : MonoBehaviour
// {
//     public static CarUnlockManager Instance { get; private set; }

//     [Header("DataManager Reference")]
//     [SerializeField] private DataManager dataManager;

//     [System.Serializable]
//     public class CarPrice
//     {
//         public int carIndex;
//         public int coinPrice;
//         public int keyPrice;
//     }

//     [Header("Car Prices")]
//     [SerializeField] private CarPrice[] carPrices;

//     public event Action<int> OnCarUnlocked;

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//     }
//     #region Subscription

//     void OnEnable()
//     {
//         StaticEvents.GameEconomy.OnCurrencyChange += OnCurrencyChanged;
//     }
//     void OnDisable()
//     {
//         StaticEvents.GameEconomy.OnCurrencyChange -= OnCurrencyChanged;
//     }

//     #endregion

//     void Start()
//     {
//         SyncCarsWithGameCars(carPrices.Length);

//         CheckAutoUnlocks();
//     }
//     void OnCurrencyChanged(int amount, GlobalEnums.CurrencyType type)
//     {
//         if (type == GlobalEnums.CurrencyType.Coin)
//         {
//             CheckAutoUnlocks();
//         }
//         if (type == GlobalEnums.CurrencyType.Key)
//         {
//             CheckAutoUnlocks();
//         }

//     }
//     void CheckAutoUnlocks()
//     {
//         foreach (var price in carPrices)
//         {
//             if (IsCarUnlocked(price.carIndex))
//                 continue;

//             if (CanAffordCar(price.carIndex))
//             {
//                 AutoUnlockCar(price.carIndex);
//             }
//         }
//     }

//     // Auto unlock car when player reaches required amount
//     void AutoUnlockCar(int carIndex)
//     {
//         if (IsCarUnlocked(carIndex))
//             return;
//         while (dataManager.gameData.carData.cars.Count <= carIndex)
//         {
//             dataManager.gameData.carData.AddCarToList();
//         }
//         var car = dataManager.gameData.carData.cars[carIndex];
//         car.isUnlocked = true;
//         dataManager.gameData.carData.cars[carIndex] = car;
//         dataManager.SaveGameData();

//         OnCarUnlocked?.Invoke(carIndex);

//         LogHelper.Log($"Car {carIndex} AUTO-UNLOCKED! You now have enough currency!");

//         // Optional: Show a popup notification here
//         // UIManager.Instance?.ShowCarUnlockedPopup(carIndex);
//     }
//     public bool IsCarUnlocked(int carIndex)
//     {
//         if (dataManager != null && carIndex < dataManager.gameData.carData.cars.Count)
//         {
//             return dataManager.gameData.carData.cars[carIndex].isUnlocked;
//         }
//         return false;
//     }


//     // Get car price
//     public CarPrice GetCarPrice(int carIndex)
//     {
//         foreach (var price in carPrices)
//         {
//             if (price.carIndex == carIndex)
//                 return price;
//         }
//         return null;
//     }

//     // Check if player can afford car
//     public bool CanAffordCar(int carIndex)
//     {
//         CarPrice price = GetCarPrice(carIndex);
//         if (price == null)
//             return false;

//         bool hasEnoughCoins = HasEnoughCoins(price.coinPrice);
//         bool hasEnoughKeys = HasEnoughKeys(price.keyPrice);

//         return hasEnoughCoins && hasEnoughKeys;
//     }
//     public bool HasEnoughCoins(int amount)
//     {
//         return StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Coin) >= amount;
//     }
//     public bool HasEnoughKeys(int amount)
//     {
//         return StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Key) >= amount;
//     }

//     public void UnlockCarFree(int carIndex)
//     {
//         if (!IsCarUnlocked(carIndex))
//         {
//             while (dataManager.gameData.carData.cars.Count <= carIndex)
//             {
//                 dataManager.gameData.carData.AddCarToList();
//             }

//             var car = dataManager.gameData.carData.cars[carIndex];
//             car.isUnlocked = true;
//             dataManager.gameData.carData.cars[carIndex] = car;

//             dataManager.SaveGameData();
//             OnCarUnlocked?.Invoke(carIndex);
//             LogHelper.Log($"Car {carIndex} unlocked for free!");
//         }
//     }

//     public void SyncCarsWithGameCars(int totalCarsInGame)
//     {
//         while (dataManager.gameData.carData.cars.Count < totalCarsInGame)
//         {
//             dataManager.gameData.carData.AddCarToList();
//         }
//         dataManager.SaveGameData();
//     }
// #region Unused
//     // [ContextMenu("Setup Default Prices")]
//     // void SetupDefaultPrices()
//     // {
//     //     carPrices = new CarPrice[]
//     //     {
//     //         new CarPrice { carIndex = 0, coinPrice = 0, keyPrice = 0 },      // Free
//     //         new CarPrice { carIndex = 1, coinPrice = 500, keyPrice = 0 },
//     //         new CarPrice { carIndex = 2, coinPrice = 1000, keyPrice = 0 },
//     //         new CarPrice { carIndex = 3, coinPrice = 1500, keyPrice = 1 },
//     //         new CarPrice { carIndex = 4, coinPrice = 2000, keyPrice = 2 },
//     //     };
//     //     LogHelper.Log("Default car prices set up");
//     // }
// }





// using UnityEngine;
// using System;

// public class CarUnlockManager : MonoBehaviour
// {
//     public static CarUnlockManager Instance { get; private set; }

//     [System.Serializable]
//     public class CarPrice
//     {
//         public int carIndex;
//         public int coinPrice;
//         public int keyPrice; // Optional: if you want some cars to cost keys
//     }

//     [Header("Car Prices")]
//     [SerializeField] private CarPrice[] carPrices;

//     // Event when a car is unlocked
//     public event Action<int> OnCarUnlocked;

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//     }

//     // Check if car is unlocked
//     public bool IsCarUnlocked(int carIndex)
//     {
//         if (DataManager.Instance != null)
//         {
//             return DataManager.Instance.IsCarUnlocked(carIndex);
//         }
//         return false;
//     }

//     // Get car price
//     public CarPrice GetCarPrice(int carIndex)
//     {
//         foreach (var price in carPrices)
//         {
//             if (price.carIndex == carIndex)
//                 return price;
//         }
//         return null;
//     }

//     // Check if player can afford car
//     public bool CanAffordCar(int carIndex)
//     {
//         CarPrice price = GetCarPrice(carIndex);
//         if (price == null)
//         {
//             LogHelper.LogWarning($"No price found for car {carIndex}");
//             return false;
//         }

//         bool hasEnoughCoins = CurrencyManager.Instance.HasEnoughCoins(price.coinPrice);
//         bool hasEnoughKeys = CurrencyManager.Instance.HasEnoughKeys(price.keyPrice);

//         return hasEnoughCoins && hasEnoughKeys;
//     }

//     // Purchase car
//     public bool PurchaseCar(int carIndex)
//     {
//         // Check if already unlocked
//         if (IsCarUnlocked(carIndex))
//         {
//             LogHelper.LogWarning($"Car {carIndex} is already unlocked!");
//             return false;
//         }

//         // Get price
//         CarPrice price = GetCarPrice(carIndex);
//         if (price == null)
//         {
//             LogHelper.LogError($"No price configured for car {carIndex}");
//             return false;
//         }

//         // Check if can afford
//         if (!CanAffordCar(carIndex))
//         {
//             LogHelper.LogWarning($"Cannot afford car {carIndex}");
//             return false;
//         }

//         // Spend currency
//         bool coinsSpent = CurrencyManager.Instance.SpendCoins(price.coinPrice);
//         bool keysSpent = true;

//         if (price.keyPrice > 0)
//             keysSpent = CurrencyManager.Instance.SpendKeys(price.keyPrice);

//         if (coinsSpent && keysSpent)
//         {
//             // Unlock the car
//             DataManager.Instance.UnlockCar(carIndex);
//             OnCarUnlocked?.Invoke(carIndex);

//             LogHelper.Log($"Successfully purchased car {carIndex}!");
//             return true;
//         }

//         return false;
//     }

//     // Unlock car without payment (for rewards, ads, etc.)
//     public void UnlockCarFree(int carIndex)
//     {
//         if (!IsCarUnlocked(carIndex))
//         {
//             DataManager.Instance.UnlockCar(carIndex);
//             OnCarUnlocked?.Invoke(carIndex);
//             LogHelper.Log($"Car {carIndex} unlocked for free!");
//         }
//     }

//     [ContextMenu("Setup Default Prices")]
//     void SetupDefaultPrices()
//     {
//         carPrices = new CarPrice[]
//         {
//             new CarPrice { carIndex = 0, coinPrice = 0, keyPrice = 0 },      // Free
//             new CarPrice { carIndex = 1, coinPrice = 500, keyPrice = 0 },
//             new CarPrice { carIndex = 2, coinPrice = 1000, keyPrice = 0 },
//             new CarPrice { carIndex = 3, coinPrice = 1500, keyPrice = 1 },
//             new CarPrice { carIndex = 4, coinPrice = 2000, keyPrice = 2 },
//         };
//         LogHelper.Log("Default car prices set up");
//     }
// }

