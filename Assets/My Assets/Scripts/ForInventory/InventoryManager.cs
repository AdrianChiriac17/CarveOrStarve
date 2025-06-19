using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public const int Capacity = 9;

    // internal list of items in the hotbar
    private List<RuntimeItemData> items = new List<RuntimeItemData>();

    // Fired whenever items list changes (add/remove)
    public event Action<List<RuntimeItemData>> OnInventoryChanged;

    // true if there's room for at least one more
    public bool CanAdd => items.Count < Capacity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Attempts to add an item to the first free slot.
    /// Returns true on success, false if inventory is full.
    /// </summary>
    public bool AddItem(RuntimeItemData data)
    {
        if (!CanAdd) return false;
        items.Add(data);
        OnInventoryChanged?.Invoke(GetAllItems());
        return true;
    }

    /// <summary>
    /// Removes the item at the given slot index (0–8).
    /// </summary>
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return;
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke(GetAllItems());
    }

    /// <summary>
    /// Returns a copy of the current items list (for UI iteration).
    /// </summary>
    public List<RuntimeItemData> GetAllItems()
    {
        return new List<RuntimeItemData>(items);
    }
}