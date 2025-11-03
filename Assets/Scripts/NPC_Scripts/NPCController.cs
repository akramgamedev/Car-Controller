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