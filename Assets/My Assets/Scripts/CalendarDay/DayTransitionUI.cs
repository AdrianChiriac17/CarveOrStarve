using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DayTransitionUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup fadeGroup;     // FadePanel’s CanvasGroup
    [SerializeField] private GameObject fridayPrompt;   // your Week-End prompt panel
    [SerializeField] private GameObject gameFinished;   // your Win screen

    [Header("Input System")]
    [SerializeField] private InputActionAsset controls;
    [SerializeField] private string playerMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    private CalendarManager cal;
    private InputActionMap playerMap;
    private InputActionMap uiMap;


    [Header("Other UI to Disable")]
    [SerializeField] private GameObject mainCanvas;
    private void Start()
    {
        // get Calendar
        cal = CalendarManager.Instance;
        if (cal == null) throw new Exception("CalendarManager missing");

        // subscribe to day events
        cal.OnWorkdayEnded += OnDayEnded;
        cal.OnFridayPrompt += OnFridayEnded;
        cal.OnGameFinished += OnGameFinished;

        // cache input maps
        playerMap = controls.FindActionMap(playerMapName, true);
        uiMap = controls.FindActionMap(uiMapName, true);

        // ensure UI map is off at start
        uiMap.Disable();
    }

    private void OnDestroy()
    {
        if (cal != null)
        {
            cal.OnWorkdayEnded -= OnDayEnded;
            cal.OnFridayPrompt -= OnFridayEnded;
            cal.OnGameFinished -= OnGameFinished;
        }
    }

    private void OnDayEnded()
    {
        // normal day end: fade out → advance → fade in
        StartCoroutine(DoDaySkip());
    }

    private void OnFridayEnded()
    {
        // Friday: fade out → switch to UI → show prompt
        StartCoroutine(DoFridaySkip());
    }

    private void OnGameFinished()
    {
        // last possible Friday: fade out → switch to UI → show final screen
        StartCoroutine(DoGameFinished());
    }

    private IEnumerator DoDaySkip()
    {
        yield return Fade(0f, 1f, 1.2f);
        cal.AdvanceToNextDay();
        yield return new WaitForSeconds(0.2f);
        yield return Fade(1f, 0f, 1.2f);
    }

    private IEnumerator DoFridaySkip()
    {
        yield return Fade(0f, 1f, 1.2f);

        // hide all the other UI so clicks go straight to our prompt
        if (mainCanvas != null)
            mainCanvas.SetActive(false);

        // 1) swap input so UI clicks work
        playerMap.Disable();
        uiMap.Enable();

        // 2) Unlock and show the OS cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        fridayPrompt.SetActive(true);
    }

    private IEnumerator DoGameFinished()
    {
        yield return Fade(0f, 1f, 1.2f);

        playerMap.Disable();
        uiMap.Enable();

        gameFinished.SetActive(true);
    }

    // wired to ContinueButton.OnClick()
    public void OnContinuePressed()
    {
        // 0)hide prompt
        fridayPrompt.SetActive(false);

        //0.5) bring back the main UI
        if (mainCanvas != null)
            mainCanvas.SetActive(true);



        // 1) Switch input maps back
        uiMap.Disable();
        playerMap.Enable();

        // 2) Relock and hide the OS cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // advance and fade back in
        cal.AdvanceToNextDay();
        StartCoroutine(Fade(1f, 0f, 1f));
    }

    // generic fade: from 0→1 or 1→0 over duration seconds
    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        fadeGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeGroup.alpha = to;
    }
}