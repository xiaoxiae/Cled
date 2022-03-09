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
    public Color? Color { get; set; }
    [CanBeNull] public string Type;
    [CanBeNull] public string Manufacturer;
    [CanBeNull] public string[] Labels;
}

/// <summary>
/// A class that handles various hold-related things, such as loading them, filtering them
/// and creating hold objects out of them.
/// </summary>
public class HoldManager : MonoBehaviour
{
    public IDictionary<string, Hold> Holds;

    public string ModelsFolder = Path.Combine("Models", "Holds");
    public string HoldsYamlName = "holds.yaml";

    void Start()
    {
        string yml = File.ReadAllText(Path.Combine(ModelsFolder, HoldsYamlName));

        var deserializer = new DeserializerBuilder().Build();

        Holds = deserializer.Deserialize<Dictionary<string, Hold>>(yml);

        // var obj = ToGameObject(Holds.Keys.ToArray()[0]);
    }

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    public GameObject ToGameObject(string id)
    {
        FileStream modelStream = File.OpenRead(GetHoldModelPath(id));
        FileStream textureStream = File.OpenRead(GetHoldMaterialPath(id));

        return new OBJLoader().Load(modelStream, textureStream);
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