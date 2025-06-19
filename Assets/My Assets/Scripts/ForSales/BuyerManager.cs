using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public enum WealthLevel { Dollar, DoubleDollar, TripleDollar }
public enum SculptureShape
{
    Cube,
    Sphere,
    Cylinder,
    Capsule,
    Prism
}

[Serializable]
public class Buyer
{
    public string FullName;
    public Sprite Avatar;
    public SculptureShape FavoriteShape;
    public Color FavoriteColor;
    public WealthLevel Wealth;
    public float Multiplier;
}

public class BuyerManager : MonoBehaviour
{
    public static BuyerManager Instance { get; private set; }

    [Header("Data Sources")]
    [Tooltip("Put FirstNames.txt & LastNames.txt into Assets/Resources/")]
    [SerializeField] private string firstNamesResource = "FirstNames";
    [SerializeField] private string lastNamesResource = "LastNames";

    [Header("Avatar Capture")]
    [Tooltip("A little camera pointing at your Human prefab")]
    [SerializeField] private GameObject humanPrefab;
    [SerializeField] private Camera avatarCamera;
    [SerializeField] private RenderTexture avatarRT;

    [Header("UI Setup")]
    [Tooltip("Prefab must have a BuyerSlotUI component")]
    [SerializeField] private GameObject buyerSlotPrefab;
    [SerializeField] private Transform gridContainer; // your 3×3 GridLayoutGroup

    [Header("Preference Options")]
    [SerializeField] private ColorPalette globalColorPalette;

    private string[] firstNames, lastNames;
    private readonly List<Buyer> currentBuyers = new List<Buyer>();

    private void Awake()
    {
        // singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // load name lists
        var fnAsset = Resources.Load<TextAsset>(firstNamesResource);
        var lnAsset = Resources.Load<TextAsset>(lastNamesResource);
        if (fnAsset == null || lnAsset == null)
            Debug.LogError("BuyerManager: Could not load name lists from Resources!");
        firstNames = fnAsset.text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lastNames = lnAsset.text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private void Start()
    {
        CalendarManager.Instance.OnDayChanged += OnDayChanged;
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void OnDayChanged(string dayName, int week)
    {
        if (dayName == "Monday")
            GenerateBuyersForWeek(week);
    }

    private void GenerateBuyersForWeek(int week)
    {
        // … clear old UI …
        foreach (Transform t in gridContainer) Destroy(t.gameObject);
        currentBuyers.Clear();


        var allowed = new List<WealthLevel> { WealthLevel.Dollar };
        if (week >= 2) allowed.Add(WealthLevel.DoubleDollar);
        if (week >= 3) allowed.Add(WealthLevel.TripleDollar);

        for (int i = 0; i < 9; i++)
        {
            // random shape
            // pick a random SculptureShape
            var shapes = Enum.GetValues(typeof(SculptureShape));
            SculptureShape favShape = (SculptureShape)
                shapes.GetValue(UnityEngine.Random.Range(0, shapes.Length));

            // random preference color
            var colors = globalColorPalette.options;
            int prefColorIdx = UnityEngine.Random.Range(0, colors.Length);
            Color favColor = colors[prefColorIdx].color;

            // random avatar tint (can be same or different)
            int pfpColorIdx = UnityEngine.Random.Range(0, colors.Length);
            Color avatarTint = colors[pfpColorIdx].color;

            // random wealth & multiplier...
            WealthLevel w = allowed[UnityEngine.Random.Range(0, allowed.Count)];
            float mult = w switch
            {
                WealthLevel.Dollar => UnityEngine.Random.Range(1.5f, 2.6f),
                WealthLevel.DoubleDollar => UnityEngine.Random.Range(2.7f, 4f),
                WealthLevel.TripleDollar => UnityEngine.Random.Range(3.5f, 7f),
                _ => 1f
            };

            var buyer = new Buyer
            {
                FullName = $"{GetRandom(firstNames)} {GetRandom(lastNames)}",
                Wealth = w,
                FavoriteShape = favShape,
                FavoriteColor = favColor,
                Multiplier = mult,
                Avatar = CaptureAvatar(avatarTint)
            };

            currentBuyers.Add(buyer);

            // instantiate & setup UI
            var go = Instantiate(buyerSlotPrefab, gridContainer);
            go.GetComponent<BuyerSlotUI>().Setup(buyer);
        }
    }

    private static T GetRandom<T>(T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    private Color PickRandomColor()
    {
        var opts = globalColorPalette.options;
        int idx = UnityEngine.Random.Range(0, opts.Length);
        return opts[idx].color;
    }

    private Sprite CaptureAvatar(Color tint)
    {
        // 1) instantiate & tint
        var h = Instantiate(humanPrefab);
        foreach (var r in h.GetComponentsInChildren<Renderer>())
            if (r.material != null) r.material.color = tint;

        // 2) render
        avatarCamera.targetTexture = avatarRT;
        avatarCamera.Render();
        RenderTexture.active = avatarRT;

        var tex = new Texture2D(avatarRT.width, avatarRT.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, avatarRT.width, avatarRT.height), 0, 0);
        tex.Apply();

        // 3) cleanup
        avatarCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(h);

        // 4) return sprite
        return Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
    }
}
