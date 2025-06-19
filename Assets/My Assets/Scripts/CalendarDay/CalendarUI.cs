using UnityEngine;
using TMPro;

public class CalendarUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    private CalendarManager cal;

    private void Start()
    {
        cal = CalendarManager.Instance;
        if (cal == null)
        {
            Debug.LogError("CalendarUI: CalendarManager.Instance is null! Did you forget to add it to the scene?");
            enabled = false;
            return;
        }

        cal.OnTimeUpdated += UpdateClock;
        cal.OnDayChanged += UpdateDayLabel;
    }

    private void OnDestroy()
    {
        if (cal != null)
        {
            cal.OnTimeUpdated -= UpdateClock;
            cal.OnDayChanged -= UpdateDayLabel;
        }
    }

    private void UpdateClock(float minutes)
    {
        int hour = 9 + (int)(minutes / 60);
        int min = (int)(minutes % 60);
        timeText.text = $"{hour:00}:{min:00}";
    }

    private void UpdateDayLabel(string dayName, int week)
    {
        dayText.text = $"Week {week} – {dayName}";
    }
}