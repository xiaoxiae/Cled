using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using SimpleFileBrowser;
using UnityEngine.SceneManagement;
using static PreferencesManager;

public class MenuController : MonoBehaviour
{
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        var continueButton = root.Q<Button>("continue-button");
        
        if (LastOpenBlockPath == "" || !File.Exists(LastOpenBlockPath))
            continueButton.SetEnabled(false);
        else
            continueButton.clicked += () => SceneManager.LoadScene("WallScene");
        
        var loadButton = root.Q<Button>("load-button");
        loadButton.clicked += () => FileBrowser.ShowLoadDialog(onLoadBlock, null, FileBrowser.PickMode.Files);
        
        var newButton = root.Q<Button>("new-button");
        newButton.clicked += () => FileBrowser.ShowLoadDialog(onOpenNewBlock, null, FileBrowser.PickMode.Files);

        var quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }
    
    /// <summary>
    /// Called when loading a block.
    /// </summary>
    void onLoadBlock(string[] paths)
    {
        LastOpenBlockPath = paths[0];
        SceneManager.LoadScene("WallScene");
    }

    /// <summary>
    /// Called when opening a new block.
    /// </summary>
    void onOpenNewBlock(string[] paths)
    {
        CurrentBlockModelPath = paths[0];
        LastOpenBlockPath = null;
        SceneManager.LoadScene("WallScene");
    }
}
