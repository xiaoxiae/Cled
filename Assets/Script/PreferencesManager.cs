using UnityEngine;

public static class PreferencesManager
{
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
    /// </summary>
    public static string CurrentHoldModelsPath
    {
        get => PlayerPrefs.GetString("CurrentHoldModelsPath");
        set => PlayerPrefs.SetString("CurrentHoldModelsPath", value);
    }
}
