using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupManager : MonoBehaviour
{
    public VisualTreeAsset InfoPopup;
    public VisualTreeAsset SavePopup;

    private UIDocument document;

    public PauseManager pauseManager;

    public void Start()
    {
        document = GetComponent<UIDocument>();
        document.sortingOrder = 10;
    }

    /// <summary>
    /// Create an info popup with the given content.
    /// </summary>
    public void CreateInfoPopup(string contents)
    {
        document.visualTreeAsset = InfoPopup;

        var root = document.rootVisualElement;

        if (pauseManager.State == PauseType.Unpaused)
            pauseManager.PopupPause();

        root.Q<Label>("contents").text = contents;
        root.Q<Button>("ok-button").clicked += () =>
        {
            document.visualTreeAsset = null;

            if (pauseManager.State != PauseType.HoldPicker)
                pauseManager.Unpause();
        };
    }

    /// <summary>
    /// Create a save/save as popup with the operation button name.
    /// </summary>
    public void CreateSavePopup(string operationName,
        Action operationFunction, Action discardFunction, Action cancelButton)
    {
        document.visualTreeAsset = SavePopup;

        var root = document.rootVisualElement;

        if (pauseManager.State == PauseType.Unpaused)
            pauseManager.PopupPause();

        root.Q<Button>("operation-button").text = operationName;
        root.Q<Button>("operation-button").clicked += () => { operationFunction(); };
        root.Q<Button>("discard-button").clicked += () => { discardFunction(); };
        root.Q<Button>("cancel-button").clicked += () => { cancelButton(); };

        root.Q<Button>("operation-button").clicked += () =>
        {
            document.visualTreeAsset = null;

            if (pauseManager.State != PauseType.HoldPicker)
                pauseManager.Unpause();
        };
        root.Q<Button>("discard-button").clicked += () =>
        {
            document.visualTreeAsset = null;

            if (pauseManager.State != PauseType.HoldPicker)
                pauseManager.Unpause();
        };
        root.Q<Button>("cancel-button").clicked += () =>
        {
            document.visualTreeAsset = null;

            if (pauseManager.State != PauseType.HoldPicker)
                pauseManager.Unpause();
        };
    }
}