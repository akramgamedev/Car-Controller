using UnityEngine;

public class SimpleTrafficCar : TrafficCarBase
{
    //[SerializeField] private Vector3 moveDirection = Vector3.forward;
    [SerializeField] private float destroyDistance = 50f;

    private bool isLocked = false;
    protected override void Move()
    {
        if (isLocked) return;
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision other)
    {
            StoppedByCollision();
    }
}
