using System;
using UnityEngine;

/// <summary>
///     A class for managing both session-based and persistent preferences.
/// </summary>
public static class Preferences
{
    private const float DefaultShadowStrength = 0.2f;
    private const float DefaultLightIntensity = 0.2f;

    private const int DefaultImageSupersize = 1;

    private static readonly string DefaultCaptureImagePath =
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    /// <summary>
    ///     The path to where the image captures are stored.
    ///     Defaults to the pictures folder.
    /// </summary>
    public static string CaptureImagePath
    {
        get => PlayerPrefs.GetString("CaptureImagePath", DefaultCaptureImagePath);
        set => PlayerPrefs.SetString("CaptureImagePath", value);
    }

    public static int ImageSupersize
    {
        get => PlayerPrefs.GetInt("ImageSupersize", DefaultImageSupersize);
        set => PlayerPrefs.SetInt("ImageSupersize", value);
    }

    public static float LightIntensity
    {
        get => PlayerPrefs.GetFloat("LightIntensity", DefaultLightIntensity);
        set => PlayerPrefs.SetFloat("LightIntensity", value);
    }

    public static float ShadowStrength
    {
        get => PlayerPrefs.GetFloat("ShadowStrength", DefaultShadowStrength);
        set => PlayerPrefs.SetFloat("ShadowStrength", value);
    }

    /// <summary>
    ///     The path to the last opened wall (including the holds, etc.) path.
    ///     Used when the editor scene is loaded - if it isn't empty, this path is used.
    /// </summary>
    public static string LastOpenWallPath
    {
        get => PlayerPrefs.GetString("LastOpenWallPath");
        set => PlayerPrefs.SetString("LastOpenWallPath", value);
    }

    /// <summary>
    ///     The current wall model path.
    ///     Used when the editor scene is loaded and the LastOpenWallPath is empty.
    /// </summary>
    public static string CurrentWallModelPath
    {
        get => PlayerPrefs.GetString("CurrentWallModelPath");
        set => PlayerPrefs.SetString("CurrentWallModelPath", value);
    }

    /// <summary>
    ///     The current hold models path.
    ///     Again used when the editor scene is loaded and the LastOpenWallPath is empty.
    /// </summary>
    public static string CurrentHoldModelsPath
    {
        get => PlayerPrefs.GetString("CurrentHoldModelsPath");
        set => PlayerPrefs.SetString("CurrentHoldModelsPath", value);
    }

    /// <summary>
    ///     Whether the editor has been initialized.
    ///     Used to determine things like whether to show "save or discard" when opening a new project.
    /// </summary>
    public static bool Initialized { get; set; }

    /// <summary>
    ///     Reset preferences to default.
    /// </summary>
    public static void SetToDefault()
    {
        ShadowStrength = DefaultShadowStrength;
        LightIntensity = DefaultLightIntensity;
        CaptureImagePath = DefaultCaptureImagePath;
        ImageSupersize = DefaultImageSupersize;
    }
}