using UnityEngine;

public class PreferencesManager : MonoBehaviour
{
    /// <summary>
    /// The path to the last open block.
    /// </summary>
    public string LastOpenBlockPath
    {
        get => PlayerPrefs.GetString("LastOpenBlockPath");
        set => PlayerPrefs.SetString("LastOpenBlockPath", value);
    }
}
