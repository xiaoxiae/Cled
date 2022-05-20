using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarMenu : MonoBehaviour
{
    public Importer importer;
    public Exporter exporter;

    public PopupMenu popupMenu;
    public PauseMenu pauseMenu;
    public LightManager lightManager;
    public SettingsMenu settingsMenu;
    public EditorModeManager editorModeManager;
    public RouteViewMenu routeViewMenu;
    public HoldPickerMenu holdPickerMenu;
    public NewProjectDialogue newProjectDialogue;
    public SaveManager saveManager;
    
    private Button _captureImageAsButton;
    private Button _captureImageButton;

    private Button _holdMenuButton;
    private Button _newButton;
    private Button _openButton;
    private Button _preferencesButton;

    private VisualElement _root;
    private Button _saveAsButton;

    private Button _saveButton;

    private bool _wantsToQuit = false;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        // mode label
        var currentModeLabel = _root.Q<Label>("current-mode-label");
        editorModeManager.AddModeChangeCallback(mode => { currentModeLabel.text = mode.ToString().ToUpper(); });

        // lighting
        var addLightingButton = _root.Q<Button>("add-lighting-button");
        addLightingButton.clicked += lightManager.AddLightAtPlayer;

        var clearLightingButton = _root.Q<Button>("clear-lighting-button");
        clearLightingButton.clicked += lightManager.Reset;

        // route view 
        var routeViewToggle = _root.Q<Toggle>("routes-toggle");
        routeViewToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue) routeViewMenu.Show();
            else routeViewMenu.Close();
        });

        // holds
        _holdMenuButton = _root.Q<Button>("hold-menu-button");
        _holdMenuButton.clicked += holdPickerMenu.ToggleOpen;

        // files
        _openButton = _root.Q<Button>("open-button");
        _openButton.clicked += () => EnsureSavedAction(Open);

        _newButton = _root.Q<Button>("new-button");
        _newButton.clicked += () => EnsureSavedAction(newProjectDialogue.Show);

        _saveButton = _root.Q<Button>("save-button");
        _saveButton.clicked += () =>
        {
            if (!Save())
                SaveAs();
        };

        _saveAsButton = _root.Q<Button>("save-as-button");
        _saveAsButton.clicked += () => SaveAs();

        _preferencesButton = _root.Q<Button>("preferences-button");
        _preferencesButton.clicked += () => settingsMenu.Show();

        var quitButton = _root.Q<Button>("quit-button");
        quitButton.clicked += Quit;

        // help
        var helpButton = _root.Q<Button>("help-button");
        helpButton.clicked += () => Application.OpenURL("https://github.com/Climber-Tools/Cled");

        var aboutButton = _root.Q<Button>("about-button");
        aboutButton.clicked += () =>
        {
            popupMenu.CreateInfoPopup(
                "This program was created in 2022 and maintained by Tomáš Sláma as a part of a bachelor thesis." +
                $"The project is open source under GLPv3.\n\nCurrent version: <b>{Application.version}</b>",
                displayLogo: true);
        };

        // capturing
        _captureImageButton = _root.Q<Button>("capture-image-button");
        _captureImageButton.clicked += () =>
        {
            if (!Preferences.Initialized)
            {
                if (!pauseMenu.IsTypePaused(PauseType.Popup))
                    popupMenu.CreateInfoPopup("Wall not loaded, can't take images!");
            }
            else
                // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings?redirectedfrom=MSDN#month-m-format-specifier
            {
                StartCoroutine(CaptureScreen(Path.Join(Preferences.CaptureImagePath,
                    $"cled_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png")));
            }
        };

        _captureImageAsButton = _root.Q<Button>("capture-image-as-button");
        _captureImageAsButton.clicked += () =>
        {
            if (!Preferences.Initialized)
            {
                if (!pauseMenu.IsTypePaused(PauseType.Popup))
                    popupMenu.CreateInfoPopup("Wall not loaded, can't take images!");
            }
            else
            {
                // TODO: this is copy-pasted from loading code
                var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
                    new[] { new ExtensionFilter("PNG Images (.png)", "png") });

                if (string.IsNullOrWhiteSpace(path))
                    return;

                path = Utilities.EnsureExtension(path, "png");

                StartCoroutine(CaptureScreen(path));
            }
        };

        // close one foldout when another is open
        Foldout[] foldouts =
        {
            _root.Q<Foldout>("file-foldout"),
            _root.Q<Foldout>("view-foldout"),
            _root.Q<Foldout>("capture-foldout"),
            _root.Q<Foldout>("lighting-foldout"),
            _root.Q<Foldout>("help-foldout")
        };

        foreach (var foldout in foldouts)
        {
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    // close other foldouts
                    foreach (var f in foldouts)
                        if (f != foldout)
                            f.value = false;
            });

            pauseMenu.AddPausedHook(() => { foldout.value = false; });
            pauseMenu.AddUnpausedHook(() => { foldout.value = false; });
        }
        
        // prevent the application from quitting and trigger a save instead
        Application.wantsToQuit += () =>
        {
            if (_wantsToQuit)
                return true;

            EnsureSavedAction(() =>
            {
                _wantsToQuit = true;
                Application.Quit();
            });

            return false;
        };
    }

    private void Start()
    {
        var playerLightToggle = _root.Q<Toggle>("player-light-toggle");
        playerLightToggle.RegisterValueChangedCallback(evt => lightManager.PlayerLightEnabled = evt.newValue);
        lightManager.AddPlayerLightCallback(value => { playerLightToggle.SetValueWithoutNotify(value); });
    }

    private void Update()
    {
        // only work if a popup isn't already present
        // TODO: a bit of messy code
        if (!pauseMenu.IsTypePaused(PauseType.Popup))
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            {
                using var e = new NavigationSubmitEvent { target = _saveAsButton };
                _saveAsButton.SendEvent(e);
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                using var e = new NavigationSubmitEvent { target = _saveButton };
                _saveButton.SendEvent(e);
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
            {
                using var e = new NavigationSubmitEvent { target = _newButton };
                _newButton.SendEvent(e);
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            {
                using var e = new NavigationSubmitEvent { target = _openButton };
                _openButton.SendEvent(e);
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) &&
                     Input.GetKeyDown(KeyCode.P))
            {
                using var e = new NavigationSubmitEvent { target = _captureImageAsButton };
                _captureImageAsButton.SendEvent(e);
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
            {
                using var e = new NavigationSubmitEvent { target = _captureImageButton };
                _captureImageButton.SendEvent(e);
            }
        }
    }

    /// <summary>
    ///     Attempt to save, possibly failing if the save path doesn't exist yet.
    /// </summary>
    private bool Save(bool popup = true)
    {
        if (!Preferences.Initialized && popup)
        {
            if (!pauseMenu.IsTypePaused(PauseType.Popup))
                popupMenu.CreateInfoPopup("Nothing to save as!");

            return false;
        }

        if (string.IsNullOrWhiteSpace(Preferences.LastOpenWallPath))
            return false;

        if (!exporter.Export(Preferences.LastOpenWallPath))
            return false;

        return true;
    }

    /// <summary>
    ///     Capture the screen as a coroutine, hiding all of the UI in the process.
    /// </summary>
    public IEnumerator CaptureScreen(string path)
    {
        yield return null;

        var documents = FindObjectsOfType<UIDocument>();
        var visibleDocuments = new List<UIDocument>();

        foreach (var document in documents)
            if (document.rootVisualElement is { visible: true })
            {
                visibleDocuments.Add(document);
                document.rootVisualElement.visible = false;
            }

        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();

        // Take screenshot
        ScreenCapture.CaptureScreenshot(
            path,
            Preferences.ImageSupersize
        );

        // Show UI after we're done
        foreach (var document in visibleDocuments)
            document.rootVisualElement.visible = true;
    }

    /// <summary>
    ///     Attempt to save as.
    /// </summary>
    private bool SaveAs(bool popup = true)
    {
        if (!Preferences.Initialized && popup)
        {
            if (!pauseMenu.IsTypePaused(PauseType.Popup))
                popupMenu.CreateInfoPopup("Nothing to save!");

            return false;
        }

        string path;
        if (!pauseMenu.IsTypePaused(PauseType.Popup))
        {
            pauseMenu.PauseType(PauseType.Popup);

            path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
                new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });

            pauseMenu.UnpauseType(PauseType.Popup);
        }
        else
        {
            path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
                new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });
        }

        if (string.IsNullOrWhiteSpace(path))
            return false;

        path = Utilities.EnsureExtension(path, "yaml");

        if (!exporter.Export(path))
            return false;

        Preferences.LastOpenWallPath = path;

        saveManager.ForceSaveAs = false;
        return true;
    }

    /// <summary>
    ///     Attempt to quit.
    /// </summary>
    private void Quit() => EnsureSavedAction(Application.Quit);

    /// <summary>
    ///     Perform the given action, making sure that everything is saved in the process.
    /// </summary>
    private void EnsureSavedAction(Action action)
    {
        // don't actually ensure save when nothing is initialized
        // however, make sure to pause if we're not paused
        if (!Preferences.Initialized)
        {
            pauseMenu.PauseType(PauseType.Normal);
            action();
            return;
        }

        if (saveManager.ForceSaveAs)
            popupMenu.CreateSavePopup("Save As",
                () =>
                {
                    if (SaveAs(false))
                        action();
                },
                action,
                () => { }
            );
        else
            popupMenu.CreateSavePopup("Save",
                () =>
                {
                    if (Save(false))
                        action();
                },
                action,
                () => { }
            );
    }

    /// <summary>
    ///     Prompt opening an existing project.
    /// </summary>
    private void Open()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Open existing project", "",
            new[] { new ExtensionFilter("Cled Project Files (.yaml)", "yaml") }, false, OnOpenWall);
    }

    /// <summary>
    ///     Called when opening an existing wall.
    /// </summary>
    private void OnOpenWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        if (!importer.ImportPreferences(paths[0]))
            return;

        Preferences.LastOpenWallPath = paths[0];

        importer.ImportState(Preferences.LastOpenWallPath);
    }
}