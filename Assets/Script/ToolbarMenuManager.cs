using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarMenuManager : MonoBehaviour
{
    public StateImportExportManager StateImportExportManager;
    public PopupManager PopupManager;
    public PauseManager PauseManager;

    private bool _forceSave;
    private bool _forceSaveAs;

    private VisualElement _root;

    /// <summary>
    /// Set the forceSave attribute to a given state, along with possibly enabling/disabling the save button.
    /// </summary>
    private void SetForceSave(bool state)
    {
        _forceSave = state;

        // only enable save button if save as is not
        if (state)
        {
            if (!_forceSaveAs)
                _saveButton.SetEnabled(true);
        }
        else
        {
            _saveButton.SetEnabled(false);
        }
    }

    /// <summary>
    /// Forces a save as when either main menu or quit is called.
    /// Should be called if the wall is not saved at all after loading the editor scene.
    /// </summary>
    public void ForceSaveAs() => _forceSaveAs = true;

    private Button _saveButton;
    private Button _saveAsButton;

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        GetComponent<UIDocument>().sortingOrder = 10;

        var openButton = _root.Q<Button>("open-button");
        MenuUtilities.AddOpenButtonOperation(openButton, PopupManager);

        var newButton = _root.Q<Button>("new-button");
        MenuUtilities.AddNewButtonOperation(newButton);

        _saveButton = _root.Q<Button>("save-button");
        _saveButton.SetEnabled(false);
        _saveButton.clicked += () => Save();

        _saveAsButton = _root.Q<Button>("save-as-button");
        _saveAsButton.clicked += () => SaveAs();

        var quitButton = _root.Q<Button>("quit-button");
        quitButton.clicked += Quit;

        var aboutButton = _root.Q<Button>("about-button");
        aboutButton.clicked += () =>
        {
            PopupManager.CreateInfoPopup(
                "This program was created in 2022 and maintained by Tomáš Sláma as a part of a bachelor thesis. The project is open source under GLPv3 and is open to pull requests, should you find any bugs or missing features.");
        };

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

        SetForceSave(false);
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
    private void SaveAs()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "",
            new[] { new ExtensionFilter("Cled Data Files (.yaml)", "yaml") });

        if (string.IsNullOrWhiteSpace(path))
            return;
        
        path = Utilities.EnsureExtension(path, "yaml");

        if (!StateImportExportManager.Export(path))
            return;

        PreferencesManager.LastOpenWallPath = path;

        _forceSaveAs = false;
    }

    /// <summary>
    /// Attempt to quit.
    /// </summary>
    private void Quit()
    {
        if (_forceSaveAs)
        {
            PopupManager.CreateSavePopup("Save As",
                SaveAs,
                Application.Quit,
                () => { }
            );
        }
        else if (_forceSave)
        {
            PopupManager.CreateSavePopup("Save",
                () => { Save(); },
                Application.Quit,
                () => { }
            );
        }
    }

    void Update()
    {
        // only work if a popup isn't already present
        if (!PauseManager.IsPaused(PauseType.Popup))
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) &&
                     Input.GetKeyDown(KeyCode.S))
                SaveAs();

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                if (!Save())
                    SaveAs();
            }

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
                MenuUtilities.New();

            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
                MenuUtilities.Open(PopupManager);
        }
    }
}