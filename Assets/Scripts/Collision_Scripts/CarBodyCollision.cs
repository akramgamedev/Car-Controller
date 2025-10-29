using UnityEngine;

public class CarBodyCollision : MonoBehaviour
{
    private CarCollision parentController;

    public void Initialize(CarCollision controller)
    {
        parentController = controller;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (parentController != null)
        {
            parentController.OnCarCollision(collision);
        }
    }
}