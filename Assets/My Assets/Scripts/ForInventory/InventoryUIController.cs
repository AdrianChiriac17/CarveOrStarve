using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("UI Setup")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private RectTransform slotsContainer;

    [Header("Placement")]
    [SerializeField] private BlockPlacementController blockPlacementController;

    [Header("Input (New System)")]
    [SerializeField] private InputActionAsset controlsAsset;
    [SerializeField] private string actionMapName = "Player";

    // UI slots
    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>(InventoryManager.Capacity);
    private int activeSlot = -1;

    // Input actions
    private InputAction[] selectActions;
    private InputAction placeAction;

    private void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // spawn UI slot prefabs
        for (int i = 0; i < InventoryManager.Capacity; i++)
        {
            var go = Instantiate(slotPrefab.gameObject, slotsContainer);
            var slot = go.GetComponent<InventorySlotUI>();
            slot.Init(i);
            slots.Add(slot);
        }

        // bind input actions
        var map = controlsAsset.FindActionMap(actionMapName, throwIfNotFound: true);

        selectActions = new InputAction[InventoryManager.Capacity];
        for (int i = 0; i < selectActions.Length; i++)
        {
            // Select1, Select2, ..., Select9
            selectActions[i] = map.FindAction($"Select{i + 1}", throwIfNotFound: true);
        }

        // previously UseItem, now PlaceBlock in input asset
        placeAction = map.FindAction("PlaceBlock", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        if (Instance != this) return;

        // subscribe to number‐key selects
        for (int i = 0; i < selectActions.Length; i++)
        {
            var act = selectActions[i];
            int idx = i;
            act.performed += _ => SelectSlot(idx);
            act.Enable();
        }

        // subscribe to place/spawn action
        placeAction.performed += _ => UseActiveSlot();
        placeAction.Enable();
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        // unsubscribe selects
        for (int i = 0; i < selectActions.Length; i++)
        {
            var act = selectActions[i];
            int idx = i;
            act.performed -= _ => SelectSlot(idx);
            act.Disable();
        }

        // unsubscribe place
        placeAction.performed -= _ => UseActiveSlot();
        placeAction.Disable();
    }

    private void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryUIController: No InventoryManager found!", this);
            enabled = false;
            return;
        }

        InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI(InventoryManager.Instance.GetAllItems());
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void SelectSlot(int index)
    {
        var items = InventoryManager.Instance.GetAllItems();
        activeSlot = (index < items.Count) ? index : -1;
        RefreshUI(items);
    }

    private void RefreshUI(List<RuntimeItemData> items)
    {
        for (int i = 0; i < InventoryManager.Capacity; i++)
        {
            RuntimeItemData data = (i < items.Count) ? items[i] : null;
            slots[i].Setup(data);
            slots[i].SetSelected(i == activeSlot);
        }
    }

    /// <summary>
    /// Called when the player presses the PlaceBlock action (e.g. right‐click).
    /// Instead of immediate spawn, enters the BlockPlacementController.
    /// </summary>
    private void UseActiveSlot()
    {
        var items = InventoryManager.Instance.GetAllItems();
        if (activeSlot < 0 || activeSlot >= items.Count) return;

        var data = items[activeSlot];

        if (blockPlacementController != null)
        {
            // Begin preview-in-hand; controller will handle final placement/removal
            blockPlacementController.StartPlacing(
                data.prefab,
                data.scales,
                data.color,
                data.colorName,     
                data.material,
                data.shape,         
                data.paidCost,     
                activeSlot
            );
        }
        else
        {
            // Fallback: immediate spawn in front of camera
            var cam = Camera.main;
            var obj = Instantiate(
                data.prefab,
                cam.transform.position + cam.transform.forward * 2f,
                Quaternion.identity
            );
            obj.transform.localScale = data.scales;

            var rend = obj.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var mat = new Material(data.material);
                mat.color = data.color;
                rend.material = mat;
            }

            InventoryManager.Instance.RemoveItemAt(activeSlot);
            activeSlot = -1;
            RefreshUI(InventoryManager.Instance.GetAllItems());
        }
    }
}