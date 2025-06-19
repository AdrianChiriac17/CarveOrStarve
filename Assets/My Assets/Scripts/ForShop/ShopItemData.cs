using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("UI Display")]
    public string itemName;
    public Sprite icon;

    [Header("World Prefab & Defaults")]
    public GameObject prefab;

    [Header("Pricing")]
    public float costPerUnitVolume = 10f;  // base rate

    [Header("Scaling Rules")]
    public bool uniformScaling = true;
    public Vector2 uniformRange = new Vector2(0.5f, 2f);
    public Vector3 minScale = Vector3.one * 0.5f;
    public Vector3 maxScale = Vector3.one * 2f;

    [Header("Shape Geometry")]
    public float baseVolume = 1f;
    public Color defaultColor = Color.white;
    public Vector3 defaultScale = Vector3.one;

    [Header("Shared Color Palette")]
    public ColorPalette palette;

    [Header("Materials")]
    [Tooltip("The normal material (slightly metallic)")]
    public Material defaultMaterial;

    [Tooltip("The checkerboard preview material")]
    public Material checkerboardMaterial;

    public float paidCost;  // what the player paid for this specific block
}