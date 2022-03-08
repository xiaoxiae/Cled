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
    public string Id { get; set; }
    public Color? Color { get; set; }
    [CanBeNull] public string Type;
    [CanBeNull] public string Manufacturer;
    [CanBeNull] public string[] Labels;
    
    /// <summary>
    /// Return the path of the .obj file of the hold.
    /// </summary>
    public string GetModelPath()
    {
        // TODO: hardcoded!
        return "Models/" + Id + ".obj";
    }
    
    /// <summary>
    /// Return the path of the .mtl file of the hold.
    /// </summary>
    public string GetTexturePath()
    {
        // TODO: hardcoded!
        return "Models/" + Id + ".mtl";
    }

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    public GameObject ToGameObject()
    {
        FileStream modelStream = File.OpenRead(GetModelPath());
        FileStream textureStream = File.OpenRead(GetTexturePath());
        
        return new OBJLoader().Load(modelStream, textureStream);
    }
}

/// <summary>
/// A class that handles various hold-related things, such as loading them, filtering them
/// and creating hold objects out of them.
/// </summary>
public class HoldManager : MonoBehaviour
{
    public Hold[] Holds;

    void Start()
    {
        // TODO: make this not hardcoded
        string yml = File.ReadAllText("Models/holds.yaml");
        
        var deserializer = new DeserializerBuilder().Build();

        Holds = deserializer.Deserialize<Hold[]>(yml);
        //var obj = Holds[0].ToGameObject();
    }
}