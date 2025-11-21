using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseCarUnlockHandler
{
    protected DataManager dataManager;
    protected int pageIndex;

    public BaseCarUnlockHandler(DataManager manager, int page)
    {
        dataManager=manager;
        pageIndex=page;
    }

    public abstract bool CanUnlock();
    public abstract bool TryUnlock(out int unlockedCarIndex);
    public abstract GlobalEnums.CarUnlockType GetUnlockType();


    protected List<int> GetLockedCarsInPage()
    {
        List<int> lockedCars= new List< int >();
        for(int i=0;i<dataManager.gameData.carData.cars.Count; i++)
        {
            var car=dataManager.gameData.carData.cars[i];
            if(!car.isUnlocked &&
            car.unlockType==GetUnlockType() &&
            car.pageIndex == pageIndex)
            {
                lockedCars.Add(i);
            }
        }
        return lockedCars;
    }

    protected int UnlockRandomCar(List<int> availableCars)
    {
        if(availableCars.Count ==0 ) return -1;

        int randomIndex=Random.Range(0, availableCars.Count);
        int carIndex=availableCars[randomIndex];

        var car=dataManager.gameData.carData.cars[carIndex];
        car.isUnlocked=true;
        dataManager.gameData.carData.cars[carIndex]=car;
        dataManager.SaveGameData();

        return carIndex;
    }
}
