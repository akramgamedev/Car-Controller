using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CarProperties{
    public bool isUnlocked;
    public CarProperties(bool unlocked = false)
    {
        isUnlocked = unlocked;
    }
}
[System.Serializable]
public class CarData
{
    public int selectedCarIndex;
    public List<CarProperties> cars;

    public CarData()
    {
        selectedCarIndex=0;
        cars = new List<CarProperties>();
    }
    public void AddCarToList()
    {
        cars.Add(new CarProperties());
    }
}
