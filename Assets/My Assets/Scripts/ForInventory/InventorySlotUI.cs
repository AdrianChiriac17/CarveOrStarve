using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Image that shows the item thumbnail")]
    [SerializeField] private Image iconImage;
    [Tooltip("The Image used to show selection/highlight (whole border)")]
    [SerializeField] private Image highlightImage;
    [Tooltip("The TMP component showing 1–9 key labels")]
    [SerializeField] private TMP_Text keyLabel;

    private RuntimeItemData itemData;

    private void Awake()
    {
        // sanity checks
        if (iconImage == null) Debug.LogError($"{name}: iconImage not assigned!", this);
        if (highlightImage == null) Debug.LogError($"{name}: highlightImage not assigned!", this);
        if (keyLabel == null) Debug.LogError($"{name}: keyLabel not assigned!", this);
    }

    /// <summary>
    /// Called once by InventoryUIController after Instantiate.
    /// </summary>
    public void Init(int index)
    {
        keyLabel.text = (index + 1).ToString();

        // hide both icon & highlight until Setup is called
        iconImage.sprite = null;
        iconImage.enabled = false;
        highlightImage.gameObject.SetActive(false);

        // ensure we start empty
        Setup(null);
    }

    /// <summary>
    /// Paints this slot with the given data (or clears it).
    /// </summary>
    public void Setup(RuntimeItemData data)
    {
        itemData = data;

        if (data != null && data.thumbnail != null)
        {
            iconImage.sprite = data.thumbnail;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    /// <summary>
    /// Toggles the highlight border on/off.
    /// </summary>
    public void SetSelected(bool selected)
    {
        highlightImage.gameObject.SetActive(selected);
        Debug.Log($"{name}.SetSelected({selected})");
    }

    /// <summary>
    /// True if there is no item in this slot.
    /// </summary>
    public bool IsEmpty => itemData == null;
}