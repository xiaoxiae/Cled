using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var continueButton = root.Q<Button>("continue-button");

        if (PreferencesManager.LastOpenWallPath == "" || !File.Exists(PreferencesManager.LastOpenWallPath))
            continueButton.SetEnabled(false);
        else
            continueButton.clicked += () => SceneManager.LoadScene("WallScene");

        var loadButton = root.Q<Button>("load-button");
        loadButton.clicked += () =>
            StandaloneFileBrowser.OpenFilePanelAsync("Load existing project", "",
                new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") }, false, onLoadWall);

        var newButton = root.Q<Button>("new-button");
        newButton.clicked += () => StandaloneFileBrowser.OpenFilePanelAsync("Open wall object", "",
            new[] { new ExtensionFilter("Object Files (.obj)", "obj") }, false, onOpenNewWall);

        var quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }

    /// <summary>
    /// Called when loading an existing wall.
    /// </summary>
    void onLoadWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        if (!StateImportExportManager.ImportPreferences(paths[0]))
            return;

        PreferencesManager.LastOpenWallPath = paths[0];
        SceneManager.LoadScene("WallScene");
    }

    /// <summary>
    /// Called when opening a new wall.
    /// Prompts opening holds if successful.
    /// </summary>
    void onOpenNewWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentWallModelPath = paths[0];

        StandaloneFileBrowser.OpenFolderPanelAsync("Open holds directory", "", false, onOpenNewHolds);
    }

    /// <summary>
    /// Called when opening a new wall.
    /// </summary>
    void onOpenNewHolds(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        PreferencesManager.CurrentHoldModelsPath = paths[0];

        PreferencesManager.LastOpenWallPath = null;
        
        SceneManager.LoadScene("WallScene");
    }
}