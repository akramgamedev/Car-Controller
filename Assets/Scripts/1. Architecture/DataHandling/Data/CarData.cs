using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CarProperties
{
    public bool isUnlocked;
    public GlobalEnums.CarUnlockType unlockType;
    public string identifier;
    public int pageIndex;

    public CarProperties(string id, bool unlocked /*= false*/, GlobalEnums.CarUnlockType type, int page)
    {
        identifier = id;
        isUnlocked = unlocked;
        unlockType = type;
        pageIndex = page;
    }
}
[System.Serializable]
public class CarData
{
    public int selectedCarIndex;
    public List<CarProperties> cars;
    public int completedLevels;
    public float progressionFillAmount;

    public CarData()
    {
        selectedCarIndex = 0;
        cars = new List<CarProperties>();
        completedLevels = 0;
        progressionFillAmount = 0f;
    }
    public void AddCarToList(string identifier, GlobalEnums.CarUnlockType unlockType, int pageIndex)
    {
        cars.Add(new CarProperties(identifier, false, unlockType, pageIndex));
    }
}
