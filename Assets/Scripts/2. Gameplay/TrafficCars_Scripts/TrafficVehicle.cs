using UnityEngine;

public abstract class TrafficVehicle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 10f;

    [Header("Reset Settings")]
    [SerializeField] protected bool saveInitialPosition = true;

    public bool isMoving = false;
    protected bool isDisabled = false;
    public bool IsDisabled => isDisabled;

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
            LogHelper.Log($"{gameObject.name} StartMoving() called - isMoving = {isMoving}");
        }
    }

    public virtual void StopMoving()
    {
        isMoving = false;
        LogHelper.Log($"{gameObject.name} StopMoving() called - isMoving = {isMoving}");
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
