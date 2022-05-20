using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
///     A class for storing the hold metadata.
///     It is deserialized from the `holds.yaml` file.
/// </summary>
public class HoldMetadata
{
    public string[] color;
    public DateTime date;
    public string[] labels;
    public string manufacturer;
    public string type;
    public float volume;

    public string colorName => color[0];
    public string colorHex => color[1];
}

/// <summary>
///     A class for storing both the hold information and the actual hold instantiation (model, path to preview and image,
///     etc.).
/// </summary>
public class HoldBlueprint
{
    public readonly HoldMetadata holdMetadata;
    public readonly string Id;

    public readonly GameObject Model;
    public readonly string PreviewImagePath;

    public readonly string PreviewVideoPath;

    public HoldBlueprint(string id, GameObject model, HoldMetadata holdMetadata, string previewImagePath,
        string previewVideoPath)
    {
        Id = id;
        Model = model;
        this.holdMetadata = holdMetadata;
        PreviewImagePath = previewImagePath;
        PreviewVideoPath = previewVideoPath;
    }
}

/// <summary>
///     A class that handles various hold-related things, such as loading them, filtering them
///     and creating hold objects out of them.
/// </summary>
public class HoldLoader : MonoBehaviour, IResetable
{
    private readonly IDictionary<string, HoldBlueprint> _holds = new Dictionary<string, HoldBlueprint>();

    public readonly string HoldsYamlName = "holds.yaml";

    /// <summary>
    ///     Return the total number of loaded holds.
    /// </summary>
    public int HoldCount => _holds.Keys.Count;

    /// <summary>
    ///     Return all of the loaded hold blueprints.
    /// </summary>
    public IEnumerable<HoldBlueprint> Holds => _holds.Values;

    /// <summary>
    ///     Remove all holds, destroying them in the process.
    /// </summary>
    public void Reset()
    {
        foreach (var (key, value) in _holds)
            DestroyImmediate(value.Model);

        _holds.Clear();
    }

    /// <summary>
    ///     Aggregates an attribute from HoldMetadata (like all colors, types, etc.).
    /// </summary>
    private IEnumerable<string> AttributeAggregate(Func<HoldMetadata, IEnumerable<string>> aggregateFunction)
    {
        var set = new HashSet<string>();

        foreach (var bp in Holds)
        {
            var info = bp.holdMetadata;

            foreach (var str in aggregateFunction(info).Where(str => !string.IsNullOrWhiteSpace(str)))
                set.Add(str);
        }

        return set;
    }

    /// <summary>
    ///     Return all possible hold colors.
    /// </summary>
    public IEnumerable<string> AllColors()
    {
        return AttributeAggregate(info => new List<string> { info.colorName });
    }

    /// <summary>
    ///     Return all possible hold colors.
    /// </summary>
    public IEnumerable<string> AllTypes()
    {
        return AttributeAggregate(info => new List<string> { info.type });
    }

    /// <summary>
    ///     Return all possible labels.
    /// </summary>
    public IEnumerable<string> AllLabels()
    {
        return AttributeAggregate(info => info.labels?.ToList() ?? new List<string>());
    }

    /// <summary>
    ///     Return all possible hold manufacturers.
    /// </summary>
    public IEnumerable<string> AllManufacturers()
    {
        return AttributeAggregate(info => new List<string> { info.manufacturer });
    }

    /// <summary>
    ///     Initialize holds from the given directory.
    /// </summary>
    public void Initialize(string path)
    {
        var yml = File.ReadAllText(Path.Combine(path, HoldsYamlName));
        var holdInformation = new DeserializerBuilder()
            .IgnoreUnmatchedProperties().Build()
            .Deserialize<Dictionary<string, HoldMetadata>>(yml);

        foreach (var pair in holdInformation)
        {
            var hold = new HoldBlueprint(
                pair.Key,
                ToGameObject(pair.Key),
                pair.Value,
                GetHoldPreviewImagePath(pair.Key),
                GetHoldPreviewVideoPath(pair.Key)
            );

            hold.Model.SetActive(false);

            _holds[pair.Key] = hold;
        }
    }

    /// <summary>
    ///     Return a set of holds, given a filter delegate.
    /// </summary>
    public HoldBlueprint[] Filter(Func<HoldMetadata, bool> filter)
    {
        return (from holdId in _holds.Keys
                where filter(_holds[holdId].holdMetadata)
                select _holds[holdId])
            .ToArray();
    }

    /// <summary>
    ///     Get the hold blueprint associated with the given hold ID.
    /// </summary>
    public HoldBlueprint GetHoldBlueprint(string id)
    {
        return _holds[id];
    }

    /// <summary>
    ///     Generate a 3D object from the given hold.
    /// </summary>
    private GameObject ToGameObject(string id)
    {
        var hold = Utilities.ObjectLoadWrapper(GetHoldModelPath(id), GetHoldMaterialPath(id));

        var mf = hold.transform.GetChild(0).GetComponent<MeshFilter>();
        var meshCollider = hold.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mf.mesh;

        return hold;
    }

    private static string GetHoldPreviewVideoPath(string id)
    {
        return Path.Combine(Preferences.HoldModelsPath, id + "-preview.webm");
    }

    private static string GetHoldPreviewImagePath(string id)
    {
        return Path.Combine(Preferences.HoldModelsPath, id + "-preview.jpg");
    }

    private static string GetHoldModelPath(string id)
    {
        return Path.Combine(Preferences.HoldModelsPath, id + ".obj");
    }

    private static string GetHoldMaterialPath(string id)
    {
        return Path.Combine(Preferences.HoldModelsPath, id + ".mtl");
    }
}