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
    }
}