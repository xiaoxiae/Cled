using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using YamlDotNet;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Color = System.Drawing.Color;

/// <summary>
/// An enum for all possible types of holds.
/// </summary>
public enum HoldType
{
    Crimp,
    Jug,
    Sloper,
    Pinch,
    Pocket
}

/// <summary>
/// A class for storing the information about the given hold.
/// </summary>
public class Hold
{
    public string Name { get; set; }
    public Color? Color { get; set; }
    public HoldType? Type;
    [CanBeNull] public string Manufacturer;
    [CanBeNull] public string[] Labels;
}

public class HoldManager : MonoBehaviour
{
    public Hold[] Holds;
    
    void Start()
    {
        // TODO: make this not hardcoded
        string yml = System.IO.File.ReadAllText("Data/holds.yaml");

        var deserializer = new DeserializerBuilder().Build();

        Holds = deserializer.Deserialize<Hold[]>(yml);
    }
}