using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

/// <summary>
/// The types of pauses there can be.
/// Some (like popup pauses) behave differently, since it has to be dealt with first.
/// </summary>
public enum PauseType
{
    Normal,
    Popup,
    HoldPicker,
    RouteSettings,
    Settings,
}

/// <summary>
/// A manager that handles pausing due to various reasons.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private readonly HashSet<PauseType> _pauses = new();

    private UIDocument _root;

    private readonly List<Action> _pauseHooks = new();
    private readonly List<Action> _unpauseHooks = new();

    public void Awake()
    {
        _root = GetComponent<UIDocument>();
        _unpause();
    }

    /// <summary>
    /// Unpause the given type.
    /// </summary>
    public void UnpauseType(PauseType type)
    {
        _pauses.Remove(type);
        
        if (_pauses.Count == 1 && _pauses.Contains(global::PauseType.Normal))
            UnpauseType(global::PauseType.Normal);
        
        if (_pauses.Count == 0)
            _unpause();
    }

    public void PauseType(PauseType type)
    {
        _pauses.Add(type);
        
        if (_pauses.Count != 0)
            _pause();
    }

    private void _pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        
        _root.enabled = true;

        foreach (var hook in _pauseHooks)
            hook();
    }

    /// <summary>
    /// Reset timescale, unlock cursor.
    /// </summary>
    private void _unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        
        _root.enabled = false;

        foreach (var hook in _unpauseHooks)
            hook();
    }

    /// <summary>
    /// Return true if the editor is currently unpaused.
    /// </summary>
    public bool IsAllUnpaused() => _pauses.Count == 0;

    /// <summary>
    /// Return true if the editor is currently paused.
    /// </summary>
    public bool IsAnyPaused() => !IsAllUnpaused();

    /// <summary>
    /// Return true if the editor is currently unpaused via this pause type.
    /// </summary>
    public bool IsTypePaused(PauseType type) => _pauses.Contains(type);

    /// <summary>
    /// Add a hook function that gets called every time the editor is paused.
    /// </summary>
    public void AddPausedHook(Action hook) => _pauseHooks.Add(hook);

    /// <summary>
    /// Add a hook function that gets called every time the editor is unpaused.
    /// </summary>
    public void AddUnpausedHook(Action hook) => _unpauseHooks.Add(hook);

    /// <summary>
    /// Unpause everything.
    /// </summary>
    public void UnpauseAll()
    {
        _pauses.Clear();
        _unpause();
    }

    /// <summary>
    /// Move the pause screen to the back (behind everything).
    /// </summary>
    public void PauseScreenToBack() => _root.sortingOrder = -10;

    /// <summary>
    /// Move the pause screen to the front (right behind popups).
    /// </summary>
    public void PauseScreenToFront() => _root.sortingOrder = 10;
}