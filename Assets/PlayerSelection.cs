using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;

    private string[] _selected;
    private int _selectedIndex;

    private GameObject pointerObject = null;

    private bool wasSeen;

    void Start()
    {
        // TODO: remove this, temporarily selects all of the holds
        _selected = HoldManager.Filter(hold => true);

        pointerObject = HoldManager.ToGameObject(_selected[_selectedIndex]);
    }

    void Update()
    {
        // in insert mode, do stuff with placing the hold
        if (PlayerControl.CurrentMode == Mode.Edit)
        {
            // cast a ray directly in front of the camera
            var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit))
            {
                pointerObject.transform.Rotate(0.1f, 0f, 0f);
                
                // for smooth movement of the object that we currently have selected
                pointerObject.transform.position = Vector3.Lerp(pointerObject.transform.position, hit.point,
                    pointerObject.activeSelf ? 0.55f : 1f);
                
                // TODO: store the rotation internally
                // rotate the vector around the axis of the hit normal
                
                // https://docs.unity3d.com/ScriptReference/Transform.LookAt.html
                pointerObject.transform.LookAt(hit.point + hit.normal * 100, Vector3.up);
                pointerObject.SetActive(true);
            }
            else
            {
                pointerObject.SetActive(false);
            }
        }
        
        // in normal mode, highlight selected hold
        else
        {
            pointerObject.SetActive(false);
        }
    }
}