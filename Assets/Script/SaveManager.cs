using System;
using UnityEngine;

/// <summary>
/// Keep track of whether we should save the project, or save the project as.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public bool ForceSaveAs { get; set; }
}