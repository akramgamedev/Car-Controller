using UnityEngine;

public class NPCController: MonoBehaviour
{
    [Header("Navigation")]
    public Transform[] waypoints;
    public float walkSpeed = 1.5f;
    public float rotationSpeed = 5f;
    public float waypointReachDistance = 0.5f;

    private Animator animator;
    private int currentWaypointIndex = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        if(waypoints == null || waypoints.Length==0)
        {
            LogHelper.LogWarning("No waypoints assigned to " + name);
            enabled = false;
            return;
        }
    }

    void Update()
    {

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * walkSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("Speed", walkSpeed / 3f);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= waypointReachDistance)
            GoToNextWaypoint();
    }
    
    void GoToNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length - 1)
        {
            currentWaypointIndex++;
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            enabled = false;
        }
    }

}



//********* Using Navmesh **********
// using UnityEngine;
// using UnityEngine.AI;

// public class NPCController : MonoBehaviour
// {
//     [Header("navigation")]
//     public Transform[] waypoints;
//     public float walkSpeed = 1.5f;
//     public float waypointReachDistance = 0.5f;

//     private NavMeshAgent agent;
//     private Animator animator;
//     private int currentWaypointIndex = 0;

//     void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         animator = GetComponent<Animator>();

//         agent.speed = walkSpeed;

//         if (waypoints != null && waypoints.Length > 0)
//         {
//             agent.SetDestination(waypoints[currentWaypointIndex].position);
//         }
//     }

//     void Update()
//     {
//         if (waypoints == null || waypoints.Length == 0) return;

//         float normalized = agent.velocity.magnitude / agent.speed;

//         //float speed = agent.velocity.magnitude;
//         animator.SetFloat("Speed", normalized);


//         if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
//             GoToNextWaypoint();
//     }

//     void GoToNextWaypoint()
//     {
//         if (currentWaypointIndex < waypoints.Length - 1)
//         {
//             currentWaypointIndex++;
//             agent.SetDestination(waypoints[currentWaypointIndex].position);
//         }
//         else
//         {
//             agent.isStopped = true;
//             animator.SetFloat("Speed", 0f);
//         }

//     }
// }

//************* complete NPC controller *****************
// using UnityEngine;
// using UnityEngine.AI;
// using System;

// public class NPCController : MonoBehaviour
// {
//     private Animator animator;
//     private NavMeshAgent agent;
//     private float timer;
//     private bool isFalling = false;

//     private string holeLayerName = "GroundHole";
//     private int holeLayer;
//     private int obstacleLayer;

//     public Action OnNPCDestroyed;

//     [Header("Vision Settings")]
//     public float visionDistance = 2f;
//     public float visionHeightOffset = 0.5f;
//     public float swerveDistance = 3f;

//     [Header("Movement Settings")]
//     public float walkSpeed = 1.5f;
//     public float runSpeed = 3.5f;

//     private enum NPCState { Idle, Walk, Run, Wave, Jump }
//     private NPCState currentState;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         agent = GetComponent<NavMeshAgent>();
//         holeLayer = LayerMask.NameToLayer(holeLayerName);
//         obstacleLayer = LayerMask.NameToLayer("Obstacles");

//         PickRandomState();
//     }

//     void Update()
//     {
//         if (isFalling) return;

//         timer -= Time.deltaTime;

//         // 🎬 Update Animator Speed Parameter
//         float speed = agent.velocity.magnitude;
//         animator.SetFloat("Speed", speed);

//         // check obstacles when moving
//         if ((currentState == NPCState.Walk || currentState == NPCState.Run) && agent.hasPath)
//         {
//             Vector3 rayOrigin = transform.position + Vector3.up * visionHeightOffset;

//             if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, visionDistance, 1 << obstacleLayer))
//             {
//                 Debug.DrawRay(rayOrigin, transform.forward * visionDistance, Color.red);
//                 SwerveAroundObstacle();
//                 return;
//             }
//             else
//             {
//                 Debug.DrawRay(rayOrigin, transform.forward * visionDistance, Color.green);
//             }
//         }

//         // Re-pick state if timer is up or NPC is stuck
//         if (timer <= 0f || (!agent.hasPath && (currentState == NPCState.Walk || currentState == NPCState.Run)))
//         {
//             PickRandomState();
//         }
//     }

//     void PickRandomState()
//     {
//         currentState = (NPCState)UnityEngine.Random.Range(0, 4); // Idle, Walk, Run, Wave

//         switch (currentState)
//         {
//             case NPCState.Walk:
//                 agent.speed = walkSpeed;
//                 SetRandomDestination();
//                 break;

//             case NPCState.Run:
//                 agent.speed = runSpeed;
//                 SetRandomDestination();
//                 break;

//             case NPCState.Wave:
//                 agent.ResetPath();
//                 animator.SetTrigger("Wave");
//                 break;
//             case NPCState.Jump:
//                 agent.ResetPath();
//                 animator.SetTrigger("Jump");
//                 break;

//             default: // Idle
//                 agent.ResetPath();
//                 break;
//         }

//         timer = UnityEngine.Random.Range(3f, 8f);
//     }

//     void SetRandomDestination()
//     {
//         Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 10f;
//         randomDirection += transform.position;

//         if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
//         {
//             agent.SetDestination(hit.position);
//         }
//     }

//     void SwerveAroundObstacle()
//     {
//         Vector3 side = UnityEngine.Random.value > 0.5f ? transform.right : -transform.right;
//         Vector3 swerveTarget = transform.position + side * swerveDistance;

//         if (NavMesh.SamplePosition(swerveTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
//         {
//             agent.SetDestination(hit.position);
//         }
//         else
//         {
//             PickRandomState();
//         }
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.gameObject.layer == holeLayer && !isFalling)
//         {
//             isFalling = true;
//             agent.enabled = false;

//             animator.SetTrigger("Fall");
//             OnNPCDestroyed?.Invoke();
//         }
//     }
// }
