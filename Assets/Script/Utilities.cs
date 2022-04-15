using System.IO;
using System.Text.RegularExpressions;
using Dummiesman;
using UnityEngine;

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
}