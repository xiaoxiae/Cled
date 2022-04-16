using SFB;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuUtilities
{
    /// <summary>
    /// Attach an open project operation to a button.
    /// </summary>
    /// <param name="button"></param>
    public static void AddOpenButtonOperation(Button button) => button.clicked += Open;

    /// <summary>
    /// Prompt opening an existing project.
    /// </summary>
    public static void Open()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Open existing project", "",
            new[] { new ExtensionFilter("Cled Project Files (.yaml)", "yaml") }, false, OnOpenWall);
    }

    /// <summary>
    /// Called when opening an existing wall.
    /// </summary>
    private static void OnOpenWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        if (!StateImportExportManager.ImportPreferences(paths[0]))
            return;

        PreferencesManager.LastOpenWallPath = paths[0];
        SceneManager.LoadScene("WallScene");
    }

    /// <summary>
    /// Attach a new project operation to a button.
    /// </summary>
    public static void AddNewButtonOperation(Button button) => button.clicked += New;

    /// <summary>
    /// Prompt opening a new wall and holds.
    /// </summary>
    public static void New()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Open wall object", "",
            new[] { new ExtensionFilter("Object Files (.obj)", "obj") }, false, OnOpenNewWall);
    }

    /// <summary>
    /// Called when opening a new wall.
    /// Prompts opening holds if successful.
    /// </summary>
    private static void OnOpenNewWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentWallModelPath = paths[0];

        StandaloneFileBrowser.OpenFolderPanelAsync("Open holds directory", "", false, OnOpenNewHolds);
    }

    /// <summary>
    /// Called when opening a new wall.
    /// </summary>
    private static void OnOpenNewHolds(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentHoldModelsPath = paths[0];

        PreferencesManager.LastOpenWallPath = null;

        SceneManager.LoadScene("WallScene");
    }
}