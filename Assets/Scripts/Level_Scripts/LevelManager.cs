using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("All Levels")]
    [SerializeField] private List<LevelData> allLevels = new List<LevelData>();

    [Header("Settings")]
    [SerializeField] private bool loadFirstLevelOnStart = true;

    private LevelData currentLevel;
    private int currentLevelIndex = 0;

    private static LevelManager instance;
    public static LevelManager Instance => instance;

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

        foreach (LevelData level in allLevels)
        {
            if (level != null)
            {
                level.DeactivateLevel();
            }

        }

    }

    private void Start()
    {
        if (loadFirstLevelOnStart && allLevels.Count > 0)
        {
            LoadLevel(0);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= allLevels.Count)
        {
            LogHelper.LogError($"Level index {levelIndex} out of range!");
            return;
        }

        if (currentLevel != null)
        {
            currentLevel.DeactivateLevel();
        }

        currentLevel = allLevels[levelIndex];
        currentLevelIndex = levelIndex;
        currentLevel.ActivateLevel();

        MovePlayerToSpawn();
    }

    public void LoadNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < allLevels.Count)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            LogHelper.Log("No more Levels! Game Completed");
        }
    }

    public void LoadPreviousLevel()
    {
        int prevIndex = currentLevelIndex - 1;
        if (prevIndex >= 0)
        {
            LoadLevel(prevIndex);
        }
    }

    public void ReloadcurrentLevel()
    {
        if (currentLevel != null)
        {
            currentLevel.ResetLevel();
            MovePlayerToSpawn();
        }
    }

    private void MovePlayerToSpawn()
    {
        if (currentLevel != null && currentLevel.PlayerSpawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = currentLevel.PlayerSpawnPoint.position;
                player.transform.rotation = currentLevel.PlayerSpawnPoint.rotation;
            }
        }
    }

    public int GetcurrentLevelNumber()
    {
        return currentLevel != null ? currentLevel.LevelNumber : -1;
    }
    
    [ContextMenu("Auto-Find All Levels")]
    private void AutoFindLevels()
    {
        allLevels.Clear();
        LevelData[] levels = FindObjectsOfType<LevelData>();
        allLevels.AddRange(levels);
        LogHelper.Log($"Found {allLevels.Count} levels in scene");
    }
}
