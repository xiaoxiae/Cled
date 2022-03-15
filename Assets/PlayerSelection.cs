using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;
    public StateManager StateManager;

    private string[] _selected;
    private int _selectedIndex;

    // the current hold object
    private GameObject pointerObject;
    
    // where the pointer object looked previously
    private Vector3 previousPointerObjectNormal = Vector3.zero;

    // the angle of the pointer object (from 0 to 2 pi)
    private float pointerObjectRotation = 0;

	public float mouseSensitivity = 1f;
    
    private bool wasSeen; 
    void Start()
    {
        // TODO: remove this, temporarily selects all of the holds
        _selected = HoldManager.Filter(hold => true);
        
        pointerObject = HoldManager.ToGameObject(_selected[_selectedIndex]);
        pointerObject.layer = 2;  // ignore this object until placed
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerControl.CurrentMode == Mode.Edit)
        {
            pointerObject.layer = 0;
            StateManager.AddHold(pointerObject);
            pointerObject = Instantiate(pointerObject);
            pointerObject.layer = 2;
        }

        if (Input.GetKey(KeyCode.LeftShift) && PlayerControl.CurrentMode == Mode.Edit)
        {
            float x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

            pointerObjectRotation = (pointerObjectRotation + x) % (2 * (float)Math.PI);
        }
        
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
                var currentNormal = Vector3.Lerp(previousPointerObjectNormal, hit.normal, 0.3f);
                
                // rotate the world "up" around the hit normal by some degrees
                var upVector = Quaternion.AngleAxis(Mathf.Rad2Deg * pointerObjectRotation, hit.normal) * Vector3.up;
                
                pointerObject.transform.LookAt(currentNormal + hit.point, upVector);
                pointerObject.SetActive(true);

                previousPointerObjectNormal = currentNormal;
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