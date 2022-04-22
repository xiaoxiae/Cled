using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public PopupManager popupManager;
    public PauseManager pauseManager;

    void Start()
    {
        // we're in a menu, don't lock stuff
        pauseManager.KeepUnlocked = true;
    }
    
    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var continueButton = root.Q<Button>("continue-button");
        
        var continuePathLabel = root.Q<Label>("continue-path-label");
        
        
        var versionLabel = root.Q<Label>("version-label");
        versionLabel.text = $"version {Application.version}";

        if (PreferencesManager.LastOpenWallPath == "" || !File.Exists(PreferencesManager.LastOpenWallPath)) {
            continueButton.SetEnabled(false);
            continuePathLabel.style.display = DisplayStyle.None;
            continuePathLabel.style.opacity = 100;
        }
        else
        {
            continueButton.clicked += () => SceneManager.LoadScene("WallScene");

            string path = PreferencesManager.LastOpenWallPath;

            if (path.Length > 50)
                path = path[..25] + "..." + path[^25..];
            
            continuePathLabel.text = path;
        }

        var openButton = root.Q<Button>("open-button");
        MenuUtilities.AddOpenButtonOperation(openButton, popupManager);
        
        var newButton = root.Q<Button>("new-button");
        MenuUtilities.AddNewButtonOperation(newButton);

        var quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }
}