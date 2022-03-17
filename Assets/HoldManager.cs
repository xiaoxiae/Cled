using System;
using System.Collections.Generic;
using System.IO;
using Dummiesman;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;
using Color = System.Drawing.Color;

/// <summary>
/// A class for storing the information about the given hold.
/// </summary>
public class HoldInformation
{
    [CanBeNull] public Color? Color { get; set; }
    [CanBeNull] public string Type;
    [CanBeNull] public string Manufacturer;
    [CanBeNull] public string[] Labels;
}

/// <summary>
/// A class for storing both the hold information and the hold model.
/// </summary>
public class Hold
{
    public GameObject Model;
    public HoldInformation HoldInformation;

    public Hold(GameObject model, HoldInformation holdInformation)
    {
        Model = model;
        HoldInformation = holdInformation;
    }
}

/// <summary>
/// A class that handles various hold-related things, such as loading them, filtering them,
/// managing the ones that are selected and creating hold objects out of them.
/// </summary>
public class HoldManager : MonoBehaviour
{
    private IDictionary<string, Hold> _holds = new Dictionary<string, Hold>();

    public readonly string ModelsFolder = Path.Combine("Models", "Holds");
    public readonly string HoldsYamlName = "holds.yaml";

    void Start()
    {
        string yml = File.ReadAllText(Path.Combine(ModelsFolder, HoldsYamlName));

        var deserializer = new DeserializerBuilder().Build();
        
        var _holdInformation = deserializer.Deserialize<Dictionary<string, HoldInformation>>(yml);

        foreach (var pair in _holdInformation)
        {
            var hold = new Hold(ToGameObject(pair.Key), pair.Value);
            hold.Model.SetActive(false);

            _holds[pair.Key] = hold;
        }
    }

    /// <summary>
    /// Return a set of holds, given a filter delegate.
    /// </summary>
    /// <returns></returns>
    public Hold[] Filter(Func<HoldInformation, bool> filter)
    {
        List<Hold> result = new List<Hold>();
        
        foreach (string holdId in _holds.Keys)
            if (filter(_holds[holdId].HoldInformation))
                result.Add(_holds[holdId]);

        return result.ToArray();
    }

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    private GameObject ToGameObject(string id)
    {
        FileStream modelStream = File.OpenRead(GetHoldModelPath(id));
        FileStream textureStream = File.OpenRead(GetHoldMaterialPath(id));

        GameObject Hold = new OBJLoader().Load(modelStream, textureStream);
        
        MeshFilter mf = Hold.transform.GetChild(0).GetComponent<MeshFilter>();
        MeshCollider collider = Hold.AddComponent<MeshCollider>();
        collider.sharedMesh = mf.mesh;

        return Hold;
    }

    /// <summary>
    /// Return the path of the .obj file of the hold.
    /// </summary>
    private string GetHoldModelPath(string id) => Path.Combine(ModelsFolder, id + ".obj");

    /// <summary>
    /// Return the path of the .mtl file of the hold.
    /// </summary>
    private string GetHoldMaterialPath(string id) => Path.Combine(ModelsFolder, id + ".mtl");
}