using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Data & Prefabs")]
    [SerializeField] private List<ShopItemData> shopItems;
    [SerializeField] private GameObject itemInfoPrefab;
    [SerializeField] private Transform itemListContent;

    [Header("Preview Setup")]
    [SerializeField] private Transform previewSpawnPoint;
    [SerializeField] private Camera previewCamera;
    [SerializeField] private RenderTexture previewRenderTexture;
    [SerializeField] private RawImage previewRawImage;
    [SerializeField] private int thumbnailSize = 256;

    [Header("Right Panel & Controls")]
    [SerializeField] private GameObject rightPanel;
    [SerializeField] private Toggle checkerToggle;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text finalPriceText;

    [Header("Customization UI")]
    [SerializeField] private GameObject sliderRowPrefab;
    [SerializeField] private Transform sliderContainer;
    [SerializeField] private GameObject colorSwatchPrefab;
    [SerializeField] private Transform colorContainer;

    // runtime state
    private GameObject currentPreview;
    private ShopItemData currentSelectedData;
    private Color currentColor;
    private float[] currentScales = new float[3];
    private int displayPrice;

    private void OnEnable()
    {
        if (previewCamera != null)
        {
            previewCamera.enabled = true;
            previewCamera.gameObject.SetActive(true);
            previewCamera.targetTexture = previewRenderTexture;
        }

        if (previewRawImage != null)
        {
            previewRawImage.texture = previewRenderTexture;
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        rightPanel.SetActive(false);

        if (shopItems == null || shopItems.Count == 0 || itemInfoPrefab == null || itemListContent == null)
            return;

        PopulateItemList();

        checkerToggle.onValueChanged.RemoveAllListeners();
        checkerToggle.onValueChanged.AddListener(OnCheckerToggled);
        checkerToggle.isOn = true;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyPressed);
    }

    private void PopulateItemList()
    {
        for (int i = itemListContent.childCount - 1; i >= 0; i--)
            Destroy(itemListContent.GetChild(i).gameObject);

        foreach (var item in shopItems)
        {
            var rowGO = Instantiate(itemInfoPrefab, itemListContent);
            var infoUI = rowGO.GetComponent<ItemInfoUI>();
            infoUI?.Init(item, OnItemSelected);
        }
    }

    private void OnItemSelected(ShopItemData data)
    {
        currentSelectedData = data;
        checkerToggle.isOn = true;

        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = Instantiate(
            data.prefab,
            previewSpawnPoint.position,
            Quaternion.identity,
            previewSpawnPoint
        );

        SetLayerRecursively(currentPreview, LayerMask.NameToLayer("Preview"));
        currentPreview.transform.localScale = data.defaultScale;

        var rend = currentPreview.GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = data.defaultColor;

        if (currentPreview.GetComponent<PreviewRotator>() == null)
            currentPreview.AddComponent<PreviewRotator>();

        foreach (Transform t in sliderContainer) Destroy(t.gameObject);
        foreach (Transform t in colorContainer) Destroy(t.gameObject);

        currentScales[0] = data.defaultScale.x;
        currentScales[1] = data.defaultScale.y;
        currentScales[2] = data.defaultScale.z;
        currentColor = data.defaultColor;

        SpawnSliders(data);
        SpawnColorSwatches(data);
        RecalculatePrice(data);

        rightPanel.SetActive(true);
    }

    private void SpawnSliders(ShopItemData data)
    {
        string[] axisNames = { "Width", "Height", "Depth" };

        for (int i = 0; i < 3; i++)
        {
            var row = Instantiate(sliderRowPrefab, sliderContainer);
            var label = row.transform.Find("Label").GetComponent<TMP_Text>();
            var slider = row.transform.Find("ValueSlider").GetComponent<Slider>();
            var valueText = row.transform.Find("ValueText").GetComponent<TMP_Text>();

            label.text = axisNames[i];
            slider.minValue = data.uniformScaling ? data.uniformRange.x : data.minScale[i];
            slider.maxValue = data.uniformScaling ? data.uniformRange.y : data.maxScale[i];
            slider.value = currentScales[i];
            valueText.text = slider.value.ToString("0.00");

            int idx = i;
            slider.onValueChanged.AddListener(v =>
            {
                currentScales[idx] = v;
                currentPreview.transform.localScale = new Vector3(currentScales[0], currentScales[1], currentScales[2]);
                RecalculatePrice(data);
                valueText.text = v.ToString("0.00");
            });
        }
    }

    private void SpawnColorSwatches(ShopItemData data)
    {
        foreach (var opt in data.palette.options)
        {
            var sw = Instantiate(colorSwatchPrefab, colorContainer);
            var img = sw.GetComponent<Image>();
            var btn = sw.GetComponent<Button>();

            img.color = opt.color;
            btn.onClick.AddListener(() =>
            {
                currentColor = opt.color;
                var rend = currentPreview.GetComponentInChildren<Renderer>();
                if (rend != null)
                    rend.material.color = currentColor;

                RecalculatePrice(data);
            });
        }
    }

    private void RecalculatePrice(ShopItemData data)
    {
        float volume = data.baseVolume * currentScales[0] * currentScales[1] * currentScales[2];

        float mul = 1f;
        foreach (var opt in data.palette.options)
            if (opt.color == currentColor)
                mul = opt.multiplier;

        float raw = volume * data.costPerUnitVolume * mul;
        displayPrice = Mathf.Max(1, Mathf.RoundToInt(raw) + 1);
        finalPriceText.text = $"Buy for ${displayPrice}";

        buyButton.interactable =
            MoneyManager.Instance.TrySpend(0) &&
            MoneyManager.Instance.Balance >= displayPrice &&
            InventoryManager.Instance.CanAdd;
    }

    private void OnCheckerToggled(bool isOn)
    {
        if (currentPreview == null) return;

        var rend = currentPreview.GetComponentInChildren<Renderer>();
        if (rend == null) return;

        var srcMat = isOn ? currentSelectedData.checkerboardMaterial : currentSelectedData.defaultMaterial;
        var m = new Material(srcMat) { color = currentColor };
        rend.material = m;
    }

    private void OnBuyPressed()
    {
        var thumb = ScreenshotUtil.CaptureCameraView(previewCamera, thumbnailSize);

        var item = new RuntimeItemData
        {
            prefab = currentSelectedData.prefab,
            thumbnail = thumb,
            scales = new Vector3(currentScales[0], currentScales[1], currentScales[2]),
            color = currentColor,
            cost = displayPrice,
            material = currentPreview.GetComponentInChildren<Renderer>().material,

            // new fields
            shape = currentSelectedData.prefab.GetComponent<BlockData>().shape, // shape baked into prefab
            colorName = GetColorName(currentColor, currentSelectedData.palette),
            paidCost = displayPrice
        };

        if (MoneyManager.Instance.TrySpend(displayPrice) &&
            InventoryManager.Instance.AddItem(item))
        {
            if (previewCamera != null)
            {
                previewCamera.enabled = false;
                previewCamera.targetTexture = null;
                previewCamera.gameObject.SetActive(false);
            }

            if (previewRawImage != null)
                previewRawImage.texture = null;

            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }

            UIController.Instance.CloseShop();
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform c in obj.transform)
            SetLayerRecursively(c.gameObject, layer);
    }

    public void HideRightPanel()
    {
        if (rightPanel != null)
            rightPanel.SetActive(false);
    }

    private string GetColorName(Color col, ColorPalette palette)
    {
        foreach (var opt in palette.options)
            if (opt.color == col)
                return opt.name;
        return "Unknown";
    }
}

