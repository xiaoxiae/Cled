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
        
        var continuePathLabel = root.Q<Label>("continue-path-label");

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
        MenuUtilities.AddOpenButtonOperation(openButton);
        
        var newButton = root.Q<Button>("new-button");
        MenuUtilities.AddNewButtonOperation(newButton);

        var quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }
}