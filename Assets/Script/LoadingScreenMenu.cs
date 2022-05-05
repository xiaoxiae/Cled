using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The class that controls the loadig screen - its text, whether it's shown or not and
/// </summary>
public class LoadingScreenMenu : MonoBehaviour, IClosable
{
    private UIDocument _document;
    private VisualElement _root;
    private Label _loadingLabel;
    
    void Awake()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;
        _loadingLabel = _root.Q<Label>("loading-label");
        
        Close();
    }

    public void Show(string text = "")
    {
        _root.visible = true;
        
        if (!string.IsNullOrWhiteSpace(text))
            _loadingLabel.text = text;
    }

    public void Close() => _root.visible = false;
}
