using System.Collections.Generic;

public class CashUnlockHandler : BaseCarUnlockHandler
{
    private int coinCost;
    public CashUnlockHandler(DataManager manager, int page, int cost) : base(manager, page)
    {
        coinCost = cost;
    }

    public override GlobalEnums.CarUnlockType GetUnlockType()
    {
        return GlobalEnums.CarUnlockType.CashUnlock;
    }

    public override bool CanUnlock()
    {
        int currentCoins = StaticEvents.GameEconomy.OnGetCurrency?.Invoke(GlobalEnums.CurrencyType.Coin) ?? 0;
        bool hasLockedCars = GetLockedCarsInPage().Count > 0;

        return currentCoins >= coinCost && hasLockedCars;
    }

    public override bool TryUnlock(out int unlockedCarIndex)
    {
        unlockedCarIndex = -1;

        if (!CanUnlock())
        {
            LogHelper.LogWarning($"Cannot unlock cash car on page {pageIndex}. Not enough coins or no locked cars.");
            return false;
        }

        StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(-coinCost, GlobalEnums.CurrencyType.Coin);

        List<int> lockedCars = GetLockedCarsInPage();
        unlockedCarIndex = UnlockRandomCar(lockedCars);

        if (unlockedCarIndex >= 0)
        {
            StaticEvents.CarUnlockEvents.OnCarUnlocked?.Invoke(unlockedCarIndex, GetUnlockType());
            LogHelper.Log($"Unlocked cash car {unlockedCarIndex} on page {pageIndex}!");
            return true;
        }
        return false;
    }
}
