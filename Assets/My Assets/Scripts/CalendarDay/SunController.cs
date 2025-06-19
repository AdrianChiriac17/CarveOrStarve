using UnityEngine;

public class SunController : MonoBehaviour
{
    [SerializeField] private Vector3 sunriseRotation = new Vector3(30, 0, 0);  // 9:00
    [SerializeField] private Vector3 sunsetRotation = new Vector3(160, 0, 0); // 17:00

    private void Start()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnTimeUpdated += UpdateSunRotation;
        else
            Debug.LogError("SunController: CalendarManager.Instance is null in Start()", this);
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
            CalendarManager.Instance.OnTimeUpdated -= UpdateSunRotation;
    }

    private void UpdateSunRotation(float timeMinutes)
    {
        float t = Mathf.InverseLerp(0f, 480f, timeMinutes); // Map 0–480 to 0–1
        transform.rotation = Quaternion.Euler(Vector3.Lerp(sunriseRotation, sunsetRotation, t));
    }
}