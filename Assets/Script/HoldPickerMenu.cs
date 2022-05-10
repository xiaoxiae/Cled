using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

/// <summary>
///     The controller for the hold picker menu.
/// </summary>
public class HoldPickerMenu : MonoBehaviour, IClosable, IAcceptable, IResetable
{
    // hacks
    private const long UIBugWorkaroundDurations = 100;

    private const string NoSelectionString = "-";

    // manager-related things
    public PauseMenu pauseMenu;
    public PopupMenu popupMenu;
    public HoldLoader holdLoader;

    public EditorModeManager editorModeManager;
    public HoldStateManager holdStateManager;

    public StyleSheet globalStyleSheets;

    public RenderTexture renderTexture;
    public VideoPlayer videoPlayer;

    private readonly List<Action> _addedChangedCallbacks = new();
    private readonly StyleColor _deselectedBorderColor = new(new Color(0.25f, 0.25f, 0.25f));

    // a dictionary for storing each visual element's state
    private readonly Dictionary<VisualElement, bool> _gridStateDictionary = new();
    private readonly Dictionary<VisualElement, HoldBlueprint> _gridToHoldDictionary = new();
    private readonly StyleColor _pickedBorderColor = new(new Color(0f, 0.5f, 0.4f));

    // styling
    // TODO: this should be moved to the USS file
    private readonly StyleColor _selectedBorderColor = new(new Color(1f, 1f, 1f));

    // a dictionary for storing the previous hold textures so we don't keep loading more
    // is again public because Bottom bar uses it
    public readonly Dictionary<VisualElement, Texture2D> GridTextureDictionary = new();

    // a dictionary for mapping hold blueprints to grid tiles
    // the HoldToGrid one is public, because Bottom bar uses it
    public readonly Dictionary<HoldBlueprint, VisualElement> HoldToGridDictionary = new();

    private DropdownField _colorDropdown;

    private Button _deselectAllButton;
    private Button _deselectFilteredButton;
    private long _doubleTriggerTimestamp;

    private Label _filteredHoldCounter;

    // store filtered holds
    private List<HoldBlueprint> _filteredHoldIDs = new();
    private VisualElement _grid;
    private DropdownField _labelsDropdown;
    private DropdownField _manufacturerDropdown;

    // UI elements
    private VisualElement _root;
    private Label _totalSelectedHoldCounter;
    private DropdownField _typeDropdown;

    private StyleBackground _videoBackground;

    public HoldBlueprint CurrentlySelectedHold { get; private set; }

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.visible = false;

        _videoBackground = new StyleBackground(Background.FromRenderTexture(renderTexture));

        _grid = _root.Q<VisualElement>("hold-grid");

        _deselectAllButton = _root.Q<Button>("deselect-all-button");
        _deselectAllButton.clicked += () =>
        {
            foreach (var bp in _gridStateDictionary.Keys.ToList())
                Deselect(bp);
        };

        _filteredHoldCounter = _root.Q<Label>("filtered-hold-counter");

        _colorDropdown = _root.Q<DropdownField>("color-dropdown");
        _labelsDropdown = _root.Q<DropdownField>("labels-dropdown");
        _manufacturerDropdown = _root.Q<DropdownField>("manufacturer-dropdown");
        _typeDropdown = _root.Q<DropdownField>("type-dropdown");

        _totalSelectedHoldCounter = _root.Q<Label>("total-selected-hold-counter");

        AddChangedCallback(UpdateItemBorders);
        AddChangedCallback(UpdateSelectCounters);
        AddChangedCallback(UpdateFilterCounters);
    }

    private void Update()
    {
        // CTRL+A selects all filtered holds (when the holdpicker is open)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A) &&
            pauseMenu.IsTypePaused(PauseType.HoldPicker))
        {
            // if all of the holds are selected, deselect them instead
            if (_filteredHoldIDs.Count == GetPickedHolds().Count)
                foreach (var hold in OrderBlueprintsToGrid(_filteredHoldIDs))
                    Deselect(HoldToGridDictionary[hold]);
            else
                foreach (var hold in OrderBlueprintsToGrid(_filteredHoldIDs))
                    Select(HoldToGridDictionary[hold], false);
        }

        // only open the hold menu if some holds were actually loaded in
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab))
            ToggleOpen();
    }

    public void Accept()
    {
        Close();
    }

    public void Close()
    {
        _root.visible = false;
        pauseMenu.UnpauseType(PauseType.HoldPicker);
    }

    /// <summary>
    ///     Clear the hold picker menu, destroying the hold textures in the process.
    /// </summary>
    public void Reset()
    {
        ClearGrid();

        _filteredHoldIDs.Clear();
        _gridStateDictionary.Clear();
        HoldToGridDictionary.Clear();
        _gridToHoldDictionary.Clear();

        foreach (var texture in GridTextureDictionary.Values)
            Destroy(texture);
        GridTextureDictionary.Clear();

        Changed();
        Close();
    }

    /// <summary>
    ///     Return the currently picked holds, in the order they're displayed in the menu.
    /// </summary>
    public List<HoldBlueprint> GetPickedHolds()
    {
        return OrderBlueprintsToGrid(HoldToGridDictionary.Keys.Where(x => _gridStateDictionary[HoldToGridDictionary[x]])
            .ToArray()).ToList();
    }

    /// <summary>
    ///     Update the grid according to the dropdown button filters.
    ///     Additionally, deselect all of the holds that are no longer filtered
    /// </summary>
    private void UpdateGrid()
    {
        var holds = holdLoader.Filter(hold =>
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
        });

        foreach (var hold in holdLoader.Holds)
            if (!holds.Contains(hold))
                Deselect(HoldToGridDictionary[hold]);

        FillGrid(holds);

        Changed();
    }

    /// <summary>
    ///     Initialize the hold picker menu using the hold loader data.
    /// </summary>
    public void Initialize()
    {
        // dropdowns
        var dropdowns = new[] { _colorDropdown, _typeDropdown, _labelsDropdown, _manufacturerDropdown };
        var choiceFunctions = new Func<IEnumerable<string>>[]
            { holdLoader.AllColors, holdLoader.AllTypes, holdLoader.AllLabels, holdLoader.AllManufacturers };

        for (var i = 0; i < dropdowns.Length; i++)
        {
            var allValues = choiceFunctions[i]().ToList();
            allValues.Insert(0, NoSelectionString);

            // only add the separator if there are some items
            if (allValues.Count != 1)
                allValues.Insert(1, "");

            dropdowns[i].choices = allValues;

            dropdowns[i].index = 0;
            dropdowns[i].RegisterValueChangedCallback(_ => UpdateGrid());
        }

        // create a visual element for each hold
        foreach (var blueprint in holdLoader.Holds)
        {
            var item = new VisualElement();

            HoldToGridDictionary[blueprint] = item;
            _gridToHoldDictionary[item] = blueprint;

            _gridStateDictionary[item] = false;

            item.styleSheets.Add(globalStyleSheets);
            item.AddToClassList("hold-picker-hold");

            item.RegisterCallback<MouseOverEvent>(evt =>
            {
                videoPlayer.url = blueprint.PreviewVideoPath;

                renderTexture.DiscardContents();
                Graphics.Blit(GridTextureDictionary[item], renderTexture);

                item.style.backgroundImage = _videoBackground;
            });

            item.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                item.style.backgroundImage = GridTextureDictionary[item];
            });

            item.RegisterCallback<ClickEvent>(evt =>
            {
                // TODO: this is a hack for a bug in Linux Unity
                // https://forum.unity.com/threads/registercallback-clickevent-triggers-twice-on-linux-only.1111474/
                if (evt.timestamp - _doubleTriggerTimestamp > UIBugWorkaroundDurations)
                    ToggleSelect(item);

                _doubleTriggerTimestamp = evt.timestamp;
            });

            GridTextureDictionary[item] = LoadTexture(blueprint.PreviewImagePath);

            item.style.backgroundImage =
                new StyleBackground(Background.FromTexture2D(GridTextureDictionary[item]));
        }

        FillGrid(holdLoader.Holds);

        // done like this to prevent issue where the border is not visible
        foreach (var blueprint in holdLoader.Holds)
        {
            Select(HoldToGridDictionary[blueprint]);
            Deselect(HoldToGridDictionary[blueprint]);
        }

        var totalHoldCounter = _root.Q<Label>("total-hold-counter");
        totalHoldCounter.text = holdLoader.HoldCount.ToString();
    }

    /// <summary>
    ///     Update the counters that change when filtered holds are changed.
    /// </summary>
    private void UpdateFilterCounters()
    {
        _filteredHoldCounter.text = _filteredHoldIDs.Count.ToString();
    }

    /// <summary>
    ///     Update the counters that change when filtered holds are selected/deselected.
    /// </summary>
    private void UpdateSelectCounters()
    {
        _totalSelectedHoldCounter.text =
            _gridStateDictionary.Values.Count(value => value).ToString();
    }

    /// <summary>
    ///     Select the hold.
    /// </summary>
    public void Select(HoldBlueprint blueprint, bool switchHeld = true)
    {
        Select(HoldToGridDictionary[blueprint], switchHeld);
    }

    /// <summary>
    ///     Select a grid element.
    /// </summary>
    private void Select(VisualElement item, bool switchHeld = true)
    {
        var blueprint = _gridToHoldDictionary[item];

        // do switch held if there currently are no selected holds
        if (switchHeld || CurrentlySelectedHold == null)
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

        Changed();
    }

    /// <summary>
    ///     Update the borders of the items according to whether they're selected or not.
    /// </summary>
    private void UpdateItemBorders()
    {
        var pickedHolds = GetPickedHolds();

        foreach (var hold in holdLoader.Holds)
        {
            var holdItem = HoldToGridDictionary[hold];

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
    ///     Deselect a grid element.
    /// </summary>
    private void Deselect(VisualElement item)
    {
        var blueprint = _gridToHoldDictionary[item];

        if (blueprint == CurrentlySelectedHold)
        {
            // switch to another hold if there are still some leftover
            if (GetPickedHolds().Count != 1)
                MoveCurrentByDelta(-1);
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

        Changed();
    }

    /// <summary>
    ///     Toggle the selection of the grid element.
    ///     Additionally, make sure that when a new item is selected, make it the currently picked hold
    ///     and when an item is deselected, if it was the currently picked hold, remove it.
    /// </summary>
    private void ToggleSelect(VisualElement item)
    {
        if (_gridStateDictionary[item])
            Deselect(item);
        else
            Select(item);
    }

    /// <summary>
    ///     Clear the hold grid.
    /// </summary>
    private void ClearGrid()
    {
        foreach (var blueprint in _filteredHoldIDs)
            _grid.Remove(HoldToGridDictionary[blueprint]);
    }

    /// <summary>
    ///     Order the given blueprints the way they will be ordered in the grid.
    ///     This means first by color and then by volume.
    /// </summary>
    private IOrderedEnumerable<HoldBlueprint> OrderBlueprintsToGrid(IEnumerable<HoldBlueprint> blueprints)
    {
        return blueprints.OrderBy(x => x.holdMetadata.colorHex)
            .ThenBy(x => x.holdMetadata.volume);
    }

    /// <summary>
    ///     Fill the grid with a selection of the holds.
    /// </summary>
    private void FillGrid(IEnumerable<HoldBlueprint> holdBlueprints)
    {
        ClearGrid();

        foreach (var blueprint in OrderBlueprintsToGrid(holdBlueprints))
            _grid.Add(HoldToGridDictionary[blueprint]);

        _filteredHoldIDs = holdBlueprints.ToList();
    }

    /// <summary>
    ///     Load a texture from the disk.
    /// </summary>
    private static Texture2D LoadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        var fileData = File.ReadAllBytes(filePath);
        var tex2D = new Texture2D(2, 2); // dimensions don't matter, LoadImage sets them

        return tex2D.LoadImage(fileData) ? tex2D : null;
    }

    public void ToggleOpen()
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

    /// <summary>
    ///     Add a callback for when the filtered/selected holds were changed.
    /// </summary>
    public void AddChangedCallback(Action callback)
    {
        _addedChangedCallbacks.Add(callback);
    }

    /// <summary>
    ///     Called when the filtered/selected holds change to invoke their callbacks.
    /// </summary>
    private void Changed()
    {
        foreach (var callback in _addedChangedCallbacks)
            callback();
    }


    /// <summary>
    ///     Return the hold before the current one.
    /// </summary>
    public HoldBlueprint GetPreviousHold()
    {
        return GetFromCurrentByDelta(-1);
    }

    /// <summary>
    ///     Return the hold after the current one.
    /// </summary>
    public HoldBlueprint GetNextHold()
    {
        return GetFromCurrentByDelta(1);
    }

    /// <summary>
    ///     Move to the previous filtered hold.
    /// </summary>
    public void MoveToPreviousHold()
    {
        MoveCurrentByDelta(-1);
    }

    /// <summary>
    ///     Move to the next filtered hold.
    /// </summary>
    public void MoveToNextHold()
    {
        MoveCurrentByDelta(1);
    }

    /// <summary>
    ///     Get the hold off by delta to the current one.
    /// </summary>
    private HoldBlueprint GetFromCurrentByDelta(int delta)
    {
        var pickedHolds = GetPickedHolds();

        if (pickedHolds.Count == 0)
            return null;

        var selectedIndex = pickedHolds.IndexOf(CurrentlySelectedHold);
        var newIndex = (selectedIndex + delta + pickedHolds.Count) % pickedHolds.Count;

        return pickedHolds[newIndex];
    }

    /// <summary>
    ///     Move the current hold by delta units.
    /// </summary>
    private void MoveCurrentByDelta(int delta)
    {
        CurrentlySelectedHold = GetFromCurrentByDelta(delta);
        Changed();
    }
}