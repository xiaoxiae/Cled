using SFB;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuUtilities
{
    /// <summary>
    /// Attach a load project operation to a button.
    /// </summary>
    /// <param name="button"></param>
    public static void AddLoadButtonOperation(Button button)
    {
        button.clicked += () =>
            StandaloneFileBrowser.OpenFilePanelAsync("Load existing project", "",
                new[] { new ExtensionFilter("Cled Project Files (.yaml)", "yaml") }, false, onLoadWall);
    }

    /// <summary>
    /// Called when loading an existing wall.
    /// </summary>
    private static void onLoadWall(string[] paths)
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
    /// <param name="button"></param>
    public static void AddNewButtonOperation(Button button)
    {
        button.clicked += () => StandaloneFileBrowser.OpenFilePanelAsync("Open wall object", "",
            new[] { new ExtensionFilter("Object Files (.obj)", "obj") }, false, onOpenNewWall);
    }
    

    /// <summary>
    /// Called when opening a new wall.
    /// Prompts opening holds if successful.
    /// </summary>
    private static void onOpenNewWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentWallModelPath = paths[0];

        StandaloneFileBrowser.OpenFolderPanelAsync("Open holds directory", "", false, onOpenNewHolds);
    }

    /// <summary>
    /// Called when opening a new wall.
    /// </summary>
    private static void onOpenNewHolds(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentHoldModelsPath = paths[0];

        PreferencesManager.LastOpenWallPath = null;
        
        SceneManager.LoadScene("WallScene");
    }
}