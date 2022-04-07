using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class HoldPickerManager : MonoBehaviour
{
    private VisualElement _root;
    private VisualElement _grid;

    private Dictionary<VisualElement, bool> _gridStateDictionary;
    private Dictionary<HoldBlueprint, VisualElement> _holdToGridDictionary;

    private Button _deselectAllButton;
    private Button _deselectFilteredButton;

    private Label _totalHoldCounter;
    private Label _filteredHoldCounter;

    private Label _totalSelectedHoldCounter;
    private Label _filteredSelectedHoldCounter;

    public HoldManager HoldManager;

    private HoldBlueprint[] _currentlyFilteredHolds;

    private long _timestampHackLastPressed;
    private readonly long timestampHackDuration = 100;

    private readonly StyleColor _selectedBorderColor = new(new Color(1f, 1f, 1f));
    private readonly StyleColor _deselectedBorderColor = new(new Color(0.35f, 0.35f, 0.35f));

    private const float BorderThickness = 4.5f;
    private const float GridElementSize = 150;
    private const float GridElementSpacing = 10;
    private const float GridElementBorderRoundness = 10;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.visible = false;

        _grid = _root.Q<VisualElement>("hold-grid");

        _gridStateDictionary = new Dictionary<VisualElement, bool>();
        _holdToGridDictionary = new Dictionary<HoldBlueprint, VisualElement>();

        _deselectFilteredButton = _root.Q<Button>("deselect-filtered-button");
        _deselectFilteredButton.clicked += () =>
        {
            foreach (var bp in _currentlyFilteredHolds) Deselect(_holdToGridDictionary[bp]);
        };

        _deselectAllButton = _root.Q<Button>("deselect-all-button");
        _deselectAllButton.clicked += () =>
        {
            foreach (var bp in _gridStateDictionary.Keys.ToList()) Deselect(bp);
        };

        _totalHoldCounter = _root.Q<Label>("total-hold-counter");

        _filteredHoldCounter = _root.Q<Label>("filtered-hold-counter");

        _totalSelectedHoldCounter = _root.Q<Label>("total-selected-hold-counter");
        _filteredSelectedHoldCounter = _root.Q<Label>("filtered-selected-hold-counter");

        // TODO: this is not very good -- do this when holds are loaded
        Invoke("tmpFill", 1);
    }

    /// <summary>
    /// TODO: all of this somewhere else!
    /// </summary>
    void tmpFill()
    {
        _currentlyFilteredHolds = HoldManager.Filter(_ => true);

        _totalHoldCounter.text = HoldManager.HoldCount.ToString();

        FillGrid(_currentlyFilteredHolds);
    }

    /// <summary>
    /// Update the counters that change when filtered holds are changed.
    /// </summary>
    void UpdateFilterCounters() => _filteredHoldCounter.text = _currentlyFilteredHolds.Length.ToString();

    /// <summary>
    /// Update the counters that change when filtered holds are selected/deselected.
    /// </summary>
    void UpdateSelectCounters()
    {
        _totalSelectedHoldCounter.text =
            _gridStateDictionary.Values.Count(value => value).ToString();
        
        _filteredSelectedHoldCounter.text =
            _currentlyFilteredHolds.Count(value => _gridStateDictionary[_holdToGridDictionary[value]]).ToString();
    }

    /// <summary>
    /// Select a grid element.
    /// </summary>
    void Select(VisualElement item)
    {
        // do nothing if it is already selected
        if (_gridStateDictionary.ContainsKey(item) && _gridStateDictionary[item])
            return;

        item.style.borderBottomColor = _selectedBorderColor;
        item.style.borderTopColor = _selectedBorderColor;
        item.style.borderLeftColor = _selectedBorderColor;
        item.style.borderRightColor = _selectedBorderColor;

        _gridStateDictionary[item] = true;

        UpdateSelectCounters();
    }

    /// <summary>
    /// Deselect a grid element.
    /// </summary>
    void Deselect(VisualElement item)
    {
        // do nothing if it is already deselected
        if (_gridStateDictionary.ContainsKey(item) && !_gridStateDictionary[item])
            return;

        item.style.borderBottomColor = _deselectedBorderColor;
        item.style.borderTopColor = _deselectedBorderColor;
        item.style.borderLeftColor = _deselectedBorderColor;
        item.style.borderRightColor = _deselectedBorderColor;

        _gridStateDictionary[item] = false;

        UpdateSelectCounters();
    }

    /// <summary>
    /// Toggle the selection of the grid element.
    /// </summary>
    void ToggleSelect(VisualElement item)
    {
        if (_gridStateDictionary[item])
            Deselect(item);
        else
            Select(item);
    }

    /// <summary>
    /// Update the status of the grid item (if it was selected/deselected).
    /// Essentially the reverse of toggle.
    /// </summary>
    /// <param name="item"></param>
    void UpdateItemStatus(VisualElement item)
    {
        if (_gridStateDictionary[item])
            Select(item);
        else
            Deselect(item);
    }

    /// <summary>
    /// Fill the grid with a selection of the holds.
    /// </summary>
    void FillGrid(HoldBlueprint[] holdBlueprints)
    {
        foreach (HoldBlueprint blueprint in holdBlueprints)
        {
            var item = new VisualElement();

            _holdToGridDictionary[blueprint] = item;

            item.style.width = GridElementSize;
            item.style.height = GridElementSize;

            item.style.borderBottomWidth = BorderThickness;
            item.style.borderTopWidth = BorderThickness;
            item.style.borderLeftWidth = BorderThickness;
            item.style.borderRightWidth = BorderThickness;

            if (_gridStateDictionary.ContainsKey(item))
                UpdateItemStatus(item);
            else
                Deselect(item);

            item.RegisterCallback<ClickEvent>(evt =>
            {
                // TODO: this is a hack for a bug in Linux Unity
                // https://forum.unity.com/threads/registercallback-clickevent-triggers-twice-on-linux-only.1111474/
                if (evt.timestamp - _timestampHackLastPressed > timestampHackDuration)
                    ToggleSelect(item);

                _timestampHackLastPressed = evt.timestamp;
            });

            item.style.marginBottom = GridElementSpacing;
            item.style.marginLeft = GridElementSpacing;
            item.style.marginRight = GridElementSpacing;
            item.style.marginTop = GridElementSpacing;

            item.style.borderBottomLeftRadius = GridElementBorderRoundness;
            item.style.borderBottomRightRadius = GridElementBorderRoundness;
            item.style.borderTopLeftRadius = GridElementBorderRoundness;
            item.style.borderTopRightRadius = GridElementBorderRoundness;

            item.style.backgroundImage =
                new StyleBackground(Background.FromTexture2D(LoadTexture(blueprint.PreviewImagePath)));

            _grid.Add(item);
        }

        UpdateFilterCounters();
    }

    /// <summary>
    /// Load a texture from the disk.
    /// </summary>
    private static Texture2D LoadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        var fileData = File.ReadAllBytes(filePath);
        var tex2D = new Texture2D(2, 2); // dimensions don't matter, LoadImage sets them

        return tex2D.LoadImage(fileData) ? tex2D : null;
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