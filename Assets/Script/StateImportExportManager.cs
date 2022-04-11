using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;


/// <summary>
/// The object that gets serialized when exporting.
/// </summary>
public class SerializableWallState
{
    public string WallModelPath { get; set; }
    public string HoldModelsPath { get; set; }

    public Dictionary<string, List<HoldState>> HoldStates { get; set; }
}

public class StateImportExportManager : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public HoldManager holdManager;
    public RouteManager routeManager;
    public WallManager wallManager;

    private static SerializableWallState Deserialize(string path)
    {
        var deserializer = new Deserializer();

        using var reader = new StreamReader(path);
        return deserializer.Deserialize<SerializableWallState>(reader);
    }

    public static bool ImportPreferences(string path)
    {
        try
        {
            var obj = Deserialize(path);

            PreferencesManager.CurrentWallModelPath = obj.WallModelPath;
            PreferencesManager.CurrentHoldModelsPath = obj.HoldModelsPath;
        }
        catch (Exception e)
        {
            Debug.Log(e);

            // TODO display message
            return false;
        }

        return true;
    }

    /// <summary>
    /// Import the state from the given path.
    /// Must be called after ImportPreferences and after the Awake functions of managers were called.
    /// 
    /// Return true if successful, else false.
    /// </summary>
    public bool ImportState(string path)
    {
        try
        {
            var obj = Deserialize(path);

            holdStateManager.Clear();
            routeManager.Clear();

            foreach (string id in obj.HoldStates.Keys)
            {
                var bp = holdManager.GetHoldBlueprint(id);

                foreach (HoldState state in obj.HoldStates[id])
                    holdStateManager.Place(bp, state);
            }

            wallManager.InitializeFromPath(PreferencesManager.CurrentWallModelPath);
        }
        catch (Exception e)
        {
            Debug.Log(e);

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
        var ext = Path.GetExtension(path);

        // if it doesn't contain an extension, add it
        // if it does, the data will still be YAML, but it's less invasive than changing it
        if (ext == "")
            path += ".yaml";

        try
        {
            using StreamWriter writer = new StreamWriter(path);

            var serializer = new Serializer();

            // get holds
            var holds = new Dictionary<string, List<HoldState>>();
            foreach (GameObject hold in holdStateManager.GetAllHolds)
            {
                var holdBlueprint = holdStateManager.GetHoldBlueprint(hold);
                var holdState = holdStateManager.GetHoldState(hold);

                if (holds.ContainsKey(holdBlueprint.Id))
                    holds[holdBlueprint.Id].Add(holdState);
                else
                    holds[holdBlueprint.Id] = new List<HoldState> { holdState };
            }

            serializer.Serialize(writer,
                new SerializableWallState
                {
                    WallModelPath = PreferencesManager.CurrentWallModelPath,
                    HoldModelsPath = PreferencesManager.CurrentHoldModelsPath,
                    HoldStates = holds,
                });
        }
        catch (Exception e)
        {
            Debug.Log(e);

            // TODO display message
            return false;
        }

        return true;
    }
}