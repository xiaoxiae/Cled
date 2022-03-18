using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public enum Mode
{
    Normal, // the usual mode that we're in
    Holding, // when we're holding a hold
    Route, // when we're looking at an entire route
}

public class PlayerSelection : MonoBehaviour
{
    public HoldManager HoldManager;
    public StateManager StateManager;
    public CameraControl CameraControl;
    public HighlightManager HighlightManager;
    public RouteManager RouteManager;

    public Mode CurrentMode = Mode.Normal;

    private Hold[] _selected;

    public float mouseSensitivity = 1f;

    void Start()
    {
        _selected = HoldManager.Filter(hold => true);
    }

    void Update()
    {
        // toggle between holding and normal with e press
        if (Input.GetKeyDown(KeyCode.E))
        {
            CurrentMode = CurrentMode == Mode.Holding ? Mode.Normal : Mode.Holding;

            if (CurrentMode == Mode.Normal)
                StateManager.SetUnheld(true);
            else
                StateManager.SetHeld(_selected[0]);
        }

        // cast a ray directly in front of the camera
        var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var hitObject = hit.collider.gameObject;

            // if we hit an object that isn't highlighted, dehighlight everything
            if (!HighlightManager.IsHighlighted(hitObject))
                HighlightManager.UnhighlightAll();

            switch (CurrentMode)
            {
                case Mode.Holding:
                    StateManager.EnableHeld();
                    
                    // when in holding mode, move the held hold accordingly
                    StateManager.InterpolateHeldToHit(hit);
                    
                    // place held hold and go to normal mode 
                    if (Input.GetMouseButtonDown(0))
                    {
                        StateManager.InterpolateHeldToHit(hit, 0);
                            
                        StateManager.PutDown();
                        CurrentMode = Mode.Normal;
                    }

                    break;
                case Mode.Normal:
                {
                    if (StateManager.IsPlacedHold(hitObject))
                    {
                        // highlight hold if in normal mode
                        HighlightManager.Highlight(hitObject);

                        // when left clicking a hold in normal mode, snap back to holding mode
                        if (Input.GetMouseButtonDown(0))
                        {
                            CurrentMode = Mode.Holding;

                            StateManager.PickUp(hitObject);
                            CameraControl.LookAt(hitObject.transform.position);
                        }
                    }

                    break;
                }
            }
        }
        else
        {
            switch (CurrentMode)
            {
                case Mode.Normal:
                    HighlightManager.UnhighlightAll();
                    break;
                case Mode.Holding:
                    StateManager.DisableHeld();
                    break;
            }
        }

        // even when the ray didn't hit anything, do some stuff
        if (CurrentMode == Mode.Holding)
        {
            // rotate hold on shift press
            if (Input.GetKey(KeyCode.LeftShift))
                StateManager.RotateHeld(Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime);
        }
    }
}