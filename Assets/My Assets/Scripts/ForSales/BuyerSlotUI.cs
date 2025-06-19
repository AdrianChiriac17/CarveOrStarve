using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyerSlotUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text shapeText;
    [SerializeField] private Image colorSwatch;
    [SerializeField] private TMP_Text wealthText;

    /// <summary>Called by BuyerManager to populate this slot.</summary>
    public void Setup(Buyer buyer)
    {
        nameText.text = buyer.FullName;
        avatarImage.sprite = buyer.Avatar;
        shapeText.text = buyer.FavoriteShape.ToString();
        colorSwatch.color = buyer.FavoriteColor;
        wealthText.text = buyer.Wealth switch
        {
            WealthLevel.Dollar => "$",
            WealthLevel.DoubleDollar => "$$",
            WealthLevel.TripleDollar => "$$$",
            _ => ""
        };
    }
}