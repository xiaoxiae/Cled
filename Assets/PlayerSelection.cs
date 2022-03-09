using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public PlayerControl PlayerControl;
    public HoldManager HoldManager;

    // TODO: a queue of the currenly selected holds
    // TODO: the index in the given queue where we are right now


    private bool wasSeen;

    void Update()
    {
        //// in insert mode, do stuff with placing the hold
        //if (PlayerControl.CurrentMode == Mode.Insert)
        //{
        //    // ignore the raycast ignore layer
        //    int layerMask = ~(1 << 2);

        //    var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit, layerMask))
        //    {
        //        // for smooth movement
        //        pointerObjectTransform.position = Vector3.Lerp(pointerObjectTransform.position, hit.point,
        //            pointerObject.activeSelf ? 0.55f : 1f);
        //        
        //        pointerObjectTransform.LookAt(hit.point + hit.normal * 100);
        //        pointerObject.SetActive(true);
        //    }
        //    else
        //    {
        //        pointerObject.SetActive(false);
        //    }
        //}
        //
        //// in normal mode, highlight selected hold
        //else
        //{
        //    pointerObject.SetActive(false);
        //}
    }
}