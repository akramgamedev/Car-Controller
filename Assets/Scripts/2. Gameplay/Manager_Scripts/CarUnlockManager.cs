using UnityEngine;
using System;

public class CarUnlockManager : MonoBehaviour
{
    public static CarUnlockManager Instance { get; private set; }

    [System.Serializable]
    public class CarPrice
    {
        public int carIndex;
        public int coinPrice;
        public int keyPrice; // Optional: if you want some cars to cost keys
    }

    [Header("Car Prices")]
    [SerializeField] private CarPrice[] carPrices;

    // Event when a car is unlocked
    public event Action<int> OnCarUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Subscribe to currency changes to auto-check unlocks
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += CheckAutoUnlocks;
            CurrencyManager.Instance.OnKeysChanged += CheckAutoUnlocks;
        }
        
        // Check on start
        CheckAutoUnlocks(0);
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged -= CheckAutoUnlocks;
            CurrencyManager.Instance.OnKeysChanged -= CheckAutoUnlocks;
        }
    }

    // Check all cars and auto-unlock if player has enough currency
    void CheckAutoUnlocks(int _)
    {
        foreach (var price in carPrices)
        {
            // Skip if already unlocked
            if (IsCarUnlocked(price.carIndex))
                continue;

            // Check if player has enough currency
            if (CanAffordCar(price.carIndex))
            {
                AutoUnlockCar(price.carIndex);
            }
        }
    }

    // Auto unlock car when player reaches required amount
    void AutoUnlockCar(int carIndex)
    {
        if (IsCarUnlocked(carIndex))
            return;

        // DataManager.Instance.UnlockCar(carIndex);
        OnCarUnlocked?.Invoke(carIndex);

        LogHelper.Log($"🎉 Car {carIndex} AUTO-UNLOCKED! You now have enough currency!");
        
        // Optional: Show a popup notification here
        // UIManager.Instance?.ShowCarUnlockedPopup(carIndex);
    }

    // Check if car is unlocked
    public bool IsCarUnlocked(int carIndex)
    {
        // if (DataManager.Instance != null)
        // {
        //     return DataManager.Instance.IsCarUnlocked(carIndex);
        // }
        return false;
    }

    // Get car price
    public CarPrice GetCarPrice(int carIndex)
    {
        foreach (var price in carPrices)
        {
            if (price.carIndex == carIndex)
                return price;
        }
        return null;
    }

    // Check if player can afford car
    public bool CanAffordCar(int carIndex)
    {
        CarPrice price = GetCarPrice(carIndex);
        if (price == null)
            return false;

        bool hasEnoughCoins = CurrencyManager.Instance.HasEnoughCoins(price.coinPrice);
        bool hasEnoughKeys = CurrencyManager.Instance.HasEnoughKeys(price.keyPrice);

        return hasEnoughCoins && hasEnoughKeys;
    }

    // Unlock car without payment (for rewards, ads, etc.)
    public void UnlockCarFree(int carIndex)
    {
        if (!IsCarUnlocked(carIndex))
        {
            // DataManager.Instance.UnlockCar(carIndex);
            OnCarUnlocked?.Invoke(carIndex);
            LogHelper.Log($"Car {carIndex} unlocked for free!");
        }
    }

    // [ContextMenu("Setup Default Prices")]
    // void SetupDefaultPrices()
    // {
    //     carPrices = new CarPrice[]
    //     {
    //         new CarPrice { carIndex = 0, coinPrice = 0, keyPrice = 0 },      // Free
    //         new CarPrice { carIndex = 1, coinPrice = 500, keyPrice = 0 },
    //         new CarPrice { carIndex = 2, coinPrice = 1000, keyPrice = 0 },
    //         new CarPrice { carIndex = 3, coinPrice = 1500, keyPrice = 1 },
    //         new CarPrice { carIndex = 4, coinPrice = 2000, keyPrice = 2 },
    //     };
    //     LogHelper.Log("Default car prices set up");
    // }
}


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