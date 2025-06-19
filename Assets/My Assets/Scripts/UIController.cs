using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("Child under your Canvas that holds the shop UI")]
    [SerializeField] private GameObject shopPanel;
    [Tooltip("Child under your Canvas that holds your main HUD/inventory")]
    [SerializeField] private GameObject mainPanel;
    [Tooltip("Child under your Canvas that holds your social media UI")]
    [SerializeField] private GameObject socialPanel;    // ← new

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset controls;
    [SerializeField] private string playerMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    [Header("Player")]
    [SerializeField] private FirstPersonController firstPerson;

    private InputActionMap playerMap;
    private InputActionMap uiMap;
    private InputAction toggleShop;
    private InputAction toggleSocial;           // ← new

    private bool shopOpen;
    private bool socialOpen;                       // ← new

    private void Awake()
    {
        Instance = this;

        // initial UI state
        shopPanel.SetActive(false);
        socialPanel.SetActive(false);  // ← new
        mainPanel.SetActive(true);
        shopOpen = false;
        socialOpen = false;            // ← new

        // player inputs always on
        playerMap = controls.FindActionMap(playerMapName, true);
        playerMap.Enable();

        // UI inputs only when shop OR social open
        uiMap = controls.FindActionMap(uiMapName, true);
        uiMap.Disable();

        // grab both toggles from the Player map
        toggleShop = playerMap.FindAction("ToggleShop", true);
        toggleSocial = playerMap.FindAction("ToggleSocial", true);

        // ensure cursor/movement reset
        firstPerson.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        toggleShop.performed += OnToggleShop;
        toggleSocial.performed += OnToggleSocial;       // ← new
    }

    private void OnDisable()
    {
        toggleShop.performed -= OnToggleShop;
        toggleSocial.performed -= OnToggleSocial;       // ← new
    }

    private void OnToggleShop(InputAction.CallbackContext _)
    {
        shopOpen = !shopOpen;

        // opening shop: force-close social
        if (shopOpen && socialOpen)
            CloseSocial();

        // panels
        shopPanel.SetActive(shopOpen);
        mainPanel.SetActive(!(shopOpen || socialOpen));
        // social stays closed if shop opens
        socialPanel.SetActive(false);

        // swap UI map
        if (shopOpen) uiMap.Enable(); else uiMap.Disable();

        // player movement
        firstPerson.enabled = !shopOpen && !socialOpen;

        // cursor
        if (shopOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!socialOpen) // only re-lock when neither is open
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnToggleSocial(InputAction.CallbackContext _)
    {
        socialOpen = !socialOpen;

        // opening social: force-close shop
        if (socialOpen && shopOpen)
            CloseShop();

        // panels
        socialPanel.SetActive(socialOpen);
        mainPanel.SetActive(!(socialOpen || shopOpen));
        shopPanel.SetActive(false);

        // swap UI map
        if (socialOpen) uiMap.Enable(); else uiMap.Disable();

        // player movement
        firstPerson.enabled = !socialOpen && !shopOpen;

        // cursor
        if (socialOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!shopOpen) // only re-lock when neither is open
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseShop()
    {
        if (!shopOpen) return;
        shopOpen = false;

        var shop = shopPanel.GetComponent<ShopUI>();
        if (shop != null) shop.HideRightPanel();

        shopPanel.SetActive(false);
        mainPanel.SetActive(!socialOpen);
        uiMap.Disable();
        playerMap.Enable();

        firstPerson.enabled = !socialOpen;
        Cursor.lockState = socialOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = socialOpen ? true : false;
    }

    private void CloseSocial()
    {
        if (!socialOpen) return;
        socialOpen = false;

        socialPanel.SetActive(false);
        mainPanel.SetActive(!shopOpen);
        uiMap.Disable();
        playerMap.Enable();

        firstPerson.enabled = !shopOpen;
        Cursor.lockState = shopOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = shopOpen ? true : false;
    }
}
