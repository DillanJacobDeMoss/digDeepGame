using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    /*These transforms should always be visible to the camera and represent the
     *North & East bounds of the camera space
     */
    public Transform nodeNorth;
    public Transform nodeEast;

    private Camera cam;

    /*values that describe where the nodes are relative to the camera space
     *A value of 1 means that the node is DIRECTLY on the edge of the camera
     */
    private float northVal;
    private float eastVal;

    //the default camera size is 12 units
    private float defaultCamSize = 12f;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        northVal = cam.WorldToViewportPoint(nodeNorth.position).y;
        eastVal = cam.WorldToViewportPoint(nodeEast.position).x;
        checkForResize();
    }

    private void checkForResize()
    {
        //if either of the two values is greater than 1, then one of the nodes is not visible
        if(northVal > 1 || eastVal > 1)
        {
            //pass greater value to camera change function
            if (northVal > eastVal)
            {
                changeCameraSize(northVal);
            }
            else
            {
                changeCameraSize(eastVal);
            }
        }

        //if Both nodes are less than a certain value than the camera is probably too large
        if(northVal < 0.99 && eastVal < 0.99)
        {
            //pass lesser value to camera change function
            if (northVal < eastVal)
            {
                changeCameraSize(northVal);
            }
            else
            {
                changeCameraSize(eastVal);
            }
        }
    }

    private void changeCameraSize(float referenceValue)
    {
        cam.orthographicSize = referenceValue * cam.orthographicSize;
    }
}
