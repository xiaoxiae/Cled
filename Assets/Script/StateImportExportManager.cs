using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

/// <summary>
/// The object that gets serialized when exporting.
/// </summary>
public class WallState
{
    public string WallModelPath { get; set; }
    public string HoldModelsPath { get; set; }
}

public class StateImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public RouteManager routeManager;
    public WallManager wallManager;

    /// <summary>
    /// Import the state from the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Import(string path)
    {
        var deserializer = new Deserializer();

        try
        {
            using var reader = new StreamReader(path);
            
            var obj = deserializer.Deserialize<WallState>(reader);

            PreferencesManager.CurrentWallModelPath = obj.WallModelPath;
            PreferencesManager.CurrentHoldModelsPath = obj.HoldModelsPath;

            holdStateManager.Clear();
            routeManager.Clear();
            
            wallManager.InitializeFromPath(path);
        }
        catch (Exception exception)
        {
            // TODO display message
            return false;
        }

        return true;
    }

    /// <summary>
    /// Export the state to the given path.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool Export(string path)
    {
        try
        {
            using StreamWriter writer = new StreamWriter(path);

            var serializer = new Serializer();
            serializer.Serialize(writer,
                new WallState
                {
                    WallModelPath = PreferencesManager.CurrentWallModelPath,
                    HoldModelsPath = PreferencesManager.CurrentHoldModelsPath
                });
        }
        catch (Exception exception)
        {
            // TODO display message
            return false;
        }

        return true;
    }
}