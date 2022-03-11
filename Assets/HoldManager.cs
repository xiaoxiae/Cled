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
public class Hold
{
    [CanBeNull] public Color? Color { get; set; }
    [CanBeNull] public string Type;
    [CanBeNull] public string Manufacturer;
    [CanBeNull] public string[] Labels;
}

/// <summary>
/// A class that handles various hold-related things, such as loading them, filtering them,
/// managing the ones that are selected and creating hold objects out of them.
/// </summary>
public class HoldManager : MonoBehaviour
{
    private IDictionary<string, Hold> _holds;

    public readonly string ModelsFolder = Path.Combine("Models", "Holds");
    public readonly string HoldsYamlName = "holds.yaml";

    void Start()
    {
        string yml = File.ReadAllText(Path.Combine(ModelsFolder, HoldsYamlName));

        var deserializer = new DeserializerBuilder().Build();

        _holds = deserializer.Deserialize<Dictionary<string, Hold>>(yml);
    }

    /// <summary>
    /// Return a set of hold IDs, given a filter delegate.
    /// </summary>
    /// <returns></returns>
    public string[] Filter(Func<Hold, bool> filter)
    {
        List<string> result = new List<string>();
        
        foreach (string holdId in _holds.Keys)
            if (filter(_holds[holdId]))
                result.Add(holdId);

        return result.ToArray();
    }

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    public GameObject ToGameObject(string id)
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