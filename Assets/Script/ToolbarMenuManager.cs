using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarMenuManager : MonoBehaviour
{
    public StateImportExportManager StateImportExportManager;
    public PopupManager PopupManager;
    public PauseManager PauseManager;
    public LightManager lightManager;

    private bool _forceSaveAs;

    private VisualElement _root;

    /// <summary>
    /// Forces a save as when either main menu or quit is called.
    /// Should be called if the wall is not saved at all after loading the editor scene.
    /// </summary>
    public void ForceSaveAs() => _forceSaveAs = true;

    private Button _saveButton;
    private Button _saveAsButton;
    private Button _newButton;
    private Button _openButton;

    void Start()
    {
        // lighting
        var addLightingButton = _root.Q<Button>("add-lighting-button");
        addLightingButton.clicked += lightManager.AddLight;

        var clearLightingButton = _root.Q<Button>("clear-lighting-button");
        clearLightingButton.clicked += lightManager.Clear;

        var playerLightToggle = _root.Q<Toggle>("player-light-toggle");
        playerLightToggle.RegisterValueChangedCallback(evt => lightManager.PlayerLightEnabled = evt.newValue);
        lightManager.AddPlayerLightWatcher(value => { playerLightToggle.SetValueWithoutNotify(value); });
    }

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        GetComponent<UIDocument>().sortingOrder = 10;

        // files
        _openButton = _root.Q<Button>("open-button");
        _openButton.clicked += () => _ensureSavedAction(() => { MenuUtilities.Open(PopupManager); });

        _newButton = _root.Q<Button>("new-button");
        _newButton.clicked += () => _ensureSavedAction(MenuUtilities.New);

        _saveButton = _root.Q<Button>("save-button");
        _saveButton.SetEnabled(false);
        _saveButton.clicked += () => _ensureSavedAction(() => { });

        _saveAsButton = _root.Q<Button>("save-as-button");
        _saveAsButton.clicked += () => SaveAs();

        var quitButton = _root.Q<Button>("quit-button");
        quitButton.clicked += Quit;

        // help
        var aboutButton = _root.Q<Button>("about-button");
        aboutButton.clicked += () =>
        {
            PopupManager.CreateInfoPopup(
                "This program was created in 2022 and maintained by Tomáš Sláma as a part of a bachelor thesis. The project is open source under GLPv3 and is open to pull requests, should you find any bugs or missing features.");
        };

        // capturing
        var captureImageButton = _root.Q<Button>("capture-image-button");
        captureImageButton.clicked += () =>
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings?redirectedfrom=MSDN#month-m-format-specifier
            StartCoroutine(CaptureScreen(Path.Join(PreferencesManager.CaptureImagePath,
                $"cled_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png")));
        };

        var captureImageAsButton = _root.Q<Button>("capture-image-as-button");
        captureImageAsButton.clicked += () =>
        {
            // TODO: this is copy-pasted from loading code
            var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
                new[] { new ExtensionFilter("PNG Images (.png)", "png") });

            if (string.IsNullOrWhiteSpace(path))
                return;

            path = Utilities.EnsureExtension(path, "png");

            StartCoroutine(CaptureScreen(path));
        };

        Foldout[] foldouts =
        {
            _root.Q<Foldout>("file-foldout"),
            _root.Q<Foldout>("view-foldout"),
            _root.Q<Foldout>("capture-foldout"),
            _root.Q<Foldout>("lighting-foldout"),
            _root.Q<Foldout>("help-foldout"),
        };

        foreach (Foldout foldout in foldouts)
        {
            foldout.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue)
                {
                    // close other foldouts
                    foreach (Foldout f in foldouts)
                        if (f != foldout)
                            f.value = false;
                }
            });

            PauseManager.AddPausedHook(() => { foldout.value = false; });
            PauseManager.AddUnpausedHook(() => { foldout.value = false; });
        }
    }

    /// <summary>
    /// Attempt to save, possibly failing if the save path doesn't exist yet.
    /// </summary>
    private bool Save()
    {
        if (String.IsNullOrWhiteSpace(PreferencesManager.LastOpenWallPath))
            return false;

        if (!StateImportExportManager.Export(PreferencesManager.LastOpenWallPath))
            return false;

        return true;
    }

    /// <summary>
    /// Capture the screen as a coroutine, hiding all of the UI in the process.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator CaptureScreen(string path)
    {
        yield return null;

        var documents = FindObjectsOfType<UIDocument>();
        var visibleDocuments = new List<UIDocument>();

        foreach (var document in documents)
            if (document.rootVisualElement.visible)
            {
                visibleDocuments.Add(document);
                document.rootVisualElement.visible = false;
            }

        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();

        // Take screenshot
        ScreenCapture.CaptureScreenshot(
            path,
            PreferencesManager.CaptureImageMultiplier
        );

        // Show UI after we're done
        foreach (var document in visibleDocuments)
            document.rootVisualElement.visible = true;
    }

    /// <summary>
    /// Attempt to save as.
    /// </summary>
    private bool SaveAs()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
            new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });

        if (string.IsNullOrWhiteSpace(path))
            return false;

        path = Utilities.EnsureExtension(path, "yaml");

        if (!StateImportExportManager.Export(path))
            return false;

        PreferencesManager.LastOpenWallPath = path;

        _forceSaveAs = false;
        return true;
    }

    /// <summary>
    /// Attempt to quit.
    /// </summary>
    private void Quit() => _ensureSavedAction(Application.Quit);

    /// <summary>
    /// Perform the action, making sure that everything is saved in the process.
    /// </summary>
    private void _ensureSavedAction(Action action)
    {
        if (_forceSaveAs)
        {
            PopupManager.CreateSavePopup("Save As",
                () =>
                {
                    if (SaveAs())
                        action();
                },
                action,
                () => { }
            );
        }
        else
        {
            PopupManager.CreateSavePopup("Save",
                () =>
                {
                    if (Save())
                        action();
                },
                action,
                () => { }
            );
        }
    }

    void Update()
    {
        // only work if a popup isn't already present
        if (!PauseManager.IsPaused(PauseType.Popup))
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
                using (var e = new NavigationSubmitEvent { target = _saveAsButton })
                    _saveAsButton.SendEvent(e);

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                using (var e = new NavigationSubmitEvent { target = _saveButton })
                    _saveButton.SendEvent(e);

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
                using (var e = new NavigationSubmitEvent { target = _newButton })
                    _newButton.SendEvent(e);

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
                using (var e = new NavigationSubmitEvent { target = _openButton })
                    _openButton.SendEvent(e);
        }
    }
}