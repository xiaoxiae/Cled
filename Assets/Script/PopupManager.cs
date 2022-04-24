using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupManager : MonoBehaviour
{
    public VisualTreeAsset InfoPopup;
    public VisualTreeAsset SavePopup;

    private UIDocument document;

    public PauseManager pauseManager;

    public void Awake()
    {
        document = GetComponent<UIDocument>();
    }

    public void Close()
    {
        document.visualTreeAsset = null;
        pauseManager.UnpauseType(PauseType.Popup);
    }

    /// <summary>
    /// Create an info popup with the given content.
    /// </summary>
    public void CreateInfoPopup(string contents)
    {
        document.visualTreeAsset = InfoPopup;

        var root = document.rootVisualElement;
        Utilities.DisableElementFocusable(root);

        pauseManager.PauseType(PauseType.Popup);

        root.Q<Label>("contents").text = contents;
        root.Q<Button>("ok-button").clicked += Close;
    }

    /// <summary>
    /// Create a save/save as popup with the operation button name.
    /// </summary>
    public void CreateSavePopup(string operationName, Action operationAction, Action discardAction, Action cancelButton)
    {
        document.visualTreeAsset = SavePopup;

        var root = document.rootVisualElement;

        pauseManager.PauseType(PauseType.Popup);

        root.Q<Button>("operation-button").text = operationName;
        root.Q<Button>("operation-button").clicked += operationAction;
        root.Q<Button>("discard-button").clicked += discardAction;
        root.Q<Button>("cancel-button").clicked += cancelButton;

        root.Q<Button>("operation-button").clicked += Close;
        root.Q<Button>("discard-button").clicked += Close;
        root.Q<Button>("cancel-button").clicked += Close;
    }
}