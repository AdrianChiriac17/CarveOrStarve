using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SculptureManager : MonoBehaviour
{
    public static SculptureManager Instance { get; private set; }

    private Dictionary<string, float> colorVolumes = new();           // colorName → volume
    private Dictionary<BlockData.Shape, float> shapeVolumes = new();  // shape → volume
    private float totalPaidCost = 0f;                                  // total spent

    private readonly HashSet<BlockData> registeredBlocks = new();     // to avoid duplicates

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Init shapes to 0
        foreach (BlockData.Shape shape in System.Enum.GetValues(typeof(BlockData.Shape)))
        {
            shapeVolumes[shape] = 0f;
        }

        // colorVolumes will grow as needed (keys are dynamic)
    }

    public void RegisterBlock(BlockData block)
    {
        if (!registeredBlocks.Add(block))
            return; // already added

        // Color
        if (!colorVolumes.ContainsKey(block.colorName))
            colorVolumes[block.colorName] = 0f;
        colorVolumes[block.colorName] += block.volume;

        // Shape
        shapeVolumes[block.shape] += block.volume;

        // Cost
        totalPaidCost += block.paidCost;

        // Log registration
        Debug.Log($"[Sculpture] Registered: {block.colorName} {block.shape} | Volume: {block.volume:0.###} | Cost: ${block.paidCost:0.00}");
    }

    private void Start()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnDayChanged += OnDayChanged;
        else
            Debug.LogError("SculptureManager: CalendarManager.Instance is null in Start!", this);
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnDayChanged -= OnDayChanged;
    }


    // Optional removal (if blocks ever leave the zone)
    public void UnregisterBlock(BlockData block)
    {
        if (!registeredBlocks.Remove(block))
            return;

        colorVolumes[block.colorName] -= block.volume;
        shapeVolumes[block.shape] -= block.volume;
        totalPaidCost -= block.paidCost;
    }

    /// <summary>Clears all sculpture data; called each Monday.</summary>
    public void ClearAll()
    {
        colorVolumes.Clear();
        shapeVolumes.Clear();
        totalPaidCost = 0f;
        registeredBlocks.Clear();

        // Re-initialize shapeVolumes so GetPredominantShape() still works
        foreach (BlockData.Shape shape in System.Enum.GetValues(typeof(BlockData.Shape)))
            shapeVolumes[shape] = 0f;

        Debug.Log("[Sculpture] Cleared all sculpture data for new week.");
    }

    private void OnDayChanged(string dayName, int week)
    {
        if (dayName == "Monday")
            ClearAll();
    }


    public string GetPredominantColorName()
    {
        return colorVolumes
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault().Key;
    }

    public BlockData.Shape GetPredominantShape()
    {
        return shapeVolumes
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault().Key;
    }

    public float GetTotalSculptureCost() => totalPaidCost;
}
