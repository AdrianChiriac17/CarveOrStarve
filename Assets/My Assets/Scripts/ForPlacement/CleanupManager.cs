using System.Collections.Generic;
using UnityEngine;

public class CleanupManager : MonoBehaviour
{
    public static CleanupManager Instance { get; private set; }
    private readonly List<GameObject> allPlaced = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnDayChanged += OnDayChanged;
        }
        else
        {
            Debug.LogError("CleanupManager: CalendarManager.Instance is null in Start!", this);
        }
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnDayChanged -= OnDayChanged;
    }

    /// <summary>Call whenever you finalize a block.</summary>
    public void Register(GameObject block)
    {
        allPlaced.Add(block);
    }

    /// <summary>Destroy all placed blocks and clear list.</summary>
    public void ClearAll()
    {
        foreach (var b in allPlaced)
            if (b != null) Destroy(b);
        allPlaced.Clear();
    }

    /// <summary>Called by CalendarManager whenever the day changes.</summary>
    private void OnDayChanged(string dayName, int week)
    {
        if (dayName == "Monday")
            ClearAll();
    }
}