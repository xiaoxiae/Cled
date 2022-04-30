using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The type of the highlight.
/// </summary>
public enum HighlightType
{
    /// <summary>
    /// A main highlighted object.
    /// </summary>
    Main,
    
    /// <summary>
    /// A secondarily highlighted object.
    /// </summary>
    Secondary,
    
    /// <summary>
    /// A transparent object.
    /// </summary>
    Transparent,
}

/// <summary>
/// A script for working with managing highlighted things.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;

    private readonly Dictionary<GameObject, HighlightType> _currentlyHighlighted = new();

    /// <summary>
    /// Return true if the given object is highlighted.
    /// </summary>
    public bool IsHighlighted(GameObject obj) => _currentlyHighlighted.ContainsKey(obj);

    /// <summary>
    /// Highlight an entire route, possibly de-highlighting holds outside it.
    /// </summary>
    public void HighlightRoute(Route route, bool fadeOtherHolds = false)
    {
        foreach (var hold in route.Holds)
            Highlight(hold, HighlightType.Secondary);

        if (!fadeOtherHolds) return;

        foreach (var hold in holdStateManager.PlacedHolds)
            if (!route.ContainsHold(hold))
                Highlight(hold, HighlightType.Transparent);
    }

    /// <summary>
    /// Set the opacity of a hold to the given value.
    /// </summary>
    private static void SetHoldOpacity(GameObject hold, float opacity)
    {
        var renderer = hold.transform.GetChild(0).GetComponent<Renderer>();
        
        Utilities.SetRendererOpacity(renderer, opacity);
        
        // TODO: this is super dirty (2nd children of holds are only markers in this version)
        // set opacity of marker too
        if (hold.transform.childCount == 2)
        {
            var renderer2 = hold.transform.GetChild(1).transform.GetChild(0).GetComponent<MeshRenderer>();
            Utilities.SetRendererOpacity(renderer2, opacity);
        }
    }

    /// <summary>
    /// Highlight the given object.
    /// If it is already, only change the highlighting to main from others.
    /// </summary>
    public void Highlight(GameObject obj, HighlightType highlightType)
    {
        if (IsHighlighted(obj))
        {
            if (_currentlyHighlighted[obj] == highlightType)
                return;

            // highlight only if we're going up in highlights
            if (!(_currentlyHighlighted[obj] == HighlightType.Transparent && highlightType == HighlightType.Main ||
                _currentlyHighlighted[obj] == HighlightType.Secondary && highlightType == HighlightType.Main))
                return;
        }

        _currentlyHighlighted[obj] = highlightType;

        Outline outline;
        switch (highlightType)
        {
            case HighlightType.Main:
                outline = obj.GetOrAddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                outline.OutlineColor = Color.white;
                outline.OutlineWidth = 3.5f;
                outline.UpdateMaterialProperties();
                SetHoldOpacity(obj, 1f);
                break;
            case HighlightType.Secondary:
                outline = obj.GetOrAddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                outline.OutlineColor = Color.grey;
                outline.OutlineWidth = 3.5f;
                outline.UpdateMaterialProperties();
                SetHoldOpacity(obj, 1f);
                break;
            case HighlightType.Transparent:
                SetHoldOpacity(obj, 0.3f);
                break;
        }
    }

    /// <summary>
    /// Unhighlight the given object (if it's not already).
    /// </summary>
    public void Unhighlight(GameObject obj)
    {
        if (!IsHighlighted(obj)) return;

        _currentlyHighlighted.Remove(obj);
        DestroyImmediate(obj.GetComponent<Outline>());

        SetHoldOpacity(obj, 1.0f);
    }

    /// <summary>
    /// Unhighlight all currently highlighted holds.
    /// Optionally only unhighlight those with a specific mode.
    /// </summary>
    public void UnhighlightAll(HighlightType? highlightType = null)
    {
        // .ToList() is used since we're modifying the collection
        foreach (GameObject obj in _currentlyHighlighted.Keys.ToList())
            if (highlightType == null || highlightType.Value == _currentlyHighlighted[obj])
                if (obj)
                    Unhighlight(obj);
    }
}