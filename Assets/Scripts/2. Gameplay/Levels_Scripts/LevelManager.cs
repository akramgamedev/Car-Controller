using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("All Levels")]
    [SerializeField] private List<LevelHandler> allLevels = new List<LevelHandler>();
    private LevelHandler currentLevelHandler;
    private int currentLevelIndex = 0;
    [SerializeField] SplineCarController playerCar;

    private static LevelManager instance;
    public static LevelManager Instance => instance;
    private LevelUnlockData LevelData => DataManager.Instance?.gameData?.level;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #region  Subscription
    void OnEnable()
    {
        StaticEvents.GameEvents.OnGameWin += OnLevelComplete;
    }
    void OnDisable()
    {
        StaticEvents.GameEvents.OnGameWin -= OnLevelComplete;
    }
    #endregion
    private void OnLevelComplete()
    {
        currentLevelHandler.GiveCompletionReward();
        LevelData.SetUnlockedLevelIndex(currentLevelIndex +1);
        DataManager.Instance.SaveGameData();
    }
    private void Start()
    {
        // If possible use a serialized reference instead
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCar = playerObj.GetComponent<SplineCarController>();
        }

        if (playerCar == null)
        {
            LogHelper.LogWarning("SplineCarController (Player) not found in scene");
        }

         LoadLevel( Mathf.Clamp(DataManager.Instance.gameData.level.GetUnlockedLevelIndex(),0,allLevels.Count - 1));
    }
    public void LoadLevel(int levelIndex)
    {
        foreach (LevelHandler level in allLevels)
        {
            if (level != null)
                level.DeactivateLevel();
        }
        // Here handle , if levels are 20 and game is infinite , then level 21 will mean level 1 or any other game design requirement
        if (levelIndex < 0 || levelIndex >= allLevels.Count)
        {
            LogHelper.LogError($"Level index {levelIndex} out of range!");
            return;
        }

        currentLevelHandler = allLevels[levelIndex];
        currentLevelHandler.ActivateLevel(playerCar);
       
        currentLevelIndex = levelIndex;
        LogHelper.Log($"Loaded Level {levelIndex}");
    }
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
    public int GetTotalLevelCount()
    {
        return allLevels.Count;
    }
    [ContextMenu("Auto-Find All Levels")]
    private void AutoFindLevels()
    {
        allLevels.Clear();
        LevelHandler[] levels = FindObjectsByType<LevelHandler>(FindObjectsSortMode.None);
        allLevels.AddRange(levels);
        LogHelper.Log($"Found {allLevels.Count} levels in scene");
    }
}
