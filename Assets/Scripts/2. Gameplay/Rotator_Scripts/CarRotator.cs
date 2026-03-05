using UnityEngine;

public class CarRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }
    void Update()
    {
        _transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
