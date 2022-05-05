using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
/// A class for storing wall metadata, such as routesetters, valid grades, zones where the boulders are, etc.
/// </summary>
public class WallMetadata
{
    public WallMetadata()
    {
        Grades = new List<string>();
        Setters = new List<string>();
        Zones = new List<string>();
    }

    public List<string> Grades { get; }
    public List<string> Setters { get; }
    public List<string> Zones { get; }
}

public class WallLoader : MonoBehaviour
{
    public GameObject Wall;
    public WallMetadata Metadata;

    /// <summary>
    /// Generate the wall object from the given path, discarding the old one in the process.
    /// </summary>
    public void Initialize(string path)
    {
        if (!Wall)
            DestroyImmediate(Wall);

        var wallFolder = Path.GetDirectoryName(path);
        var wallObjectName = Path.GetFileNameWithoutExtension(path);

        var objPath = Path.Combine(wallFolder, wallObjectName + ".obj");
        var mtlPath = Path.Combine(wallFolder, wallObjectName + ".mtl");

        Wall = Utilities.ObjectLoadWrapper(objPath, mtlPath);

        var metadataPath = Path.Combine(wallFolder, wallObjectName + ".yaml");
        if (File.Exists(metadataPath))
        {
            string yml = File.ReadAllText(metadataPath);
            
            Metadata = new Deserializer().Deserialize<WallMetadata>(yml);
        }
        else
        {
            Metadata = new WallMetadata();
        }
        
        MeshFilter mf = Wall.transform.GetChild(0).GetComponent<MeshFilter>();
        MeshCollider collider = Wall.AddComponent<MeshCollider>();
        collider.sharedMesh = mf.mesh;
    }

    public void Clear()
    {
        if (Wall)
            Destroy(Wall);

        Metadata = null;
    }
}