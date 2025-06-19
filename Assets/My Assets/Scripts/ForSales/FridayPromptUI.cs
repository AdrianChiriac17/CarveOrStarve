using TMPro;
using UnityEngine;

public class FridayPromptUI : MonoBehaviour
{
    [Header("References to your UI Texts")]
    [SerializeField] private TMP_Text totalCostText;
    [SerializeField] private TMP_Text predominantColorText;
    [SerializeField] private TMP_Text predominantShapeText;

    private void OnEnable()
    {
        // 1) Get all data from SculptureManager
        float totalCost = SculptureManager.Instance.GetTotalSculptureCost();
        string topColor = SculptureManager.Instance.GetPredominantColorName();
        BlockData.Shape topShape = SculptureManager.Instance.GetPredominantShape();

        // 2) Populate your TMP_Text fields
        totalCostText.text = $"Materials cost: ${totalCost:0.00}";
        predominantColorText.text = $"Most used color: {topColor}";
        predominantShapeText.text = $"Most used shape: {topShape}";
    }
}