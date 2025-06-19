using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CalendarManager : MonoBehaviour
{
    public static CalendarManager Instance { get; private set; }

    public int currentWeek = 1;
    public int currentDayIndex = 0; // 0=Monday ... 4=Friday
    public float currentTimeMinutes = 0f;


    [Header("Input (New System)")]
    [SerializeField] private InputActionAsset controls;   // assign your .inputactions here
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string skipDayActionName = "SkipDay";
    private InputAction skipDayAction;

    private readonly string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

    public event Action<string, int> OnDayChanged;
    public event Action<float> OnTimeUpdated;
    public event Action OnWorkdayEnded;
    public event Action OnFridayPrompt;
    public event Action OnGameFinished;

    public bool IsFriday => currentDayIndex == 4;
    public bool IsLastWeek => currentWeek == 3;
    public float RealSecondsPerGameMinute = 1f;

    private bool isTimeRunning = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- wire up input ---
        var map = controls.FindActionMap(actionMapName, throwIfNotFound: true);
        skipDayAction = map.FindAction(skipDayActionName, throwIfNotFound: true);
        skipDayAction.performed += OnSkipDay;
        skipDayAction.Enable();
    }

    private void Start()
    {
        StartNewDay();
    }

    private void Update()
    {
        if (!isTimeRunning) return;

        currentTimeMinutes += Time.deltaTime / RealSecondsPerGameMinute;
        OnTimeUpdated?.Invoke(currentTimeMinutes);

        if (currentTimeMinutes >= 480f) // 9:00 + 480min = 17:00
        {
            EndDay();
        }


    }

    private void EndDay()
    {
        isTimeRunning = false;
       
        if (IsFriday)
            OnFridayPrompt?.Invoke();
        else
            OnWorkdayEnded?.Invoke();

    }

    public void AdvanceToNextDay()
    {
        currentDayIndex++;
        if (currentDayIndex > 4)
        {
            if (IsLastWeek)
            {
                OnGameFinished?.Invoke();
                return;
            }

            currentDayIndex = 0;
            currentWeek++;
        }

        StartNewDay();
    }

    private void StartNewDay()
    {
        currentTimeMinutes = 0f;
        isTimeRunning = true;
        OnDayChanged?.Invoke(days[currentDayIndex], currentWeek);
    }

    private void OnDestroy()
    {
        if (skipDayAction != null)
        {
            skipDayAction.performed -= OnSkipDay;
            skipDayAction.Disable();
        }
    }

    private void OnSkipDay(InputAction.CallbackContext ctx)
    {
        // only allow skip if day isn’t already ending
        if (isTimeRunning)
            EndDay();
    }
}
