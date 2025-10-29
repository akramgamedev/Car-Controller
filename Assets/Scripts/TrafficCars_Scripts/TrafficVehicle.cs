using NUnit.Framework;
using UnityEngine;

public abstract class TrafficVehicle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 10f;

    protected bool isMoving = false;
    protected bool isDisabled = false;

    public virtual void StartMoving()
    {
        if (!isDisabled)
        {
            isMoving = true;
        }
    }

    public virtual void StopMoving()
    {
        isMoving = false;
    }

    public virtual void DisabledVehicle()
    {
        isDisabled = true;
        isMoving = false;
    }

    protected abstract void Move();

    protected virtual void Update()
    {
        if(isMoving && !isDisabled)
        {
            Move();
        }
    }
}
