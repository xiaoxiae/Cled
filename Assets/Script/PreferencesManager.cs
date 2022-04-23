using System;
using UnityEngine;

public static class PreferencesManager
{
    /// <summary>
    /// The path to where the image captures are stored.
    /// Defaults to the pictures folder.
    /// </summary>
    public static string CaptureImagePath
    {
        get => PlayerPrefs.GetString("CaptureImagePath", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        set => PlayerPrefs.SetString("CaptureImagePath", value);
    }

    /// <summary>
    /// How 
    /// </summary>
    public static int CaptureImageMultiplier
    {
        get => PlayerPrefs.GetInt("CaptureImageMultiplier", 1);
        set => PlayerPrefs.GetInt("CaptureImageMultiplier", value);
    }

    /// <summary>
    /// The path to the last opened wall (including the holds, etc.) path.
    /// Used when the editor scene is loaded - if it isn't empty, this path is used.
    /// </summary>
    public static string LastOpenWallPath
    {
        get => PlayerPrefs.GetString("LastOpenWallPath");
        set => PlayerPrefs.SetString("LastOpenWallPath", value);
    }
    
    /// <summary>
    /// The current wall model path.
    /// Used when the editor scene is loaded and the LastOpenWallPath is empty.
    /// </summary>
    public static string CurrentWallModelPath
    {
        get => PlayerPrefs.GetString("CurrentWallModelPath");
        set => PlayerPrefs.SetString("CurrentWallModelPath", value);
    }
    
    /// <summary>
    /// The current hold models path.
    /// Again used when the editor scene is loaded and the LastOpenWallPath is empty.
    /// </summary>
    public static string CurrentHoldModelsPath
    {
        get => PlayerPrefs.GetString("CurrentHoldModelsPath");
        set => PlayerPrefs.SetString("CurrentHoldModelsPath", value);
    }
    
    /// <summary>
    /// Whether the editor has been initialized.
    /// Used to determine things like whether to show "save or discard" when opening a new project.
    /// </summary>
    public static bool Initialized
    {
        get => PlayerPrefs.GetInt("Initialized") != 0;
        set => PlayerPrefs.SetInt("Initialized", value ? 1 : 0);
    }
}
