// ShopUI.cs
using UnityEngine;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Data & Prefabs")]
    [Tooltip("All your ShopItemData assets here")]
    [SerializeField] private List<ShopItemData> shopItems;

    [Tooltip("The ItemInfoPrefab you just made")]
    [SerializeField] private GameObject itemInfoPrefab;

    [Tooltip("Content Transform under your Scroll View")]
    [SerializeField] private Transform itemListContent;

    [Header("Preview Setup")]
    [Tooltip("Empty Transform in front of your preview camera")]
    [SerializeField] private Transform previewSpawnPoint;

    private GameObject currentPreview;


    private void OnEnable()
    {
        if (shopItems == null || itemInfoPrefab == null || itemListContent == null)
        {
            Debug.LogError("[ShopUI] Missing references, cannot open shop");
            return;
        }
        //PopulateItemList();
    }

    private void PopulateItemList()
    { 

        // clear any existing
        foreach (Transform c in itemListContent)
            Destroy(c.gameObject);

        // fill in one row per ShopItemData
        foreach (var item in shopItems)
        {
            var row = Instantiate(itemInfoPrefab, itemListContent);
            row.GetComponent<ItemInfoUI>().Init(item, OnItemSelected);
        }
    }

    private void OnItemSelected(ShopItemData data)
    {
        Debug.Log($"Spawning preview of {data.itemName}");

        // destroy old preview
        if (currentPreview != null)
            Destroy(currentPreview);

        // spawn new preview
        currentPreview = Instantiate(
            data.prefab,
            previewSpawnPoint.position,
            Quaternion.identity,
            previewSpawnPoint
        );

        // apply defaults
        currentPreview.transform.localScale = data.defaultScale;
        var rend = currentPreview.GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = data.defaultColor;
    }
}
