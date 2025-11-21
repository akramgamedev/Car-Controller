using System.Collections.Generic;
using UnityEngine;

public class VIPUnlockHandler : BaseCarUnlockHandler
{
    public VIPUnlockHandler(DataManager manager, int page) : base(manager, page)
    {
    }

    public override GlobalEnums.CarUnlockType GetUnlockType()
    {
        return GlobalEnums.CarUnlockType.VIPUnlock;
    }

    public override bool CanUnlock()
    {
        // Check IAP purchase status
        bool hasVIPPurchase = dataManager.gameData.inApp.GetRemoveAds();
        return hasVIPPurchase && GetLockedCarsInPage().Count > 0;
    }

    public override bool TryUnlock(out int unlockedCarIndex)
    {
        unlockedCarIndex = -1;

        if (!CanUnlock())
        {
            LogHelper.LogWarning("VIP purchase required to unlock VIP cars.");
            return false;
        }

        List<int> lockedCars = GetLockedCarsInPage();
        unlockedCarIndex = UnlockRandomCar(lockedCars);

        if (unlockedCarIndex >= 0)
        {
            StaticEvents.CarUnlockEvents.OnCarUnlocked?.Invoke(unlockedCarIndex, GetUnlockType());
            LogHelper.Log($"Unlocked VIP car {unlockedCarIndex}!");
            return true;
        }

        return false;
    }

    public void UnlockAllVIPCars()
    {
        List<int> lockedCars = GetLockedCarsInPage();

        foreach (int carIndex in lockedCars)
        {
            var car = dataManager.gameData.carData.cars[carIndex];
            car.isUnlocked = true;
            dataManager.gameData.carData.cars[carIndex] = car;

            StaticEvents.CarUnlockEvents.OnCarUnlocked?.Invoke(carIndex, GetUnlockType());
        }

        dataManager.SaveGameData();
        LogHelper.Log($"Unlocked all VIP cars!");
    }
}
