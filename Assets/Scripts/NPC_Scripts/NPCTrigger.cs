using System.Collections.Generic;
using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    [Header("NPCs References")]
    public List<NPCController> npcs = new List<NPCController>();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            foreach (NPCController npc in npcs)
            {
                npc.gameObject.SetActive(true);
                LogHelper.Log("NPC activated");
            }
        }
    }
private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw lines to controlled NPC when selected
        Gizmos.color = Color.yellow;
        foreach (NPCController npc in npcs)
        {
            if (npc != null)
            {
                Gizmos.DrawLine(transform.position, npc.transform.position);
            }
        }
    }
}
