using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class LevelData : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] private int levelNumber;
    [SerializeField] private string levelName;

    [Header("Level Elements")]
    [SerializeField] private List<TrafficVehicle> allTrafficCars = new List<TrafficVehicle>();
    [SerializeField] private List<TrafficTriggerZone> allTriggerZones = new List<TrafficTriggerZone>();

    [Header("Spline Setup")]
    [SerializeField] private SplineContainer levelSplineContainer;

    private bool isActive = false;

    public int LevelNumber => levelNumber;
    public SplineContainer LevelSplineContainer => levelSplineContainer;

    public void ActivateLevel(SplineCarController playerCar)
    {
        if (isActive) return;

        gameObject.SetActive(true);
        isActive = true;
        ResetLevel();

         // ✅ Assign spline container to player's car
        if (playerCar != null && levelSplineContainer != null)
        {
            playerCar.splineContainer = levelSplineContainer;
            LogHelper.Log($"Spline assigned to Player for Level {levelNumber}: {levelName}");
        }

        LogHelper.Log($"Level {levelNumber} - {levelName} activated");
    }

    public void DeactivateLevel()
    {
        if (!isActive) return;

        gameObject.SetActive(false);
        isActive = false;

        LogHelper.Log($"Level {levelNumber} - {levelName} deactivated");
        
    }

    public void ResetLevel()
    {
        foreach (TrafficVehicle car in allTrafficCars)
        {
            if (car != null)
            {
                car.StopMoving();
                car.gameObject.SetActive(true);

                car.transform.localPosition = car.transform.localPosition;
            }
        }
    }


    [ContextMenu("Auto-Find All Traffic Elements")]
    private void AutoFindElements()
    {
        allTrafficCars.Clear();
        allTriggerZones.Clear();

        TrafficVehicle[] cars = GetComponentsInChildren<TrafficVehicle>(true);
        allTrafficCars.AddRange(cars);

        TrafficTriggerZone[] zones = GetComponentsInChildren<TrafficTriggerZone>(true);
        allTriggerZones.AddRange(zones);

        LogHelper.Log($"Found {allTrafficCars.Count} traffic cars and {allTriggerZones.Count} trigger zones in {levelName}");

    }
}
