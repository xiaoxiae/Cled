using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
/// A class for storing the information about the given hold.
/// </summary>
public class HoldInformation
{
    public string[] color;
    public string type;
    public string manufacturer;
    public string[] labels;
    public DateTime date;

    public string colorName => color[0];
    public string colorHex => color[1];
}

/// <summary>
/// A class for storing both the hold information and the actual hold instantiation (model, path to preview and image, etc.).
/// </summary>
public class HoldBlueprint
{
    public readonly GameObject Model;
    public readonly HoldInformation HoldInformation;

    public readonly string PreviewVideoPath;
    public readonly string PreviewImagePath;

    public HoldBlueprint(GameObject model, HoldInformation holdInformation, string previewImagePath,
        string previewVideoPath)
    {
        Model = model;
        HoldInformation = holdInformation;
        PreviewImagePath = previewImagePath;
        PreviewVideoPath = previewVideoPath;
    }
}

/// <summary>
/// A class that handles various hold-related things, such as loading them, filtering them,
/// managing the ones that are selected and creating hold objects out of them.
/// </summary>
public class HoldManager : MonoBehaviour
{
    private readonly IDictionary<string, HoldBlueprint> _holds = new Dictionary<string, HoldBlueprint>();

    public readonly string ModelsFolder = Path.Combine("Models", "Holds");
    public readonly string HoldsYamlName = "holds.yaml";

    /// <summary>
    /// Return the total number of loaded holds.
    /// </summary>
    public int HoldCount => _holds.Keys.Count;

    /// <summary>
    /// Aggregates an attribute from HoldInformation (like all colors, types, etc.).
    /// </summary>
    public List<string> AttributeAggregate(Func<HoldInformation, List<string>> aggregateFunction)
    {
        var set = new HashSet<string>();

        foreach (var bp in _holds.Values)
        {
            var info = bp.HoldInformation;

            foreach (string str in aggregateFunction(info).Where(str => !string.IsNullOrEmpty(str)))
                set.Add(str);
        }

        return set.ToList();
    }

    /// <summary>
    /// Return all possible hold colors.
    /// </summary>
    public List<string> AllColors() => AttributeAggregate(info => new List<string> { info.colorName });

    /// <summary>
    /// Return all possible hold colors.
    /// </summary>
    public List<string> AllTypes() => AttributeAggregate(info => new List<string> { info.type });

    /// <summary>
    /// Return all possible labels.
    /// </summary>
    public List<string> AllLabels() => AttributeAggregate(info => info.labels == null ? new List<string>() : info.labels.ToList());

    /// <summary>
    /// Return all possible hold manufacturers.
    /// </summary>
    public List<string> AllManufacturers() => AttributeAggregate(info => new List<string> { info.manufacturer });

    // it takes a while for the holds to load
    public bool Ready { get; set; }

    void Start()
    {
        string yml = File.ReadAllText(Path.Combine(ModelsFolder, HoldsYamlName));

        var deserializer = new DeserializerBuilder().Build();

        var holdInformation = deserializer.Deserialize<Dictionary<string, HoldInformation>>(yml);

        foreach (var pair in holdInformation)
        {
            var hold = new HoldBlueprint(
                ToGameObject(pair.Key),
                pair.Value,
                GetHoldPreviewImagePath(pair.Key),
                GetHoldPreviewVideoPath(pair.Key)
            );

            hold.Model.SetActive(false);

            _holds[pair.Key] = hold;
        }

        Ready = true;
    }

    /// <summary>
    /// Return a set of holds, given a filter delegate.
    /// </summary>
    /// <returns></returns>
    public HoldBlueprint[] Filter(Func<HoldInformation, bool> filter) => (from holdId in _holds.Keys
            where filter(_holds[holdId].HoldInformation)
            select _holds[holdId])
        .ToArray();

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    private GameObject ToGameObject(string id)
    {
        FileStream modelStream = File.OpenRead(GetHoldModelPath(id));
        FileStream textureStream = File.OpenRead(GetHoldMaterialPath(id));

        GameObject hold = new OBJLoader().Load(modelStream, textureStream);

        MeshFilter mf = hold.transform.GetChild(0).GetComponent<MeshFilter>();
        MeshCollider meshCollider = hold.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mf.mesh;

        return hold;
    }

    private string GetHoldPreviewVideoPath(string id) => Path.Combine(ModelsFolder, id + "-preview.webm");

    private string GetHoldPreviewImagePath(string id) => Path.Combine(ModelsFolder, id + "-preview.jpg");

    private string GetHoldModelPath(string id) => Path.Combine(ModelsFolder, id + ".obj");

    private string GetHoldMaterialPath(string id) => Path.Combine(ModelsFolder, id + ".mtl");
}