using UnityEngine;
using UnityEngine.InputSystem;

public class ShopController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject shopCanvas;    // drag your ShopCanvas root here

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;
    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("References")]
    [SerializeField] private FirstPersonController firstPerson;


    private InputAction toggleShopAction;
    private bool isOpen = false;

    private void Awake()
    {
        // grab the action from your PlayerInputHandler's asset & map
        var map = playerControls.FindActionMap(actionMapName);
        toggleShopAction = map.FindAction("ToggleShop");
    }

    private void OnEnable()
    {
        toggleShopAction.Enable();
        toggleShopAction.performed += OnToggleShop;
    }

    private void OnDisable()
    {
        toggleShopAction.performed -= OnToggleShop;
        toggleShopAction.Disable();
    }

    private void OnToggleShop(InputAction.CallbackContext ctx)
    {
        isOpen = !isOpen;
        shopCanvas.SetActive(isOpen);

        // lock or unlock the player
        firstPerson.enabled = !isOpen;

        // show/hide cursor
        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }
}