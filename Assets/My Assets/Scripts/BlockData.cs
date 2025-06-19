using UnityEngine;

public class BlockData : MonoBehaviour
{
    public enum Shape
    {
        Cube,
        Sphere,
        Cylinder,
        Capsule,
        Prism
    }

    [Tooltip("The shape type of this block")]
    public Shape shape;

    [Tooltip("The name of the chosen color (matches color palette)")]
    public string colorName;

    [Tooltip("The physical volume of the block")]
    public float volume;

    public float paidCost;  // stored on the placed GameObject
}