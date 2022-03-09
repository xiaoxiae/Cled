using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dummiesman;
using UnityEngine;

public class WallImporter : MonoBehaviour
{
    public string WallFolder = Path.Combine("Models", "Wall");
    public string WallObjectName = Path.Combine("wall");
    
    void Start()
    {
        var wallObj = ToGameObject();
        
        wallObj.AddComponent<MeshRenderer>();
        wallObj.AddComponent<MeshCollider>();

        wallObj.GetComponent<MeshCollider>().convex = true;

    }

    /// <summary>
    /// Generate a 3D object from the given hold.
    /// </summary>
    public GameObject ToGameObject()
    {
        var modelStream = File.OpenRead(GetWallModelPath());

        var materialsPath = GetWallMaterialPath();
        
        if (!File.Exists(materialsPath)) return new OBJLoader().Load(modelStream);
        
        var textureStream = File.OpenRead(GetWallMaterialPath());
        return new OBJLoader().Load(modelStream, textureStream);
    }

    /// <summary>
    /// Return the path of the .obj file of the wall.
    /// </summary>
    private string GetWallModelPath() => Path.Combine(WallFolder, WallObjectName + ".obj");

    /// <summary>
    /// Return the path of the .mtl file of the wall.
    /// </summary>
    private string GetWallMaterialPath() => Path.Combine(WallFolder, WallObjectName + ".mtl");
}
