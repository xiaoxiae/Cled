using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dummiesman;
using UnityEngine;
using UnityEngine.UIElements;

public class Utilities
{
    /// <summary>
    /// Loading an object with a .mtl behaves weirdly, because the texture file must be relative to something weird.
    /// This wrapper reads the .mtl file and tries a bunch of places the texture could be in.
    /// </summary>
    public static GameObject ObjectLoadWrapper(string objPath, string mtlPath)
    {
        var modelStream = File.OpenRead(objPath);

        if (!File.Exists(mtlPath))
            return new OBJLoader().Load(modelStream);
        
        var textureContents = File.ReadAllText(mtlPath);

        var mc = Regex.Matches(textureContents, @"^\s*map_Kd\s+(.+?)$", RegexOptions.Multiline);

        if (mc.Count != 0)
        {
            var matchPath = mc[0].Groups[1].Value;
            
            // where to look for the materials
            string[] candidatePaths =
            {
                Path.GetFullPath(matchPath),
                Path.Combine(Path.GetDirectoryName(mtlPath) ?? string.Empty, matchPath),
            };
                    
            foreach(string fullPath in candidatePaths)
                if (File.Exists(fullPath))
                {
                    textureContents = textureContents.Replace(matchPath, fullPath);
                    break;
                }
        }

        // create a stream from the string
        MemoryStream textureStream = new MemoryStream();
        StreamWriter writer = new StreamWriter(textureStream);
        writer.Write(textureContents);
        writer.Flush();
        textureStream.Position = 0;

        return new OBJLoader().Load(modelStream, textureStream);
    }

    /// <summary>
    /// Ensure that the given path has an extension. If it doesn't, add one.
    /// </summary>
    public static string EnsureExtension(string path, string extension)
    {
        var ext = Path.GetExtension(path);

        if (ext == "")
            path += $".{extension}";

        return path;
    }

    /// <summary>
    /// Return an UID of a GameObject that is a string.
    /// </summary>
    public static string GetObjectId(GameObject obj) => Sha256(obj.GetInstanceID().ToString())[..12];

    /// <summary>
    /// Return a sha256 hash from a string.
    /// </summary>
    public static string Sha256(string randomString)
    {
        var crypt = new System.Security.Cryptography.SHA256Managed();
        var hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }
    

    /// <summary>
    /// Disable the element being able to be focused.
    /// </summary>
    public static void DisableElementFocusable(VisualElement element)
    {
        foreach (var child in element.Children())
        {
            child.focusable = false;
            
            DisableElementFocusable(child);
        }
    }
}