using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// How should the highlight look like - whether it's primary, secondary, or tertiary.
/// </summary>
public enum HighlightType
{
    Primary,
    Secondary,
    Tertiary,
}

/// <summary>
/// A script for working with managing highlighted things.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;

    private readonly Dictionary<GameObject, HighlightType> _highlighted = new();

    /// <summary>
    /// Return true if the given object is highlighted.
    /// </summary>
    public bool IsHighlighted(GameObject obj) => _highlighted.ContainsKey(obj);

    /// <summary>
    /// Highlight an entire route, de-highlighting every other one.
    /// </summary>
    public void Highlight(Route route, bool fadeOtherHolds = false)
    {
        foreach (var hold in route.Holds)
            Highlight(hold, HighlightType.Secondary);

        if (fadeOtherHolds)
            foreach (var hold in holdStateManager.GetAllHolds())
                if (!route.ContainsHold(hold))
                    Highlight(hold, HighlightType.Tertiary);
    }

    /// <summary>
    /// Set the opacity of a hold to the given value.
    /// </summary>
    private static void SetHoldOpacity(GameObject hold, float opacity)
    {
        var renderer = hold.transform.GetChild(0).GetComponent<Renderer>();
        var mtl = renderer.material;

        // https://stackoverflow.com/questions/39366888/unity-mesh-renderer-wont-be-completely-transparent
        // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/
        mtl.color = new Color(1f, 1f, 1f, opacity);

        if (Math.Abs(opacity - 1) < 0.01)
        {
            mtl.SetFloat("_Mode", 0);
            mtl.SetOverrideTag("RenderType", "");
            mtl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mtl.SetInt("_ZWrite", 1);
            mtl.DisableKeyword("_ALPHATEST_ON");
            mtl.DisableKeyword("_ALPHABLEND_ON");
            mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtl.renderQueue = -1;
        }
        else
        {
            mtl.SetFloat("_Mode", 2);
            mtl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mtl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mtl.SetInt("_ZWrite", 0);
            mtl.DisableKeyword("_ALPHATEST_ON");
            mtl.EnableKeyword("_ALPHABLEND_ON");
            mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtl.renderQueue = 3000;
        }
    }

    /// <summary>
    /// Highlight the given object.
    /// If it is already, only change the highlighting from secondary to primary (or tertiary to primary).
    /// </summary>
    public void Highlight(GameObject obj, HighlightType highlightType)
    {
        if (IsHighlighted(obj))
        {
            // the primary highlight overrides the secondary
            if (_highlighted[obj] is HighlightType.Secondary or HighlightType.Tertiary &&
                highlightType == HighlightType.Primary)
                Unhighlight(obj);
            else
                return;
        }

        _highlighted[obj] = highlightType;

        Outline outline;
        switch (highlightType)
        {
            case HighlightType.Primary:
                outline = obj.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                outline.OutlineColor = Color.white;
                break;
            case HighlightType.Secondary:
                outline = obj.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                outline.OutlineColor = Color.grey;
                break;
            case HighlightType.Tertiary:
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

        _highlighted.Remove(obj);

        if (obj.GetComponent<Outline>() != null)
            DestroyImmediate(obj.GetComponent<Outline>());

        SetHoldOpacity(obj, 1.0f);
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