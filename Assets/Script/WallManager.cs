using System.IO;
using Dummiesman;
using UnityEngine;

using static PreferencesManager;

public class WallManager : MonoBehaviour
{
    public GameObject Wall { get; set; }
    
    /// <summary>
    /// Generate the wall object from the given path.
    /// </summary>
    public void InitializeFromPath(string path)
    {
        var wallFolder = Path.GetDirectoryName(path);
        var wallObjectName = Path.GetFileNameWithoutExtension(path);
        
        var modelStream = File.OpenRead(Path.Combine(wallFolder, wallObjectName + ".obj"));
        var materialsPath = Path.Combine(wallFolder, wallObjectName + ".mtl");
        
        if (!File.Exists(materialsPath))
            Wall = new OBJLoader().Load(modelStream);
        else
        {
            var textureStream = File.OpenRead(materialsPath);
            Wall = new OBJLoader().Load(modelStream, textureStream);
        }

        MeshFilter mf = Wall.transform.GetChild(0).GetComponent<MeshFilter>();
        MeshCollider collider = Wall.AddComponent<MeshCollider>();
        collider.sharedMesh = mf.mesh;
    }
}
