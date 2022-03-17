using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script for working with managing selected things.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    private List<GameObject> _selected = new List<GameObject>();

    /// <summary>
    /// Return true if the given object is selected.
    /// </summary>
    public bool IsSelected(GameObject obj) => _selected.Contains(obj);
    
    /// <summary>
    /// Select the given object (if it's not already).
    /// </summary>
    public void Select(GameObject obj)
    {
        if (IsSelected(obj)) return;
        
        _selected.Add(obj);
        
        var outline = obj.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 15f;
    }
    
    /// <summary>
    /// Deselect the given object (if it's not already).
    /// </summary>
    public void Deselect(GameObject obj)
    {
        if (!IsSelected(obj)) return;
        
        _selected.Remove(obj);
        Destroy(obj.GetComponent<Outline>());
    }

    /// <summary>
    /// Deselect all currently selected holds.
    /// </summary>
    public void DeselectAll()
    {
        while (_selected.Count != 0)
            Deselect(_selected[0]);
    }
}
