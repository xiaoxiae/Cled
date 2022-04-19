using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public enum PausedState
{
    Unpaused,
    Regular,
    Popup,
    HoldPicker,
}

/// <summary>
/// A manager that handles pausing due to various reasons.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public PausedState State { get; set; }

    private UIDocument _root;

    public void Start()
    {
        _root = GetComponent<UIDocument>();

        Unpause();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == PausedState.Regular)
                Unpause();
            else if (State == PausedState.Unpaused)
                RegularPause();
        }
    }

    /// <summary>
    /// Regular pause.
    /// </summary>
    public void RegularPause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        _root.enabled = true;

        State = PausedState.Regular;

        foreach (var hook in _pauseHooks)
            hook();
    }

    /// <summary>
    /// Pause due to a popup.
    /// </summary>
    public void PopupPause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        _root.enabled = true;

        State = PausedState.Popup;

        foreach (var hook in _pauseHooks)
            hook();
    }

    /// <summary>
    /// Pause due to the hold picker menu.
    /// </summary>
    public void HoldPickerPause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        _root.enabled = true;

        State = PausedState.HoldPicker;

        foreach (var hook in _pauseHooks)
            hook();
    }

    /// <summary>
    /// Continue (unpause).
    /// </summary>
    public void Unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        _root.enabled = false;

        State = PausedState.Unpaused;

        foreach (var hook in _unpauseHooks)
            hook();
    }

    private List<Action> _pauseHooks = new();
    private List<Action> _unpauseHooks = new();

    /// <summary>
    /// Add a hook function that gets called every time the editor is paused.
    /// </summary>
    public void AddPausedHook(Action hook) => _pauseHooks.Add(hook);

    /// <summary>
    /// Add a hook function that gets called every time the editor is unpaused.
    /// </summary>
    public void AddUnpausedHook(Action hook) => _unpauseHooks.Add(hook);
}