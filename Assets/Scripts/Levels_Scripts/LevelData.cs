using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class LevelData : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] private int levelNumber;
    [SerializeField] private string levelName;

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
        // Add any reset logic you might want later, like respawning pickups or resetting checkpoints
        LogHelper.Log($"Level {levelNumber} reset");
    }
}
