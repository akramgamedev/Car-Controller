using UnityEngine;

public class ForwardTrafficCar : TrafficVehicle
{
    protected override void Move()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}