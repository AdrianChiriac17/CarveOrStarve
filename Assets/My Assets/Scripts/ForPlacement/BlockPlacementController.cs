using UnityEngine;
using UnityEngine.InputSystem;

public class BlockPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private GameObject sculptingZoneGameObject; //Needed for zone bounds check

    [Header("Placement Settings")]
    [SerializeField] private float moveSpeed = 2f;   // units per second
    [SerializeField] private float rotateSpeed = 90f;  // degrees per second
    [SerializeField] private float minDistance = 1f;   // closest to camera
    [SerializeField] private float maxDistance = 5f;   // farthest from camera

    [Header("Input System")]
    [SerializeField] private InputActionAsset controls;
    [SerializeField] private string actionMapName = "Player";

    // Input actions
    private InputAction middleClick, rotateX, rotateY, rotateZ;
    private InputAction moveForward, moveBack, moveLeft, moveRight, moveUp, moveDown;
    private InputAction placeBlock, cancelPlace;

    // State
    private enum State { Idle, Previewing, Placing }
    private State state = State.Idle;

    private GameObject preview;
    private int currentSlotIndex;
    private Material[] originalMaterials;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Placement data
    private Vector3 _placingScales;
    private Color _placingColor;
    private Material _placingMaterial;
    private int _placingSlotIndex;
    private string _placingColorName;
    private BlockData.Shape _placingShape;
    private float _placingPaidCost;


    private Collider _zoneCollider;
    private void Awake()
    {
        var map = controls.FindActionMap(actionMapName, true);

        middleClick = map.FindAction("MiddleClick", true);
        rotateX = map.FindAction("RotateX", true);
        rotateY = map.FindAction("RotateY", true);
        rotateZ = map.FindAction("RotateZ", true);
        moveForward = map.FindAction("MoveForward", true);
        moveBack = map.FindAction("MoveBack", true);
        moveLeft = map.FindAction("MoveLeft", true);
        moveRight = map.FindAction("MoveRight", true);
        moveUp = map.FindAction("MoveUp", true);
        moveDown = map.FindAction("MoveDown", true);
        placeBlock = map.FindAction("PlaceBlock", true);
        cancelPlace = map.FindAction("CancelPlace", true);

        middleClick.performed += _ =>
        {
            if (state == State.Previewing)
                EnterPlacementMode();
        };
        placeBlock.performed += _ =>
        {
            if (state == State.Placing)
                ExitPlacementMode(place: true);
        };
        cancelPlace.performed += _ =>
        {
            if (state == State.Placing)
                ExitPlacementMode(place: false);
        };
    }

    private void Start()
    {
        if (holdPoint == null)
            Debug.LogError("BlockPlacementController: holdPoint not assigned!", this);
        if (firstPersonController == null)
            Debug.LogError("BlockPlacementController: FirstPersonController not assigned!", this);

        _zoneCollider = sculptingZoneGameObject.GetComponent<Collider>();
        if (_zoneCollider == null)
            Debug.LogError("SculptingZone GameObject needs a Collider!", this);
    }

    private void OnEnable() => controls.FindActionMap(actionMapName, true).Enable();
    private void OnDisable() => controls.FindActionMap(actionMapName, true).Disable();

    private void Update()
    {
        if (state != State.Placing || preview == null) return;

        // Rotation
        if (rotateX.ReadValue<float>() > 0) preview.transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime, Space.Self);
        if (rotateY.ReadValue<float>() > 0) preview.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        if (rotateZ.ReadValue<float>() > 0) preview.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime, Space.Self);

        // Movement
        Vector3 offset = Vector3.zero;
        if (moveForward.ReadValue<float>() > 0) offset += Vector3.forward;
        if (moveBack.ReadValue<float>() > 0) offset += Vector3.back;
        if (moveLeft.ReadValue<float>() > 0) offset += Vector3.left;
        if (moveRight.ReadValue<float>() > 0) offset += Vector3.right;
        if (moveUp.ReadValue<float>() > 0) offset += Vector3.up;
        if (moveDown.ReadValue<float>() > 0) offset += Vector3.down;

        preview.transform.localPosition += offset * moveSpeed * Time.deltaTime;
        preview.transform.localPosition = ClampLocalPosition(preview.transform.localPosition);
    }

    public void StartPlacing(
        GameObject prefab,
        Vector3 scales,
        Color color,
        string colorName,
        Material material,
        BlockData.Shape shape,
        float paidCost,
        int slotIndex
    )
    {
        if (preview != null)
            Destroy(preview);

        _placingScales = scales;
        _placingColor = color;
        _placingColorName = colorName;
        _placingMaterial = material;
        _placingShape = shape;
        _placingPaidCost = paidCost;
        _placingSlotIndex = slotIndex;

        preview = Instantiate(prefab, holdPoint);
        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = scales;

        foreach (var r in preview.GetComponentsInChildren<Renderer>())
        {
            var ghostMat = new Material(material);
            ghostMat.color = new Color(color.r, color.g, color.b, 0.5f);
            r.material = ghostMat;
        }

        state = State.Previewing;
    }

    private void EnterPlacementMode()
    {
        state = State.Placing;

        firstPersonController.CanMove = false;
        firstPersonController.CanLook = true;

        var rends = preview.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[rends.Length];

        for (int i = 0; i < rends.Length; i++)
        {
            originalMaterials[i] = rends[i].material;
            var ghost = new Material(originalMaterials[i]);
            Color c = ghost.color;
            c.a = 0.5f;
            ghost.color = c;
            rends[i].material = ghost;
        }

        originalPosition = preview.transform.localPosition;
        originalRotation = preview.transform.localRotation;
    }

    private void ExitPlacementMode(bool place)
    {
        firstPersonController.CanMove = true;
        firstPersonController.CanLook = true;

        if (place)
        {
            // 1. Instantiate real block
            var placed = Instantiate(preview, preview.transform.position, preview.transform.rotation);
            placed.transform.localScale = _placingScales;

            // 2. Apply opaque material
            foreach (var r in placed.GetComponentsInChildren<Renderer>())
            {
                var realMat = new Material(_placingMaterial);
                realMat.color = _placingColor;
                r.material = realMat;
            }

            // 3. Attach and configure BlockData
            var bd = placed.AddComponent<BlockData>();
            bd.shape = _placingShape;
            bd.colorName = _placingColorName;
            bd.volume = ComputeVolume(_placingShape, _placingScales); // custom volume
            bd.paidCost = _placingPaidCost;

            // 4. Debug log the final placed block
            Debug.Log($"Placed block: {bd.colorName} {bd.shape} {bd.volume}");

            // MOST IMPORTANT CHECK IF IT S IN THE SCULPTURE ZONE
            var rend = placed.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var blockBounds = rend.bounds;
                var zoneBounds = _zoneCollider.bounds;

                if (zoneBounds.Intersects(blockBounds))
                {
                    Debug.Log($"[SculptZone] Registered {bd.colorName} {bd.shape}");
                    SculptureManager.Instance.RegisterBlock(bd);
                }
                else
                {
                    Debug.Log($"[SculptZone] Skipped {bd.colorName} {bd.shape} (out of zone)");
                }
            }
            else
            {
                Debug.LogWarning("No Renderer on placed block; cannot test sculpt zone.");
            }



            // 5. Register in cleanup
            CleanupManager.Instance.Register(placed);

            // 6. Remove from inventory
            InventoryManager.Instance.RemoveItemAt(_placingSlotIndex);

            // 7. Clear preview and state
            Destroy(preview);
            state = State.Idle;
        }
        else
        {
            // Cancel placement
            preview.transform.localPosition = originalPosition;
            preview.transform.localRotation = originalRotation;
            preview.transform.localScale = _placingScales;

            foreach (var r in preview.GetComponentsInChildren<Renderer>())
            {
                r.material = new Material(_placingMaterial) { color = _placingColor };
            }

            state = State.Previewing;
        }
    }

    private Vector3 ClampLocalPosition(Vector3 localPos)
    {
        Vector3 dir = localPos.normalized;
        float dist = Mathf.Clamp(localPos.magnitude, minDistance, maxDistance);
        return dir * dist;
    }

    private float ComputeVolume(BlockData.Shape shape, Vector3 scale)
    {
        switch (shape)
        {
            case BlockData.Shape.Cube:
            case BlockData.Shape.Prism:
                // Base × Height × Depth
                return scale.x * scale.y * scale.z;

            case BlockData.Shape.Sphere:
                // (4/3)πr³ where r is average of half-extents
                float rSphere = (scale.x + scale.y + scale.z) / 6f;
                return (4f / 3f) * Mathf.PI * Mathf.Pow(rSphere, 3);

            case BlockData.Shape.Cylinder:
                // πr²h; assume vertical cylinder (height = Y)
                float rCyl = (scale.x + scale.z) / 4f;
                return Mathf.PI * Mathf.Pow(rCyl, 2) * scale.y;

            case BlockData.Shape.Capsule:
                // Volume = cylinder + 2 hemispheres
                float rCap = (scale.x + scale.z) / 4f;
                float hCap = Mathf.Max(0f, scale.y - 2f * rCap); // cylinder height
                float cylVolume = Mathf.PI * Mathf.Pow(rCap, 2) * hCap;
                float sphereVolume = (4f / 3f) * Mathf.PI * Mathf.Pow(rCap, 3);
                return cylVolume + sphereVolume;

            default:
                return scale.x * scale.y * scale.z;
        }
    }
}