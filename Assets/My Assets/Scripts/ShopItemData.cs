using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item Data")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;          // your thumbnail
    public GameObject prefab;    // the 3D shape

    [Header("Default Customization")]
    public Color defaultColor = Color.red;
    public Vector3 defaultScale = Vector3.one;
}