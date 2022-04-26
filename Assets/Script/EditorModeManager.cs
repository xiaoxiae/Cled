using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorModeManager : MonoBehaviour
{
    /// <summary>
    /// The possible modes that the editor can be in.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// The mode that we're usually in.
        /// </summary>
        Normal,
        
        /// <summary>
        /// We're holding a hold.
        /// </summary>
        Holding,
        
        /// <summary>
        /// We've selected a route.
        /// </summary>
        Route
    }

    private Mode _currentMode;
    
    /// <summary>
    /// The current mode that we're in.
    /// </summary>
    public Mode CurrentMode
    {
        get => _currentMode;
        set
        {
            _currentMode = value;

            foreach (var callback in _modeChangeCallbacks)
                callback(value);
        }
    }

    private readonly List<Action<Mode>> _modeChangeCallbacks = new();
    
    /// <summary>
    /// Add a callback that gets called when the mode is changed.
    /// </summary>
    public void AddModeChangeCallback(Action<Mode> callback) => _modeChangeCallbacks.Add(callback);
}
