using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script for working with managing highlighted things.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    private List<GameObject> _highlighted = new List<GameObject>();

    /// <summary>
    /// Return true if the given object is highlighted.
    /// </summary>
    public bool IsHighlighted(GameObject obj) => _highlighted.Contains(obj);
    
    /// <summary>
    /// Highlight the given object (if it's not already).
    /// </summary>
    public void Highlight(GameObject obj)
    {
        if (IsHighlighted(obj)) return;
        
        _highlighted.Add(obj);
        
        var outline = obj.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 15f;
    }
    
    /// <summary>
    /// Dehighlight the given object (if it's not already).
    /// </summary>
    public void Unhighlight(GameObject obj)
    {
        if (!IsHighlighted(obj)) return;
        
        _highlighted.Remove(obj);
        Destroy(obj.GetComponent<Outline>());
    }

    /// <summary>
    /// Unhighlight all currently highlighted holds.
    /// </summary>
    public void UnhighlightAll()
    {
        while (_highlighted.Count != 0)
            Unhighlight(_highlighted[0]);
    }
}
