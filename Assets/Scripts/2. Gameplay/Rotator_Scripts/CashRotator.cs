using UnityEngine;

public class CashRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
