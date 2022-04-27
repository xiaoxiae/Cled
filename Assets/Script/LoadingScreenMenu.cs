using UnityEngine;
using UnityEngine.UIElements;

public class LoadingScreenMenu : MonoBehaviour
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

        _document.sortingOrder = 100;
        
        if (!string.IsNullOrWhiteSpace(text))
            _loadingLabel.text = text;
    }

    public void Close() => _root.visible = false;

    /// <summary>
    /// Move the loading screen behind the popup.
    /// </summary>
    public void ToBehindPopup() => _document.sortingOrder = 9;
}
