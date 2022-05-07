using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dummiesman;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Utilities
{
    /// <summary>
    ///     Loading an object with a .mtl behaves weirdly, because the texture file must be relative to something weird.
    ///     This wrapper reads the .mtl file and tries a bunch of places the texture could be in.
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
                Path.Combine(Path.GetDirectoryName(mtlPath) ?? string.Empty, matchPath)
            };

            foreach (var fullPath in candidatePaths)
                if (File.Exists(fullPath))
                {
                    textureContents = textureContents.Replace(matchPath, fullPath);
                    break;
                }
        }

        // create a stream from the string
        var textureStream = new MemoryStream();
        var writer = new StreamWriter(textureStream);
        writer.Write(textureContents);
        writer.Flush();
        textureStream.Position = 0;

        return new OBJLoader().Load(modelStream, textureStream);
    }

    /// <summary>
    ///     Ensure that the given path has an extension. If it doesn't, add one.
    /// </summary>
    public static string EnsureExtension(string path, string extension)
    {
        var ext = Path.GetExtension(path);

        if (ext == "")
            path += $".{extension}";

        return path;
    }

    /// <summary>
    ///     Return an UID of a GameObject that is a string.
    /// </summary>
    public static string GetObjectId(GameObject obj)
    {
        return Sha256(obj.GetInstanceID().ToString())[..12];
    }

    /// <summary>
    ///     Return a sha256 hash from a string.
    /// </summary>
    public static string Sha256(string randomString)
    {
        var crypt = new SHA256Managed();
        var hash = new StringBuilder();
        var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (var theByte in crypto) hash.Append(theByte.ToString("x2"));

        return hash.ToString();
    }

    /// <summary>
    ///     Disable the element being able to be focused.
    /// </summary>
    public static void DisableElementFocusable(VisualElement element)
    {
        foreach (var child in element.Children())
        {
            child.focusable = false;

            DisableElementFocusable(child);
        }
    }

    public static void SetRendererOpacity(Renderer renderer, float opacity)
    {
        var mtl = renderer.material;

        // https://stackoverflow.com/questions/39366888/unity-mesh-renderer-wont-be-completely-transparent
        // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/
        mtl.color = new Color(mtl.color.r, mtl.color.g, mtl.color.b, opacity);

        if (Math.Abs(opacity - 1) < 0.01)
        {
            mtl.SetFloat("_Mode", 0);
            mtl.SetOverrideTag("RenderType", "");
            mtl.SetInt("_SrcBlend", (int)BlendMode.One);
            mtl.SetInt("_DstBlend", (int)BlendMode.Zero);
            mtl.SetInt("_ZWrite", 1);
            mtl.DisableKeyword("_ALPHATEST_ON");
            mtl.DisableKeyword("_ALPHABLEND_ON");
            mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtl.renderQueue = -1;
        }
        else
        {
            mtl.SetFloat("_Mode", 2);
            mtl.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mtl.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mtl.SetInt("_ZWrite", 0);
            mtl.DisableKeyword("_ALPHATEST_ON");
            mtl.EnableKeyword("_ALPHABLEND_ON");
            mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtl.renderQueue = 3000;
        }
    }
}


/// <summary>
///     An interface for things that can be closed (like menus, popups, etc.).
/// </summary>
public interface IClosable
{
    public void Close();
}

/// <summary>
///     An interface for things that can be accepted (like settings or popups).
/// </summary>
public interface IAcceptable
{
    public void Accept();
}


/// <summary>
///     An interface for things that can be reset to their initial state.
/// </summary>
public interface IResetable
{
    public void Reset();
}

/// <summary>
///     By default, serialization emits all public fields, which includes things like magnitude and normalized form.
///     This class ensures that only x, y and z coordinates of the Vector3 are stored and the serialization looks nice.
///     There is probably a different way to do this, but this works well enough.
/// </summary>
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    ///     Automatic conversion from SerializableVector3 to Vector3.
    /// </summary>
    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    ///     Automatic conversion from Vector3 to SerializableVector3.
    /// </summary>
    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }

    /// <summary>
    ///     Automatic conversion from SerializableVector3 to Vector3.
    /// </summary>
    public static implicit operator Vector2(SerializableVector2 rValue)
    {
        return new Vector2(rValue.x, rValue.y);
    }

    /// <summary>
    ///     Automatic conversion from Vector3 to SerializableVector3.
    /// </summary>
    public static implicit operator SerializableVector2(Vector2 rValue)
    {
        return new SerializableVector2(rValue.x, rValue.y);
    }
}


/// <summary>
///     The object that gets serialized when exporting.
/// </summary>
public class SerializableState
{
    // capture
    public SerializableCaptureSettings CaptureSettings;
    public List<string> EndingHoldIDs;
    public string HoldModelsPath;

    // given a hold instance, store its id and state
    public Dictionary<string, SerializableHold> Holds;

    // lights
    public SerializableLights Lights;

    // player
    public SerializablePlayer Player;

    // all routes
    public List<SerializableRoute> Routes;

    // selected holds
    public List<string> SelectedHoldBlueprintIDs;

    // starting/ending holds
    public List<string> StartingHoldIDs;
    public string Version;

    // paths to wall and models
    public string WallModelPath;
}

public class SerializableCaptureSettings
{
    public string ImagePath;
    public int ImageSupersize;
}

public class SerializableRoute
{
    public string Grade;
    public List<string> HoldIDs;

    public string Name;
    public string Setter;
    public string Zone;
}

public class SerializableHold
{
    public string BlueprintId;
    public HoldState State;
}

public class SerializableLights
{
    // light parameters
    public float Intensity;

    // positions of the lights
    public List<SerializableVector3> Positions;
    public float ShadowStrength;
}

public class SerializablePlayer
{
    // whether the player's flashlight is on
    public bool Light;

    // the player position and orientation
    public SerializableVector3 Position { get; set; }
    public SerializableVector2 Orientation { get; set; }

    // whether the player is flying
    public bool Flying { get; set; }
}