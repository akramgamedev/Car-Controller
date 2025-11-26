using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class LevelHandler : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] private int levelNumber;
    [SerializeField] private string levelName;

    [Header("Level Rewards")]
    [SerializeField] private int levelCompletionCoins = 100;
    [SerializeField] private int levelCompletionKeys = 1;

    [Header("Spline Setup")]
    [SerializeField] private SplineContainer levelSplineContainer;

    [Header("Path Line")]
    [SerializeField] private RoadPathLine pathLine; // Reference to the path line renderer

    private bool isActive = false;

    public int LevelNumber => levelNumber;
    public SplineContainer LevelSplineContainer => levelSplineContainer;

    public void ActivateLevel(SplineCarController playerCar)
    {
        if (isActive) return;

        gameObject.SetActive(true);
        isActive = true;
        ResetLevel();

        if (playerCar != null && levelSplineContainer != null)
        {
            playerCar.SetupNewLevel(levelSplineContainer);

            // Setup path line to follow the same spline
            if (pathLine != null)
            {
                pathLine.car = playerCar.transform;
                pathLine.SetSpline(levelSplineContainer);
                LogHelper.Log($"✓ Path line configured for Level {levelNumber}");
            }

            LogHelper.Log($"✓ Spline assigned and initialized for Level {levelNumber}: {levelName}");
        }
        else
        {
            if (playerCar == null)
                LogHelper.LogError("PlayerCar is null!");
            if (levelSplineContainer == null)
                LogHelper.LogError($"Level {levelNumber} has no spline container assigned!");
        }

        LogHelper.Log($"Level {levelNumber} - {levelName} activated");
    }

    public void DeactivateLevel()
    {
        if (!isActive) return;

        // Clear the path line when deactivating
        if (pathLine != null)
        {
            pathLine.ClearPath();
        }

        gameObject.SetActive(false);
        isActive = false;

        LogHelper.Log($"Level {levelNumber} - {levelName} deactivated");
    }

    public void ResetLevel()
    {
        LogHelper.Log($"Level {levelNumber} reset");
    }

    public void GiveCompletionReward()
    {
        if (levelCompletionCoins > 0)
        {
            StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(levelCompletionCoins, GlobalEnums.CurrencyType.Coin);
        }
        else
        {
            StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(levelCompletionKeys, GlobalEnums.CurrencyType.Key);
        }
    }
}


// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Splines;

// public class LevelHandler : MonoBehaviour
// {
//     [Header("Level Info")]
//     [SerializeField] private int levelNumber;
//     [SerializeField] private string levelName;

//     [Header("Level Rewards")]
//     [SerializeField] private int levelCompletionCoins = 100;
//     [SerializeField] private int levelCompletionKeys = 1;

//     [Header("Spline Setup")]
//     [SerializeField] private SplineContainer levelSplineContainer;

//     private bool isActive = false;

//     public int LevelNumber => levelNumber;
//     public SplineContainer LevelSplineContainer => levelSplineContainer;

//     public void ActivateLevel(SplineCarController playerCar)
//     {
//         if (isActive) return;

//         gameObject.SetActive(true);
//         isActive = true;
//         ResetLevel();

//         if (playerCar != null && levelSplineContainer != null)
//         {
//             playerCar.SetupNewLevel(levelSplineContainer);

//             LogHelper.Log($"✓ Spline assigned and initialized for Level {levelNumber}: {levelName}");
//         }
//         else
//         {
//             if (playerCar == null)
//                 LogHelper.LogError("PlayerCar is null!");
//             if (levelSplineContainer == null)
//                 LogHelper.LogError($"Level {levelNumber} has no spline container assigned!");
//         }

//         LogHelper.Log($"Level {levelNumber} - {levelName} activated");
//     }

//     public void DeactivateLevel()
//     {
//         if (!isActive) return;

//         gameObject.SetActive(false);
//         isActive = false;

//         LogHelper.Log($"Level {levelNumber} - {levelName} deactivated");

//     }

//     public void ResetLevel()
//     {
//         LogHelper.Log($"Level {levelNumber} reset");
//     }
//     public void GiveCompletionReward()
//     {
//         if (levelCompletionCoins > 0)
//         {
//             StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(levelCompletionCoins, GlobalEnums.CurrencyType.Coin);
//         }
//         else
//         {
//             StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(levelCompletionKeys, GlobalEnums.CurrencyType.Key);

//         }
//     }
// }
