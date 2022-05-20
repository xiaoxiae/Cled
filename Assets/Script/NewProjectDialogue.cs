using System;
using System.Globalization;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

public class NewProjectDialogue : MonoBehaviour, IClosable, IAcceptable, IResetable
{
    private VisualElement _root;

    public PauseMenu pauseMenu;
    public Importer importer;
    public PopupMenu popupMenu;

    private TextField wallTextField;
    private TextField holdsTextField;
    
    private void Awake()
    {
        var document = GetComponent<UIDocument>();

        _root = document.rootVisualElement;
        _root.visible = false;
        
        wallTextField = _root.Q<TextField>("wall-text-field");
        holdsTextField = _root.Q<TextField>("holds-text-field");

        var holdsOpenButton = _root.Q<Button>("holds-open-button");
        holdsOpenButton.clicked += () =>
        {
            StandaloneFileBrowser.OpenFolderPanelAsync("Open holds directory", "", false, OnOpenNewHolds);
        };

        var wallOpenButton = _root.Q<Button>("wall-open-button");
        wallOpenButton.clicked += () =>
        {
            StandaloneFileBrowser.OpenFilePanelAsync("Open wall object", "",
                new[] { new ExtensionFilter("Object Files (.obj)", "obj") }, false, OnOpenNewWall);
        };

        var applyButton = _root.Q<Button>("apply-button");
        applyButton.clicked += Accept;

        var discardButton = _root.Q<Button>("close-button");
        discardButton.clicked += Close;
    }

    public void Accept()
    {
        if (String.IsNullOrWhiteSpace(wallTextField.value))
        {
            popupMenu.CreateInfoPopup("No wall path specified!");
            return;
        }
        
        if (String.IsNullOrWhiteSpace(holdsTextField.value))
        {
            popupMenu.CreateInfoPopup("No holds path specified!");
            return;
        }

        importer.ImportFromNew(
            wallTextField.value,
            holdsTextField.value
        );

        Close();
    }

    /// <summary>
    ///     Close the settings, clearing them.
    /// </summary>
    public void Close()
    {
        _root.visible = false;
        pauseMenu.UnpauseType(PauseType.NewProjectDialogue);

        Reset();
    }

    public void Reset()
    {
        wallTextField.SetValueWithoutNotify("");
        holdsTextField.SetValueWithoutNotify("");
    }

    /// <summary>
    ///     Show the settings.
    /// </summary>
    public void Show()
    {
        _root.visible = true;
        Reset();

        pauseMenu.PauseType(PauseType.NewProjectDialogue);
    }

    /// <summary>
    ///     Called when opening a new wall.
    ///     Prompts opening holds if successful.
    /// </summary>
    private void OnOpenNewWall(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        wallTextField.SetValueWithoutNotify(paths[0]);
    }

    /// <summary>
    ///     Called when opening a new wall.
    /// </summary>
    private void OnOpenNewHolds(string[] paths)
    {
        if (paths.Length == 0 || string.IsNullOrWhiteSpace(paths[0]))
            return;

        holdsTextField.SetValueWithoutNotify(paths[0]);
    }
}