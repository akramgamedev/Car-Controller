using System.Collections.Generic;
using UnityEngine;

public class ChestUnlockHandler : BaseCarUnlockHandler
{
    private int keyCost;
    public ChestUnlockHandler(DataManager manager, int page, int cost) : base(manager, page)
    {
        keyCost = cost;
    }

    public override GlobalEnums.CarUnlockType GetUnlockType()
    {
        return GlobalEnums.CarUnlockType.ChestUnlock;
    }
    public override bool CanUnlock()
    {
        int currentKeys = StaticEvents.GameEconomy.OnGetCurrency?.Invoke(GlobalEnums.CurrencyType.Key) ?? 0;
        bool hasLockedCars = GetLockedCarsInPage().Count > 0;

        return currentKeys >= keyCost && hasLockedCars;
    }

    public override bool TryUnlock(out int unlockedCarIndex)
    {
        unlockedCarIndex = -1;
        if (!CanUnlock())
        {
            LogHelper.LogWarning($"Cannot unlock chest car. Not enough keys or no locked cars.");
            return false;
        }

        StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(-keyCost, GlobalEnums.CurrencyType.Key);

        List<int> lockedCars = GetLockedCarsInPage();
        unlockedCarIndex = UnlockRandomCar(lockedCars);

        if (unlockedCarIndex >= 0)
        {
            StaticEvents.CarUnlockEvents.OnCarUnlocked?.Invoke(unlockedCarIndex, GetUnlockType());
            LogHelper.Log($"Unlocked chest car {unlockedCarIndex}!");
            return true;
        }

        return false;
    }

}

