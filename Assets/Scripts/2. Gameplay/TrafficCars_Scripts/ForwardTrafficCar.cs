using UnityEngine;

public class ForwardTrafficCar : TrafficVehicle
{
    [Header("Forward Movement")]
    [SerializeField] private bool useRigidbody = false;
    [SerializeField] private Rigidbody vehicleRigidbody;

    protected override void Move()
    {
        if (useRigidbody && vehicleRigidbody != null)
        {
            Vector3 targetVelocity = transform.forward * moveSpeed;
            vehicleRigidbody.linearVelocity = Vector3.Lerp(vehicleRigidbody.linearVelocity, targetVelocity, Time.deltaTime * 5f);
        }
        else
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
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
        
        if (useRigidbody && vehicleRigidbody != null)
        {
            vehicleRigidbody.linearVelocity = Vector3.zero;
        }
    }

    public override void ResetVehicle()
    {
        base.ResetVehicle();
        
        if (useRigidbody && vehicleRigidbody != null)
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