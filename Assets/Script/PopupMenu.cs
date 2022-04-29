using System;
using System.Web.UI.Design;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupMenu : MonoBehaviour
{
    public VisualTreeAsset InfoPopup;
    public VisualTreeAsset SavePopup;

    private UIDocument _document;

    public PauseMenu pauseMenu;

    public void Awake()
    {
        _document = GetComponent<UIDocument>();
    }

    public void Close()
    {
        _document.visualTreeAsset = null;
        pauseMenu.UnpauseType(PauseType.Popup);
    }

    /// <summary>
    /// Create an info popup with the given content.
    /// </summary>
    public void CreateInfoPopup(string contents, Action okAction = null)
    {
        _document.visualTreeAsset = InfoPopup;

        var root = _document.rootVisualElement;
        Utilities.DisableElementFocusable(root);

        pauseMenu.PauseType(PauseType.Popup);

        root.Q<Label>("contents").text = contents;
        root.Q<Button>("ok-button").clicked += Close;
        
        if (okAction != null)
            root.Q<Button>("ok-button").clicked += okAction;
    }

    /// <summary>
    /// Create a save/save as popup with the operation button name.
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