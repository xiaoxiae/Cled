using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

public class Exporter : MonoBehaviour
{
    public HoldStateManager holdStateManager;
    public HoldPickerMenu holdPickerMenu;
    public RouteManager routeManager;
    public LightManager lightManager;
    public PopupMenu popupMenu;

    public MovementController movementController;
    public CameraController cameraController;

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

            var serializer = new SerializerBuilder().DisableAliases().Build();

            // save holds
            var holds = new Dictionary<string, SerializableHold>();
            foreach (GameObject hold in holdStateManager.PlacedHolds)
            {
                var holdBlueprint = holdStateManager.GetHoldBlueprint(hold);
                var holdState = holdStateManager.GetHoldState(hold);

                holds[Utilities.GetObjectId(hold)] = new SerializableHold
                    { BlueprintId = holdBlueprint.Id, State = holdState };
            }

            // save only routes that either contain one or more holds, or contain a starting/ending hold
            var routes = new List<SerializableRoute>();
            foreach (Route route in routeManager.GetUsableRoutes())
                routes.Add(new SerializableRoute
                {
                    HoldIDs = route.Holds.Select(Utilities.GetObjectId).ToList(),
                    Name = route.Name,
                    Setter = route.Setter,
                    Zone = route.Zone,
                    Grade = route.Grade,
                });

            serializer.Serialize(writer,
                new SerializableState
                {
                    Version = Application.version,
                    Player = new SerializablePlayer
                    {
                        Position = movementController.Position,
                        Orientation = cameraController.Orientation,
                        Flying = movementController.Flying,
                        Light = lightManager.PlayerLightEnabled
                    },
                    WallModelPath = Preferences.CurrentWallModelPath,
                    HoldModelsPath = Preferences.CurrentHoldModelsPath,
                    Holds = holds,
                    Routes = routes,
                    StartingHoldIDs = routeManager.StartingHolds.Select(Utilities.GetObjectId).ToList(),
                    EndingHoldIDs = routeManager.EndingHolds.Select(Utilities.GetObjectId).ToList(),
                    SelectedHoldBlueprintIDs = holdPickerMenu.GetPickedHolds().Select(val => val.Id).ToList(),
                    Lights = new SerializableLights
                    {
                        Positions = lightManager.GetPositions().Select<Vector3, SerializableVector3>(x => x).ToList(),
                        Intensity = Preferences.LightIntensity,
                        ShadowStrength = Preferences.LightIntensity,
                    },
                    CaptureSettings = new SerializableCaptureSettings
                    {
                        ImagePath = Preferences.CaptureImagePath,
                        ImageSupersize = Preferences.ImageSupersize,
                    }
                });

            return true;
        }
        catch (Exception e)
        {
            popupMenu.CreateInfoPopup($"The following exception occurred while exporting the project: {e.Message}");
            return false;
        }
    }
}