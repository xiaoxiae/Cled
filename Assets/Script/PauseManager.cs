using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public enum PauseType
{
    Normal,
    Popup,
    HoldPicker,
    RouteSettings,
}

/// <summary>
/// A manager that handles pausing due to various reasons.
/// </summary>
public class PauseManager : MonoBehaviour
{
    private HashSet<PauseType> Pauses = new();

    private UIDocument _root;

    public void Start()
    {
        _root = GetComponent<UIDocument>();
        _unpause();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused(PauseType.Normal))
                Unpause(PauseType.Normal);
            else if (IsUnpaused())
                Pause(PauseType.Normal);
        }
    }

    /// <summary>
    /// Unpause the given type.
    /// </summary>
    /// <param name="unpauseIfNormallyPaused">If the editor is paused normally, unpause the normal pause.</param>
    public void Unpause(PauseType type, bool unpauseIfNormallyPaused = true)
    {
        Pauses.Remove(type);
        
        if (Pauses.Count == 1 && Pauses.Contains(PauseType.Normal))
            Unpause(PauseType.Normal);
        
        if (Pauses.Count == 0)
            _unpause();
    }

    public void Pause(PauseType type)
    {
        Pauses.Add(type);
        
        if (Pauses.Count != 0)
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
    /// Return true if the editor is currently unpaused.
    /// </summary>
    public bool IsUnpaused() => Pauses.Count == 0;

    /// <summary>
    /// Return true if the editor is currently unpaused via this pause type.
    /// </summary>
    public bool IsPaused(PauseType type) => Pauses.Contains(type);

    /// <summary>
    /// Add a hook function that gets called every time the editor is paused.
    /// </summary>
    public void AddPausedHook(Action hook) => _pauseHooks.Add(hook);

    /// <summary>
    /// Add a hook function that gets called every time the editor is unpaused.
    /// </summary>
    public void AddUnpausedHook(Action hook) => _unpauseHooks.Add(hook);
}