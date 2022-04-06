using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class HoldPickerManager : MonoBehaviour
{
    private VisualElement _root;
    private VisualElement _grid;

    public HoldManager HoldManager;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.visible = false;

        _grid = _root.Q<VisualElement>("hold-grid");

        // TODO: this is probably not very good
        Invoke("tmpFill", 1);
    }

    void tmpFill()
    {
        FillGrid(HoldManager.Filter(_ => true));
    }

    /// <summary>
    /// Fill the grid with a selection of the holds.
    /// </summary>
    void FillGrid(HoldBlueprint[] holdBlueprints)
    {
        foreach (HoldBlueprint blueprint in holdBlueprints)
        {
            var item = new VisualElement();
            item.style.width = 220;
            item.style.height = 220;

            item.style.marginBottom = 10;
            item.style.marginLeft = 10;
            item.style.marginRight = 10;
            item.style.marginTop = 10;
            
            item.style.borderBottomLeftRadius = 10;
            item.style.borderBottomRightRadius = 10;
            item.style.borderTopLeftRadius = 10;
            item.style.borderTopRightRadius = 10;

            item.style.backgroundImage =
                new StyleBackground(Background.FromTexture2D(LoadTexture(blueprint.PreviewImagePath)));

            _grid.Add(item);
        }
    }

    /// <summary>
    /// Load a texture from the disk.
    /// </summary>
    /// <param name="FilePath"></param>
    /// <returns></returns>
    private static Texture2D LoadTexture(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            
            if (Tex2D.LoadImage(FileData))
                return Tex2D;
        }

        return null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (Time.timeScale == 0 && _root.visible)
            {
                Time.timeScale = 1;
                _root.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Time.timeScale == 1 && !_root.visible)
            {
                Time.timeScale = 0;
                _root.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}