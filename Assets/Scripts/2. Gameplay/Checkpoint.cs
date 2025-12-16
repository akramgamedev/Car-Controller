using UnityEngine;

/// <summary>
/// Place this on checkpoint trigger objects in your scene
/// Checkpoint should have:
/// - Collider (IsTrigger = true)
/// - Tag: "Checkpoint"
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private bool showDebugMessages = true;
    
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Car" tag
        if (!hasBeenTriggered && other.CompareTag("Car"))
        {
            hasBeenTriggered = true;
            
            // Get the SplineCarController from the parent
            SplineCarController carController = other.GetComponentInParent<SplineCarController>();
            
            if (carController != null)
            {
                // Save the checkpoint
                CheckpointManager.Instance.SaveCheckpoint(carController);
                
                if (showDebugMessages)
                {
                    LogHelper.Log($"[Checkpoint] Triggered by {other.name}. Position saved!");
                }
                
                // Destroy checkpoint after delay
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                LogHelper.LogWarning("[Checkpoint] Car collider found but SplineCarController not found in parent!");
            }
        }
    }
}