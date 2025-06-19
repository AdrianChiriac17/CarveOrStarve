// ItemInfoUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ItemInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text costText;      

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
        // clear any old listeners
        selectButton.onClick.RemoveAllListeners();

        // set name & icon
        nameText.text = data.itemName;
        iconImage.sprite = data.icon;

        // --- calculate & show range ---
        // find cheapest & priciest color multiplier
        float minMult = float.MaxValue, maxMult = float.MinValue;
        foreach (var opt in data.palette.options)
        {
            minMult = Mathf.Min(minMult, opt.multiplier);
            maxMult = Mathf.Max(maxMult, opt.multiplier);
        }

        // compute min & max volume
        float minVol, maxVol;
        if (data.uniformScaling)
        {
            minVol = data.baseVolume * Mathf.Pow(data.uniformRange.x, 3);
            maxVol = data.baseVolume * Mathf.Pow(data.uniformRange.y, 3);
        }
        else
        {
            minVol = data.baseVolume
                   * data.minScale.x * data.minScale.y * data.minScale.z;
            maxVol = data.baseVolume
                   * data.maxScale.x * data.maxScale.y * data.maxScale.z;
        }

        // compute cost range
        float minCost = minVol * data.costPerUnitVolume * minMult;
        float maxCost = maxVol * data.costPerUnitVolume * maxMult;

        //round them up nicely so they look good.
        int displayMinCost = Mathf.Max(1, Mathf.RoundToInt(minCost));
        int displayMaxCost = Mathf.Max(1, Mathf.RoundToInt(maxCost));

        costText.text = $"From ${displayMinCost} to ${displayMaxCost}";
        // --- end range display ---

        // wire up the button
        selectButton.onClick.AddListener(() => onClick(data));
    }
}