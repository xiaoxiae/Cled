using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Button = UnityEngine.UIElements.Button;
using Cursor = UnityEngine.Cursor;

public class HoldPickerManager : MonoBehaviour
{
    // a dictionary for storing each visual element's state
    private readonly Dictionary<VisualElement, bool> _gridStateDictionary = new();

    // a dictionary for mapping hold blueprints to grid tiles
    private readonly Dictionary<HoldBlueprint, VisualElement> _holdToGridDictionary = new();

    // a dictionary for storing the previous hold textures so we don't keep loading more
    private readonly Dictionary<VisualElement, Texture2D> _gridTextureDictionary = new();

    // UI elements
    private VisualElement _root;
    private VisualElement _grid;

    private Button _deselectAllButton;
    private Button _deselectFilteredButton;

    private Label _totalHoldCounter;
    private Label _filteredHoldCounter;

    private Label _totalSelectedHoldCounter;
    private Label _filteredSelectedHoldCounter;

    private DropdownField _colorDropdown;
    private DropdownField _typeDropdown;
    private DropdownField _labelsDropdown;
    private DropdownField _manufacturerDropdown;

    public StyleSheet globalStyleSheets;

    private StyleBackground _videoBackground;

    // manager-related things
    public PauseManager pauseManager;

    public HoldManager HoldManager;

    private HoldBlueprint[] _allHolds;
    private HoldBlueprint[] _currentlyFilteredHolds;

    // hacks
    private const long UIBugWorkaroundDurations = 100;
    private long _doubleTriggerTimestamp;

    // styling
    // TODO: this should be moved to the USS file
    private readonly StyleColor _selectedBorderColor = new(new Color(1f, 1f, 1f));
    private readonly StyleColor _deselectedBorderColor = new(new Color(0.35f, 0.35f, 0.35f));

    public RenderTexture RenderTexture;
    public VideoPlayer VideoPlayer;

    private const string NoSelectionString = "-";

    /// <summary>
    /// Return the currently picked holds.
    /// </summary>
    public List<HoldBlueprint> GetPickedHolds() =>
        _holdToGridDictionary.Keys.Where(x => _gridStateDictionary[_holdToGridDictionary[x]]).ToList();

    /// <summary>
    /// Update the grid according to the dropdown button filters.
    /// </summary>
    private void UpdateGrid()
    {
        FillGrid(HoldManager.Filter(hold =>
        {
            if (!string.IsNullOrWhiteSpace(_colorDropdown.value) && _colorDropdown.value != NoSelectionString &&
                hold.colorName != _colorDropdown.value)
                return false;

            if (!string.IsNullOrWhiteSpace(_labelsDropdown.value) && _labelsDropdown.value != NoSelectionString &&
                hold.labels.Contains(_labelsDropdown.value))
                return false;

            if (!string.IsNullOrWhiteSpace(_manufacturerDropdown.value) &&
                _manufacturerDropdown.value != NoSelectionString && hold.manufacturer != _manufacturerDropdown.value)
                return false;

            if (!string.IsNullOrWhiteSpace(_typeDropdown.value) &&
                _typeDropdown.value != NoSelectionString && hold.manufacturer != _typeDropdown.value)
                return false;

            return true;
        }));

        UpdateSelectCounters();
    }

    public void Close()
    {
        _root.visible = false;
        pauseManager.Unpause(PauseType.HoldPicker);
    }

    void Start()
    {
        var document = GetComponent<UIDocument>();
        document.sortingOrder = 5;
        
        _root = document.rootVisualElement;
        _root.visible = false;

        _videoBackground = new StyleBackground(Background.FromRenderTexture(RenderTexture));

        _grid = _root.Q<VisualElement>("hold-grid");

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

        _colorDropdown = _root.Q<DropdownField>("color-dropdown");
        _labelsDropdown = _root.Q<DropdownField>("labels-dropdown");
        _manufacturerDropdown = _root.Q<DropdownField>("manufacturer-dropdown");
        _typeDropdown = _root.Q<DropdownField>("type-dropdown");

        _totalSelectedHoldCounter = _root.Q<Label>("total-selected-hold-counter");
        _filteredSelectedHoldCounter = _root.Q<Label>("filtered-selected-hold-counter");
        _allHolds = HoldManager.Filter(_ => true);

        _totalHoldCounter.text = HoldManager.HoldCount.ToString();

        // dropdowns
        var dropdowns = new[] { _colorDropdown, _typeDropdown, _labelsDropdown, _manufacturerDropdown };
        var choiceFunctions = new Func<List<string>>[]
            { HoldManager.AllColors, HoldManager.AllTypes, HoldManager.AllLabels, HoldManager.AllManufacturers };

        for (int i = 0; i < dropdowns.Length; i++)
        {
            var allValues = choiceFunctions[i]();
            allValues.Insert(0, NoSelectionString);

            // only add the separator if there are some items
            if (allValues.Count != 1)
                allValues.Insert(1, "");

            dropdowns[i].choices = allValues;

            dropdowns[i].index = 0;
            dropdowns[i].RegisterValueChangedCallback(_ => UpdateGrid());
        }

        _currentlyFilteredHolds = new HoldBlueprint[] { };

        // create a visual element for each hold
        foreach (HoldBlueprint blueprint in _allHolds)
        {
            var item = new VisualElement();

            _holdToGridDictionary[blueprint] = item;
            _gridStateDictionary[item] = false;

            Deselect(item);

            item.styleSheets.Add(globalStyleSheets);
            item.AddToClassList("hold-picker-hold");

            item.RegisterCallback<MouseOverEvent>(evt =>
            {
                VideoPlayer.url = blueprint.PreviewVideoPath;

                RenderTexture.DiscardContents();
                Graphics.Blit(_gridTextureDictionary[item], RenderTexture);

                item.style.backgroundImage = _videoBackground;
            });

            item.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                item.style.backgroundImage = _gridTextureDictionary[item];
            });

            item.RegisterCallback<ClickEvent>(evt =>
            {
                // TODO: this is a hack for a bug in Linux Unity
                // https://forum.unity.com/threads/registercallback-clickevent-triggers-twice-on-linux-only.1111474/
                if (evt.timestamp - _doubleTriggerTimestamp > UIBugWorkaroundDurations)
                    ToggleSelect(item);

                _doubleTriggerTimestamp = evt.timestamp;
            });

            _gridTextureDictionary[item] = LoadTexture(blueprint.PreviewImagePath);

            item.style.backgroundImage =
                new StyleBackground(Background.FromTexture2D(_gridTextureDictionary[item]));
        }

        FillGrid(_allHolds);
    }


    /// <summary>
    /// Update the counters that change when filtered holds are changed.
    /// </summary>
    private void UpdateFilterCounters() => _filteredHoldCounter.text = _currentlyFilteredHolds.Length.ToString();

    /// <summary>
    /// Update the counters that change when filtered holds are selected/deselected.
    /// </summary>
    private void UpdateSelectCounters()
    {
        _totalSelectedHoldCounter.text =
            _gridStateDictionary.Values.Count(value => value).ToString();

        _filteredSelectedHoldCounter.text =
            _currentlyFilteredHolds.Count(value => _gridStateDictionary[_holdToGridDictionary[value]]).ToString();
    }

    /// <summary>
    /// Select a grid element.
    /// </summary>
    private void Select(VisualElement item)
    {
        // do nothing if it is already selected
        if (_gridStateDictionary[item])
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
    private void Deselect(VisualElement item)
    {
        // do nothing if it is already deselected
        if (!_gridStateDictionary[item])
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
    private void ToggleSelect(VisualElement item)
    {
        if (_gridStateDictionary[item])
            Deselect(item);
        else
            Select(item);
    }

    /// <summary>
    /// Fill the grid with a selection of the holds.
    /// </summary>
    private void FillGrid(HoldBlueprint[] holdBlueprints)
    {
        foreach (var blueprint in _currentlyFilteredHolds)
            _grid.Remove(_holdToGridDictionary[blueprint]);

        foreach (var blueprint in holdBlueprints
                     .OrderBy(x => x.HoldInformation.colorHex)
                     .ThenBy(x => x.HoldInformation.type))
            _grid.Add(_holdToGridDictionary[blueprint]);

        _currentlyFilteredHolds = holdBlueprints;

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
        // CTRL+A selects all filtered holds
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
            foreach (var hold in _currentlyFilteredHolds)
                Select(_holdToGridDictionary[hold]);
        
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab))
        {
            // don't open it when popups or route settings are present
            if (pauseManager.IsPaused(PauseType.Popup) || pauseManager.IsPaused(PauseType.RouteSettings))
                return;

            if (pauseManager.IsPaused(PauseType.HoldPicker))
            {
                Close();
            }
            else
            {
                _root.visible = true;
                pauseManager.Pause(PauseType.HoldPicker);
            }

            // TODO: while this does fix it, it is pretty buggy
            // Input.ResetInputAxes();
        }
    }
}