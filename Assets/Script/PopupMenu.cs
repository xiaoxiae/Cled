using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
///     A class for showing popups of any kind.
/// </summary>
public class PopupMenu : MonoBehaviour, IClosable, IAcceptable
{
    public VisualTreeAsset InfoPopup;
    public VisualTreeAsset SavePopup;

    public PauseMenu pauseMenu;

    private UIDocument _document;

    public Button CurrentOkButton;

    public void Awake()
    {
        _document = GetComponent<UIDocument>();
    }

    public void Accept()
    {
        if (CurrentOkButton != null)
        {
            using var e = new NavigationSubmitEvent { target = CurrentOkButton };
            CurrentOkButton.SendEvent(e);
        }
    }

    public void Close()
    {
        // we want to do accept for info popups for things like the loading screen
        if (_document.visualTreeAsset == InfoPopup)
            Accept();
        
        _document.visualTreeAsset = null;
        pauseMenu.UnpauseType(PauseType.Popup);
    }

    /// <summary>
    ///     Create an info popup with the given content.
    /// </summary>
    public void CreateInfoPopup(string contents, Action okAction = null, bool displayLogo = false)
    {
        _document.visualTreeAsset = InfoPopup;

        var root = _document.rootVisualElement;

        pauseMenu.PauseType(PauseType.Popup);

        root.Q<Label>("contents").text = contents;

        var logo = root.Q<VisualElement>("logo");
        logo.style.display = !displayLogo ? DisplayStyle.None : DisplayStyle.Flex;

        var okButton = root.Q<Button>("ok-button");

        okButton.clicked += Close;
        if (okAction != null)
            okButton.clicked += okAction;

        CurrentOkButton = okButton;
    }

    /// <summary>
    ///     Create a save/save as popup with the operation button name.
    /// </summary>
    public void CreateSavePopup(string operationName, Action operationAction, Action discardAction, Action cancelButton)
    {
        _document.visualTreeAsset = SavePopup;

        var root = _document.rootVisualElement;

        pauseMenu.PauseType(PauseType.Popup);

        root.Q<Button>("operation-button").text = operationName;
        root.Q<Button>("operation-button").clicked += operationAction;
        root.Q<Button>("discard-button").clicked += discardAction;
        root.Q<Button>("cancel-button").clicked += cancelButton;

        root.Q<Button>("operation-button").clicked += Close;
        root.Q<Button>("discard-button").clicked += Close;
        root.Q<Button>("cancel-button").clicked += Close;
    }
}