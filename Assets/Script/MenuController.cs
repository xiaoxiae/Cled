using UnityEngine;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    public Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        quitButton = root.Q<Button>("quit-button");
        quitButton.clicked += Application.Quit;
    }
}
