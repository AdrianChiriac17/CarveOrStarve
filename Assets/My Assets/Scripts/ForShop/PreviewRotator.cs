using UnityEngine;

// Simple component to spin a preview object around its Y-axis.
public class PreviewRotator : MonoBehaviour
{
    [Tooltip("Degrees per second of rotation around the Y-axis")]
    public float rotationSpeed = 60f;

    void Update()
    {
        // Rotate around world Y, so it spins in place
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}