using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

/// <summary>
///     The types of pauses there can be.
///     Some (like popup pauses) behave differently, since it has to be dealt with first.
/// </summary>
public enum PauseType
{
    Normal,
    Popup,
    HoldPicker,
    RouteSettings,
    Settings
}

/// <summary>
///     A manager that handles pausing due to various reasons.
/// </summary>
public class PauseMenu : MonoBehaviour, IResetable
{
    // hooks for pausing and unpausing
    private readonly List<Action> _pauseHooks = new();

    // the positions where the dark pause screen should appear
    private readonly Dictionary<PauseType, float> _pausePositions = new()
    {
        { global::PauseType.HoldPicker, 4 },
        { global::PauseType.Normal, 0 },
        { global::PauseType.Popup, 14.99f },
        { global::PauseType.Settings, 2 },
        { global::PauseType.RouteSettings, 2 }
    };

    private readonly HashSet<PauseType> _pauses = new();
    private readonly List<Action> _unpauseHooks = new();

    private UIDocument _document;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        Unpause();
    }

    private void Start()
    {
        if (!Preferences.Initialized)
            PauseType(global::PauseType.Normal);
    }

    public void Reset()
    {
        UnpauseAll();
    }

    /// <summary>
    ///     Unpause the given type.
    /// </summary>
    public void UnpauseType(PauseType type)
    {
        _pauses.Remove(type);
        UpdatePauseScreenPosition();

        // don't actually unpause when we're not initialized
        if (!Preferences.Initialized)
            return;

        // unpause normal if it would be the last pause type
        if (_pauses.Count == 1 && _pauses.Contains(global::PauseType.Normal))
            UnpauseType(global::PauseType.Normal);

        if (_pauses.Count == 0)
            Unpause();
    }

    /// <summary>
    ///     Pause the given type.
    /// </summary>
    public void PauseType(PauseType type)
    {
        _pauses.Add(type);
        UpdatePauseScreenPosition();

        if (_pauses.Count != 0)
            Pause();
    }

    /// <summary>
    ///     An internal function that performs the pausing.
    ///     Stops time, unlocks cursor, calls hooks, shows screen.
    /// </summary>
    private void Pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;

        _document.enabled = true;

        foreach (var hook in _pauseHooks)
            hook();
    }

    /// <summary>
    ///     An internal function that performs the unpausing.
    ///     Starts time, locks cursor, calls hooks, hides screen.
    /// </summary>
    private void Unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;

        _document.enabled = false;

        foreach (var hook in _unpauseHooks)
            hook();
    }

    /// <summary>
    ///     Moves the pause screen to right behind the closest widget.
    /// </summary>
    private void UpdatePauseScreenPosition()
    {
        var max = (from PauseType type in Enum.GetValues(typeof(PauseType))
            where _pauses.Contains(type)
            select _pausePositions[type]).Prepend(int.MinValue).Max();

        _document.sortingOrder = max;
    }

    /// <summary>
    ///     Return true if the editor is currently unpaused.
    /// </summary>
    public bool IsAllUnpaused()
    {
        return _pauses.Count == 0;
    }

    /// <summary>
    ///     Return true if the editor is currently paused.
    /// </summary>
    public bool IsAnyPaused()
    {
        return !IsAllUnpaused();
    }

    /// <summary>
    ///     Return true if the editor is currently unpaused via this pause type.
    /// </summary>
    public bool IsTypePaused(PauseType type)
    {
        return _pauses.Contains(type);
    }

    /// <summary>
    ///     Add a hook function that gets called every time the editor is paused.
    /// </summary>
    public void AddPausedHook(Action hook)
    {
        _pauseHooks.Add(hook);
    }

    /// <summary>
    ///     Add a hook function that gets called every time the editor is unpaused.
    /// </summary>
    public void AddUnpausedHook(Action hook)
    {
        _unpauseHooks.Add(hook);
    }

    /// <summary>
    ///     Unpause everything.
    /// </summary>
    public void UnpauseAll()
    {
        _pauses.Clear();
        UpdatePauseScreenPosition();
        Unpause();
    }
}