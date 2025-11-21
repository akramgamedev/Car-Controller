using UnityEngine;
using System.Collections.Generic;

public class ProgressionUnlockHandler : BaseCarUnlockHandler
{
    private int levelsRequiredForUnlock;
    private float fillAmountPerLevel;

    public ProgressionUnlockHandler(DataManager manager, int page, int levelsRequired, float fillPerLevel) : base(manager, page)
    {
        levelsRequiredForUnlock = levelsRequired;
        fillAmountPerLevel = fillPerLevel;
    }

    public override GlobalEnums.CarUnlockType GetUnlockType()
    {
        return GlobalEnums.CarUnlockType.ProgressionUnlock;
    }

    public override bool CanUnlock()
    {
        return dataManager.gameData.carData.progressionFillAmount >= 1f &&
               GetLockedCarsInPage().Count > 0;
    }

    public override bool TryUnlock(out int unlockedCarIndex)
    {
        unlockedCarIndex = -1;

        if (!CanUnlock())
        {
            return false;
        }

        dataManager.gameData.carData.progressionFillAmount = 0f;

        List<int> lockedCars = GetLockedCarsInPage();
        unlockedCarIndex = UnlockRandomCar(lockedCars);

        if (unlockedCarIndex >= 0)
        {
            StaticEvents.CarUnlockEvents.OnCarUnlocked?.Invoke(unlockedCarIndex, GetUnlockType());
            LogHelper.Log($"Unlocked progression car {unlockedCarIndex}!");
            return true;
        }

        return false;
    }

    public void OnLevelCompleted()
    {
        dataManager.gameData.carData.completedLevels++;
        dataManager.gameData.carData.progressionFillAmount += fillAmountPerLevel;

        if (dataManager.gameData.carData.progressionFillAmount > 1f)
            dataManager.gameData.carData.progressionFillAmount = 1f;

        dataManager.SaveGameData();

        LogHelper.Log($"Progression: {dataManager.gameData.carData.progressionFillAmount * 100}%");

        if (CanUnlock())
        {
            TryUnlock(out int _);
        }
    }
}
