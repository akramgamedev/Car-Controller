using UnityEngine;

public abstract class TrafficVehicle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 10f;

    [Header("Reset Settings")]
    [SerializeField] protected bool saveInitialPosition = true;

    protected bool isMoving = false;
    protected bool isDisabled = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;


    protected virtual void Start()
    {
        if (saveInitialPosition)
        {
            SaveInitialTransform();
        }

        gameObject.SetActive(false);
    }

    private void SaveInitialTransform()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

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

    public virtual void ResetVehicle()
    {
        if (saveInitialPosition)
        {
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
        }
        isMoving = false;
        isDisabled = false;
        gameObject.SetActive(true);
    }

    protected abstract void Move();

    protected virtual void Update()
    {
        if (isMoving && !isDisabled)
        {
            Move();
        }
    }
}
