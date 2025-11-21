using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager_Temporary : MonoBehaviour
{
    [Header("All Levels")]
    [SerializeField] private List<LevelHandler> allLevels = new List<LevelHandler>();

    [Header("Settings")]
    [SerializeField] private bool loadFirstLevelOnStart = true;

    private LevelHandler currentLevel;
    private int currentLevelIndex = 0;

    private SplineCarController playerCar;

    private static LevelManager_Temporary instance;
    public static LevelManager_Temporary Instance => instance;

    private static int nextLevelToLoad = 0;
    private static bool shouldLoadSpecificLevel = false;

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

        foreach (LevelHandler level in allLevels)
        {
            if (level != null)
                level.DeactivateLevel();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCar = playerObj.GetComponent<SplineCarController>();
        }

        if (playerCar == null)
        {
            LogHelper.LogWarning("SplineCarController (Player) not found in scene");
        }
    }

    private void Start()
    {
        if (shouldLoadSpecificLevel)
        {
            LoadLevel(nextLevelToLoad);
            shouldLoadSpecificLevel = false;
        }
        else if (/*loadFirstLevelOnStart &&*/ allLevels.Count > 0)
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
        currentLevel.ActivateLevel(playerCar);

        LogHelper.Log($"Loaded Level {levelIndex}");
    }

    public void OnLevelCompleted()
    {
        if (currentLevel != null)
        {
            currentLevel.GiveCompletionReward();
        }
    }

    public void LoadNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;

        if (nextIndex < allLevels.Count)
        {
            LogHelper.Log($"Loading next level: {nextIndex}");

            nextLevelToLoad = nextIndex;
            shouldLoadSpecificLevel = true;

            StartCoroutine(ReloadSceneWithDelay());
        }
        else
        {
            LogHelper.Log("No more Levels! Game Completed");
        }
    }

    private IEnumerator ReloadSceneWithDelay()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideLevelSuccessScreen();
        }

        yield return new WaitForSeconds(0.3f);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void LoadPreviousLevel()
    {
        int prevIndex = currentLevelIndex - 1;

        if (prevIndex >= 0)
        {
            LogHelper.Log($"Loading previous level: {prevIndex}");

            nextLevelToLoad = prevIndex;
            shouldLoadSpecificLevel = true;

            StartCoroutine(ReloadSceneWithDelay());
        }
    }

    public void ReloadCurrentLevel()
    {
        LogHelper.Log($"Reloading current level: {currentLevelIndex}");

        nextLevelToLoad = currentLevelIndex;
        shouldLoadSpecificLevel = true;

        StartCoroutine(ReloadSceneWithDelay());
    }

    public int GetCurrentLevelNumber()
    {
        return currentLevel != null ? currentLevel.LevelNumber : -1;
    }

    [ContextMenu("Auto-Find All Levels")]
    private void AutoFindLevels()
    {
        allLevels.Clear();
        LevelHandler[] levels = FindObjectsByType<LevelHandler>(FindObjectsSortMode.None);
        allLevels.AddRange(levels);
        LogHelper.Log($"Found {allLevels.Count} levels in scene");
    }

    private void OnApplicationQuit()
    {
        nextLevelToLoad = 0;
        shouldLoadSpecificLevel = false;
    }
}
