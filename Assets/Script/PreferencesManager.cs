using UnityEngine;

public static class PreferencesManager
{
    /// <summary>
    /// The path to the last opened block (including the holds, etc.) path.
    /// </summary>
    public static string LastOpenBlockPath
    {
        get => PlayerPrefs.GetString("LastOpenBlockPath");
        set => PlayerPrefs.SetString("LastOpenBlockPath", value);
    }
    
    /// <summary>
    /// The current block model path.
    /// </summary>
    public static string CurrentBlockModelPath
    {
        get => PlayerPrefs.GetString("CurrentBlockModelPath");
        set => PlayerPrefs.SetString("CurrentBlockModelPath", value);
    }
}
