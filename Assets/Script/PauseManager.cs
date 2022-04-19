using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public enum PauseType
{
    Unpaused,
    Normal,
    Popup,
    HoldPicker,
}

/// <summary>
/// A manager that handles pausing due to various reasons.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public PauseType State { get; set; }

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
            if (State == PauseType.Normal)
                Unpause();
            else if (State == PauseType.Unpaused)
                NormalPause();
        }
    }

    /// <summary>
    /// Normal pause.
    /// </summary>
    public void NormalPause()
    {
        _pause();
        State = PauseType.Normal;
    }

    /// <summary>
    /// Pause due to a popup.
    /// </summary>
    public void PopupPause()
    {
        _pause();
        State = PauseType.Popup;
    }

    /// <summary>
    /// Pause due to the hold picker menu.
    /// </summary>
    public void HoldPickerPause()
    {
        _pause();
        State = PauseType.HoldPicker;
    }

    /// <summary>
    /// Continue (unpause).
    /// </summary>
    public void Unpause()
    {
        State = PauseType.Unpaused;
        _unpause();
    }

    private void _pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        _root.enabled = true;

        foreach (var hook in _pauseHooks)
            hook();
    }

    private void _unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        _root.enabled = false;

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