using NUnit.Framework;
using UnityEngine;

public abstract class TrafficCarBase : MonoBehaviour
{
    [Header("Traffic car settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected bool isMoving = false;

    protected bool isStoppedByCollision = false;
    protected Vector3 startPosition;

    protected virtual void Start()
    {
        startPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (isMoving && !isStoppedByCollision)
            Move();
    }

    protected abstract void Move();

    public virtual void StartMoving()
    {
        if(!isStoppedByCollision)
        isMoving = true;
    }

    public virtual void StopMoving()
    {

        isMoving = false;
    }

    public virtual void StoppedByCollision()
    {
        
        isMoving = false;
        isStoppedByCollision = true;
    }

    public virtual void ResetPosition()
    {
        transform.position = startPosition;
        isMoving = false;
        isStoppedByCollision = false;
    }

}
