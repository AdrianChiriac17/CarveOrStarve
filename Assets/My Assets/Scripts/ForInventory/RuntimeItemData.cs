using UnityEngine;

public class RuntimeItemData
{
    public GameObject prefab;     // the 3D block to spawn
    public Sprite thumbnail;   // the screenshot you grabbed on Buy
    public Vector3 scales;      // the scale you applied in ShopUI
    public Color color;       // the color you chose in ShopUI
    public int cost;        // the final buy price

    public string colorName;               
    public BlockData.Shape shape;

    public Material material;
    public float paidCost;
}