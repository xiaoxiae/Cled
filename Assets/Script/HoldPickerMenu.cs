using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Button = UnityEngine.UIElements.Button;

public class HoldPickerMenu : MonoBehaviour
{
    // manager-related things
    public PauseMenu pauseMenu;
    public PopupMenu popupMenu;
    public HoldLoader holdLoader;

    public EditorModeManager editorModeManager;
    public HoldStateManager holdStateManager;

    // a dictionary for storing each visual element's state
    private readonly Dictionary<VisualElement, bool> _gridStateDictionary = new();

    // a dictionary for mapping hold blueprints to grid tiles
    private readonly Dictionary<HoldBlueprint, VisualElement> _holdToGridDictionary = new();
    private readonly Dictionary<VisualElement, HoldBlueprint> _gridToHoldDictionary = new();

    // a dictionary for storing the previous hold textures so we don't keep loading more
    private readonly Dictionary<VisualElement, Texture2D> _gridTextureDictionary = new();

    // store all holds
    private HoldBlueprint[] _allHolds = { };

    // and the filtered ones
    private HoldBlueprint[] _filteredHoldIDs = { };

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

    // hacks
    private const long UIBugWorkaroundDurations = 100;
    private long _doubleTriggerTimestamp;

    // styling
    // TODO: this should be moved to the USS file
    private readonly StyleColor _selectedBorderColor = new(new Color(1f, 1f, 1f));
    private readonly StyleColor _pickedBorderColor = new(new Color(0f, 0.5f, 0.4f));
    private readonly StyleColor _deselectedBorderColor = new(new Color(0.25f, 0.25f, 0.25f));

    public RenderTexture renderTexture;
    public VideoPlayer videoPlayer;

    private const string NoSelectionString = "-";

    public HoldBlueprint CurrentlySelectedHold { get; private set; }

    /// <summary>
    /// Return the currently picked holds, in the order they're displayed in the menu.
    /// </summary>
    public List<HoldBlueprint> GetPickedHolds() =>
        OrderBlueprintsToGrid(_holdToGridDictionary.Keys.Where(x => _gridStateDictionary[_holdToGridDictionary[x]])
            .ToArray()).ToList();

    /// <summary>
    /// Update the grid according to the dropdown button filters.
    /// </summary>
    private void UpdateGrid()
    {
        FillGrid(holdLoader.Filter(hold =>
        {
            if (!string.IsNullOrWhiteSpace(_colorDropdown.value) &&
                _colorDropdown.value != NoSelectionString && hold.colorName != _colorDropdown.value)
                return false;

            if (!string.IsNullOrWhiteSpace(_labelsDropdown.value) &&
                _labelsDropdown.value != NoSelectionString && hold.labels.Contains(_labelsDropdown.value))
                return false;

            if (!string.IsNullOrWhiteSpace(_manufacturerDropdown.value) &&
                _manufacturerDropdown.value != NoSelectionString && hold.manufacturer != _manufacturerDropdown.value)
                return false;

            if (!string.IsNullOrWhiteSpace(_typeDropdown.value) &&
                _typeDropdown.value != NoSelectionString && hold.type != _typeDropdown.value)
                return false;

            return true;
        }));

        UpdateSelectCounters();
    }

    /// <summary>
    /// Close the hold picker menu.
    /// </summary>
    public void Close()
    {
        _root.visible = false;
        pauseMenu.UnpauseType(PauseType.HoldPicker);
    }

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.visible = false;

        Utilities.DisableElementFocusable(_root);

        _videoBackground = new StyleBackground(Background.FromRenderTexture(renderTexture));

        _grid = _root.Q<VisualElement>("hold-grid");

        _deselectAllButton = _root.Q<Button>("deselect-all-button");
        _deselectAllButton.clicked += () =>
        {
            foreach (var bp in _gridStateDictionary.Keys.ToList())
                Deselect(bp);
        };

        _totalHoldCounter = _root.Q<Label>("total-hold-counter");

        _filteredHoldCounter = _root.Q<Label>("filtered-hold-counter");

        _colorDropdown = _root.Q<DropdownField>("color-dropdown");
        _labelsDropdown = _root.Q<DropdownField>("labels-dropdown");
        _manufacturerDropdown = _root.Q<DropdownField>("manufacturer-dropdown");
        _typeDropdown = _root.Q<DropdownField>("type-dropdown");

        _totalSelectedHoldCounter = _root.Q<Label>("total-selected-hold-counter");
        _filteredSelectedHoldCounter = _root.Q<Label>("filtered-selected-hold-counter");

        _totalHoldCounter.text = holdLoader.HoldCount.ToString();
    }

    /// <summary>
    /// Initialize the hold picker menu using the hold loader data.
    /// </summary>
    public void Initialize()
    {
        _allHolds = holdLoader.Filter(_ => true);

        // dropdowns
        var dropdowns = new[] { _colorDropdown, _typeDropdown, _labelsDropdown, _manufacturerDropdown };
        var choiceFunctions = new Func<List<string>>[]
            { holdLoader.AllColors, holdLoader.AllTypes, holdLoader.AllLabels, holdLoader.AllManufacturers };

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

        // create a visual element for each hold
        foreach (HoldBlueprint blueprint in _allHolds)
        {
            var item = new VisualElement();

            _holdToGridDictionary[blueprint] = item;
            _gridToHoldDictionary[item] = blueprint;

            _gridStateDictionary[item] = false;

            item.styleSheets.Add(globalStyleSheets);
            item.AddToClassList("hold-picker-hold");

            item.RegisterCallback<MouseOverEvent>(evt =>
            {
                videoPlayer.url = blueprint.PreviewVideoPath;

                renderTexture.DiscardContents();
                Graphics.Blit(_gridTextureDictionary[item], renderTexture);

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

        // done like this to prevent issue where the border is not visible
        foreach (HoldBlueprint blueprint in _allHolds)
        {
            Select(_holdToGridDictionary[blueprint]);
            Deselect(_holdToGridDictionary[blueprint]);
        }
    }

    /// <summary>
    /// Update the counters that change when filtered holds are changed.
    /// </summary>
    private void UpdateFilterCounters() => _filteredHoldCounter.text = _filteredHoldIDs.Length.ToString();

    /// <summary>
    /// Update the counters that change when filtered holds are selected/deselected.
    /// </summary>
    private void UpdateSelectCounters()
    {
        _totalSelectedHoldCounter.text =
            _gridStateDictionary.Values.Count(value => value).ToString();

        _filteredSelectedHoldCounter.text =
            _filteredHoldIDs.Count(value => _gridStateDictionary[_holdToGridDictionary[value]]).ToString();
    }

    /// <summary>
    /// Select the hold.
    /// </summary>
    public void Select(HoldBlueprint blueprint, bool switchHeld = true) =>
        Select(_holdToGridDictionary[blueprint], switchHeld);

    /// <summary>
    /// Select a grid element.
    /// </summary>
    private void Select(VisualElement item, bool switchHeld = true)
    {
        HoldBlueprint blueprint = _gridToHoldDictionary[item];

        if (switchHeld)
        {
            CurrentlySelectedHold = blueprint;

            // if we selected a new hold and are holding another one, hold this one instead
            if (editorModeManager.CurrentMode == EditorModeManager.Mode.Holding)
                holdStateManager.InstantiateToHolding(CurrentlySelectedHold, true);
        }

        // do nothing if it is already selected
        if (_gridStateDictionary[item])
            return;

        _gridStateDictionary[item] = true;

        UpdateItemBorders();

        UpdateSelectCounters();
    }

    private void UpdateItemBorders()
    {
        var pickedHolds = GetPickedHolds();

        foreach (var hold in _allHolds)
        {
            var holdItem = _holdToGridDictionary[hold];

            if (hold == CurrentlySelectedHold)
            {
                holdItem.style.borderBottomColor = _pickedBorderColor;
                holdItem.style.borderTopColor = _pickedBorderColor;
                holdItem.style.borderLeftColor = _pickedBorderColor;
                holdItem.style.borderRightColor = _pickedBorderColor;
            }
            else if (pickedHolds.Contains(hold))
            {
                holdItem.style.borderBottomColor = _selectedBorderColor;
                holdItem.style.borderTopColor = _selectedBorderColor;
                holdItem.style.borderLeftColor = _selectedBorderColor;
                holdItem.style.borderRightColor = _selectedBorderColor;
            }
            else
            {
                holdItem.style.borderBottomColor = _deselectedBorderColor;
                holdItem.style.borderTopColor = _deselectedBorderColor;
                holdItem.style.borderLeftColor = _deselectedBorderColor;
                holdItem.style.borderRightColor = _deselectedBorderColor;
            }
        }
    }

    /// <summary>
    /// Deselect a grid element.
    /// </summary>
    private void Deselect(VisualElement item)
    {
        HoldBlueprint blueprint = _gridToHoldDictionary[item];

        if (blueprint == CurrentlySelectedHold)
        {
            // switch to another hold if there are still some leftover
            if (GetPickedHolds().Count != 1)
                MoveByDelta(-1);
            else
                CurrentlySelectedHold = null;

            // if we deselected the currently held hold, stop holding it
            if (editorModeManager.CurrentMode == EditorModeManager.Mode.Holding)
            {
                editorModeManager.CurrentMode = EditorModeManager.Mode.Normal;
                holdStateManager.StopHolding();
            }
        }

        // do nothing if it is already deselected
        if (!_gridStateDictionary[item])
            return;

        _gridStateDictionary[item] = false;

        UpdateItemBorders();

        UpdateSelectCounters();
    }

    /// <summary>
    /// Toggle the selection of the grid element.
    ///
    /// Additionally, make sure that when a new item is selected, make it the currently picked hold
    /// and when an item is deselected, if it was the currently picked hold, remove it.
    /// </summary>
    private void ToggleSelect(VisualElement item)
    {
        if (_gridStateDictionary[item])
            Deselect(item);
        else
            Select(item);
    }

    /// <summary>
    /// Clear the hold grid.
    /// </summary>
    private void ClearGrid()
    {
        foreach (var blueprint in _filteredHoldIDs)
            _grid.Remove(_holdToGridDictionary[blueprint]);
    }

    /// <summary>
    /// Order the given blueprints the way they will be ordered in the grid.
    /// This means first by color and then by volume.
    /// </summary>
    private IOrderedEnumerable<HoldBlueprint> OrderBlueprintsToGrid(HoldBlueprint[] blueprints) =>
        blueprints.OrderBy(x => x.holdMetadata.colorHex)
            .ThenBy(x => x.holdMetadata.volume);

    /// <summary>
    /// Fill the grid with a selection of the holds.
    /// </summary>
    private void FillGrid(HoldBlueprint[] holdBlueprints)
    {
        ClearGrid();

        foreach (var blueprint in OrderBlueprintsToGrid(holdBlueprints))
            _grid.Add(_holdToGridDictionary[blueprint]);

        _filteredHoldIDs = holdBlueprints;

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
        // CTRL+A selects all filtered holds (when the holdpicker is open)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A) &&
            pauseMenu.IsTypePaused(PauseType.HoldPicker))
            foreach (var hold in _filteredHoldIDs)
                Select(_holdToGridDictionary[hold], false);

        // only open the hold menu if some holds were actually loaded in
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab))
        {
            // don't open it when popups or route settings are present
            if (pauseMenu.IsTypePaused(PauseType.Popup) || pauseMenu.IsTypePaused(PauseType.RouteSettings))
                return;

            // if there are no holds, don't show it at all
            if (holdLoader.HoldCount == 0)
            {
                popupMenu.CreateInfoPopup("No holds loaded, nothing to filter.");
                return;
            }

            if (pauseMenu.IsTypePaused(PauseType.HoldPicker))
            {
                Close();
            }
            else
            {
                _root.visible = true;
                pauseMenu.PauseType(PauseType.HoldPicker);
            }

            // TODO: while this does fix it, it is pretty buggy
            // Input.ResetInputAxes();
        }
    }

    /// <summary>
    /// Clear the hold picker menu, destroying the hold textures in the process.
    /// </summary>
    public void Clear()
    {
        ClearGrid();

        _allHolds = new HoldBlueprint[] { };

        _filteredHoldIDs = new HoldBlueprint[] { };
        _gridStateDictionary.Clear();
        _holdToGridDictionary.Clear();
        _gridToHoldDictionary.Clear();

        foreach (var texture in _gridTextureDictionary.Values)
            Destroy(texture);

        _gridTextureDictionary.Clear();

        Close();
    }

    /// <summary>
    /// Move to the previous filtered hold.
    /// </summary>
    public void MoveToPreviousHold() => MoveByDelta(-1);

    /// <summary>
    /// Move to the next filtered hold.
    /// </summary>
    public void MoveToNextHold() => MoveByDelta(1);

    private void MoveByDelta(int delta)
    {
        var pickedHolds = GetPickedHolds();

        int selectedIndex = pickedHolds.IndexOf(CurrentlySelectedHold);
        int newIndex = (selectedIndex + delta + pickedHolds.Count) % pickedHolds.Count;

        CurrentlySelectedHold = pickedHolds[newIndex];

        UpdateItemBorders();
    }
}