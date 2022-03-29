using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// How should the highlight look like - whether it's the primary thing to focus on, secondary, or both.
/// </summary>
public enum HighlightType
{
    Primary,
    Secondary,
}

/// <summary>
/// A script for working with managing highlighted things.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, HighlightType> _highlighted = new();

    /// <summary>
    /// Return true if the given object is highlighted.
    /// </summary>
    public bool IsHighlighted(GameObject obj) => _highlighted.ContainsKey(obj);

    /// <summary>
    /// Highlight an entire route.
    /// </summary>
    public void Highlight(Route route, HighlightType highlightType)
    {
        foreach (GameObject hold in route.Holds)
            Highlight(hold, highlightType);
    }

    /// <summary>
    /// Highlight an entire route.
    /// </summary>
    public void Unhighlight(Route route)
    {
        foreach (GameObject hold in route.Holds)
            Unhighlight(hold);
    }

    /// <summary>
    /// Highlight the given object.
    /// If it is already, only change the highlighting from secondary to primary.
    /// </summary>
    public void Highlight(GameObject obj, HighlightType highlightType)
    {
        if (IsHighlighted(obj))
        {
            // the primary highlight overrides the secondary
            if (_highlighted[obj] == HighlightType.Secondary && highlightType == HighlightType.Primary)
                Unhighlight(obj);
            else
                return;
        }

        _highlighted[obj] = highlightType;

        var outline = obj.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;

        switch (highlightType)
        {
            case HighlightType.Primary:
                outline.OutlineColor = Color.white;
                outline.OutlineWidth = 30f;
                break;
            case HighlightType.Secondary:
                outline.OutlineColor = Color.grey;
                outline.OutlineWidth = 15f;
                break;
        }
    }

    /// <summary>
    /// Dehighlight the given object (if it's not already).
    /// </summary>
    public void Unhighlight(GameObject obj)
    {
        if (!IsHighlighted(obj)) return;
        
        _highlighted.Remove(obj);
        
        if (obj)
            DestroyImmediate(obj.GetComponent<Outline>());
    }

    /// <summary>
    /// Unhighlight all currently highlighted holds.
    /// Optionally only unhighlight those with a specific mode.
    /// </summary>
    public void UnhighlightAll(HighlightType? highlightType = null)
    {
        // ToList is used since we're modifying the collection
        foreach (GameObject obj in _highlighted.Keys.ToList())
            if (highlightType == null || highlightType.Value == _highlighted[obj])
                if (obj)
                    Unhighlight(obj);
    }
}