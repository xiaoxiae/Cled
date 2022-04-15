using System.IO;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public GameObject Wall { get; set; }

    /// <summary>
    /// Generate the wall object from the given path, discarding the old one in the process.
    /// </summary>
    public void InitializeFromPath(string path)
    {
        if (!Wall)
            DestroyImmediate(Wall);

        var wallFolder = Path.GetDirectoryName(path);
        var wallObjectName = Path.GetFileNameWithoutExtension(path);

        var objPath = Path.Combine(wallFolder, wallObjectName + ".obj");
        var mtlPath = Path.Combine(wallFolder, wallObjectName + ".mtl");
        
        Wall = Utilities.ObjectLoadWrapper(objPath, mtlPath);

        MeshFilter mf = Wall.transform.GetChild(0).GetComponent<MeshFilter>();
        MeshCollider collider = Wall.AddComponent<MeshCollider>();
        collider.sharedMesh = mf.mesh;
    }
}