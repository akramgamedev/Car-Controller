using UnityEngine;
/// <summary>
/// Data structure for checkpoint save
/// </summary>
[System.Serializable]
public class CheckpointData
{
    public float splineProgress;
    public Vector3 position;
    public Quaternion rotation;
    public float currentSpeed;
    public string splineName;
    public bool hasCheckpoint;
}