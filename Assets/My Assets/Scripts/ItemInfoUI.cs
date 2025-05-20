// ItemInfoUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ItemInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    private Button selectButton;

    private void Awake()
    {
        selectButton = GetComponent<Button>();
    }

    /// <summary>
    /// Initialize this row with data + click callback
    /// </summary>
    public void Init(ShopItemData data, Action<ShopItemData> onClick)
    {
        Debug.Log($"[Init] nameText={(nameText == null ? "NULL" : nameText.name)}, btn={(selectButton == null ? "NULL" : "OK")}");

        nameText.text = data.itemName;
        iconImage.sprite = data.icon;
        selectButton.onClick.AddListener(() => onClick(data));
    }
}