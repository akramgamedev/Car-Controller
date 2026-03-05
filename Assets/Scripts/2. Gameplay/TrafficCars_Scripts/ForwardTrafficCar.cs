using UnityEngine;

public class ForwardTrafficCar : TrafficVehicle
{
    [Header("Forward Movement")]
    [SerializeField] private bool useRigidbody = false;
    [SerializeField] private Rigidbody vehicleRigidbody;
    private Vector3 cachedForward;
    private int lastForwardFrame = -1;
    private bool hasRigidbody = false;

    void Awake()
    {
        hasRigidbody = useRigidbody && vehicleRigidbody != null;
    }

    protected override void Move()
    {
        // if (useRigidbody && vehicleRigidbody != null)
        // {
        //     Vector3 targetVelocity = transform.forward * moveSpeed;
        //     vehicleRigidbody.linearVelocity = Vector3.Lerp(vehicleRigidbody.linearVelocity, targetVelocity, Time.deltaTime * 5f);
        // }
        if (hasRigidbody)
        {
            // Cache forward direction once per frame
            if (lastForwardFrame != Time.frameCount)
            {
                cachedForward = transform.forward;
                lastForwardFrame = Time.frameCount;
            }

            Vector3 targetVelocity = cachedForward * moveSpeed;
            //vehicleRigidbody.linearVelocity = Vector3.Lerp(vehicleRigidbody.linearVelocity, targetVelocity, Time.deltaTime * 5f);
            float lerpFactor = Time.deltaTime * 5f;
            vehicleRigidbody.linearVelocity = Vector3.Lerp(vehicleRigidbody.linearVelocity, targetVelocity, lerpFactor);

        }
        else
        {
            //transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            float deltaMove = moveSpeed * Time.deltaTime;
            transform.Translate(Vector3.forward * deltaMove, Space.Self);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        LogHelper.Log($"{gameObject.name} speed changed to {newSpeed}");
    }

    public override void StopMoving()
    {
        base.StopMoving();

        if (hasRigidbody)
        {
            vehicleRigidbody.linearVelocity = Vector3.zero;
        }
    }

    public override void ResetVehicle()
    {
        base.ResetVehicle();

        if (hasRigidbody)
        {
            vehicleRigidbody.linearVelocity = Vector3.zero;
            vehicleRigidbody.angularVelocity = Vector3.zero;
        }
    }
}


// using UnityEngine;

// public class ForwardTrafficCar : TrafficVehicle
// {
//     [Header("Forward Movement")]
//     [SerializeField] private bool useRigidbody = false;
//     [SerializeField] private Rigidbody vehicleRigidbody;

//     protected override void Move()
//     {
//         if (useRigidbody && vehicleRigidbody != null)
//         {
//             Vector3 targetVelocity = transform.forward * moveSpeed;
//             vehicleRigidbody.linearVelocity = Vector3.Lerp(vehicleRigidbody.linearVelocity, targetVelocity, Time.deltaTime * 5f);
//         }
//         else
//         {
//             transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
//         }
//     }

//     public void SetSpeed(float newSpeed)
//     {
//         moveSpeed = newSpeed;
//     }

//     public override void StopMoving()
//     {
//         base.StopMoving();

//         if (useRigidbody && vehicleRigidbody != null)
//         {
//             vehicleRigidbody.linearVelocity = Vector3.zero;
//         }
//     }

//     public override void ResetVehicle()
//     {
//         base.ResetVehicle();

//         if (useRigidbody && vehicleRigidbody != null)
//         {
//             vehicleRigidbody.linearVelocity = Vector3.zero;
//             vehicleRigidbody.angularVelocity = Vector3.zero;
//         }
//     }
// }