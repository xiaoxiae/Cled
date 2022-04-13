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

        if (PreferencesManager.LastOpenWallPath == "" || !File.Exists(PreferencesManager.LastOpenWallPath))
            continueButton.SetEnabled(false);
        else
            continueButton.clicked += () => SceneManager.LoadScene("WallScene");

        var loadButton = root.Q<Button>("load-button");
        MenuUtilities.AddLoadButtonOperation(loadButton);
        
        var newButton = root.Q<Button>("new-button");
        MenuUtilities.AddNewButtonOperation(newButton);

        var quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }
}