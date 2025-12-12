// ====================================================================
// CheckpointManager.cs - Singleton Manager
// ====================================================================
using UnityEngine;
/// <summary>
/// Singleton manager that handles checkpoint saving/loading
/// This creates itself automatically, no need to manually add to scene
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    private static CheckpointManager instance;
    public static CheckpointManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CheckpointManager");
                instance = go.AddComponent<CheckpointManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const string CHECKPOINT_KEY = "CheckpointData";
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Save checkpoint data when car triggers checkpoint
    /// </summary>
    public void SaveCheckpoint(SplineCarController car)
    {
        if (car == null || car.splineContainer == null)
        {
            Debug.LogError("[CheckpointManager] Cannot save - car or spline is null!");
            return;
        }

        CheckpointData data = new CheckpointData
        {
            splineProgress = car.GetSplineProgress(),
            position = car.transform.position,
            rotation = car.transform.rotation,
            currentSpeed = car.CurrentSpeed,
            splineName = car.splineContainer.name,
            hasCheckpoint = true
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(CHECKPOINT_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"[CheckpointManager] Checkpoint saved at progress: {data.splineProgress:F3}");
    }

    /// <summary>
    /// Load checkpoint data and restore car position
    /// Returns true if checkpoint was loaded successfully
    /// </summary>
    public bool LoadCheckpoint(SplineCarController car)
    {
        if (!HasCheckpoint())
        {
            Debug.Log("[CheckpointManager] No checkpoint data found.");
            return false;
        }

        string json = PlayerPrefs.GetString(CHECKPOINT_KEY);
        CheckpointData data = JsonUtility.FromJson<CheckpointData>(json);

        if (car == null)
        {
            Debug.LogError("[CheckpointManager] Car controller is null!");
            return false;
        }

        // Verify spline matches
        if (car.splineContainer != null && car.splineContainer.name != data.splineName)
        {
            Debug.LogWarning($"[CheckpointManager] Spline mismatch. Expected: {data.splineName}, Got: {car.splineContainer.name}");
            return false;
        }

        // Restore car state
        car.RestoreFromCheckpoint(data.splineProgress, data.position, data.rotation, data.currentSpeed);

        Debug.Log($"[CheckpointManager] Checkpoint loaded at progress: {data.splineProgress:F3}");
        return true;
    }

    /// <summary>
    /// Check if checkpoint data exists
    /// </summary>
    public bool HasCheckpoint()
    {
        if (!PlayerPrefs.HasKey(CHECKPOINT_KEY))
            return false;

        string json = PlayerPrefs.GetString(CHECKPOINT_KEY);
        CheckpointData data = JsonUtility.FromJson<CheckpointData>(json);
        return data.hasCheckpoint;
    }

    /// <summary>
    /// Clear saved checkpoint
    /// </summary>
    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(CHECKPOINT_KEY);
        PlayerPrefs.Save();
        Debug.Log("[CheckpointManager] Checkpoint cleared.");
    }

    /// <summary>
    /// Get checkpoint data without loading (for UI/debug)
    /// </summary>
    public CheckpointData GetCheckpointData()
    {
        if (!HasCheckpoint())
            return new CheckpointData();

        string json = PlayerPrefs.GetString(CHECKPOINT_KEY);
        return JsonUtility.FromJson<CheckpointData>(json);
    }
}