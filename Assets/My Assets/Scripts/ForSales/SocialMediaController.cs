using UnityEngine;
using UnityEngine.InputSystem;

public class SocialMediaController : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject socialPanel;

    [Header("Input Asset")]
    [SerializeField] private InputActionAsset controls;

    [Header("Action Map Names")]
    [SerializeField] private string playerMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    [Header("Player Controller")]
    [SerializeField] private FirstPersonController firstPerson;

    private InputActionMap playerMap;
    private InputActionMap uiMap;
    private InputAction toggleSocial;
    private bool socialOpen;

    private void Awake()
    {
        // 1) grab your maps
        playerMap = controls.FindActionMap(playerMapName, true);
        uiMap = controls.FindActionMap(uiMapName, true);

        // 2) grab the global action (in ANY map)
        toggleSocial = controls.FindAction("ToggleSocial", true);
        toggleSocial.performed += OnToggleSocial;
        toggleSocial.Enable();  // always listening

        // 3) start closed
        socialPanel.SetActive(false);

        Debug.Log("[Social] Awake complete, ToggleSocial listening");
    }

    private void OnDestroy()
    {
        toggleSocial.performed -= OnToggleSocial;
    }

    private void OnToggleSocial(InputAction.CallbackContext ctx)
    {
        Debug.Log("[Social] ToggleSocial triggered");

        socialOpen = !socialOpen;
        Debug.Log($"[Social] socialOpen = {socialOpen}");

        if (socialOpen)
        {
            // close shop if needed
            UIController.Instance?.CloseShop();

            socialPanel.SetActive(true);

            // swap maps
            playerMap.Disable();
            uiMap.Enable();

            // block movement & look
            firstPerson.CanMove = false;
            firstPerson.CanLook = false;

            // show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            socialPanel.SetActive(false);

            // restore maps
            uiMap.Disable();
            playerMap.Enable();

            // restore movement & look
            firstPerson.CanMove = true;
            firstPerson.CanLook = true;

            // hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}