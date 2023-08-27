/************************************************************************************************************************************************
For now this does nothing except debug. Eventually we need to figure out some things such as the proper width, which depends on the user's aspect ratio.

Right now for a 16:9 aspect ratio, with equal space on both sides of the camera, we need Viewport Rect values of:
x: 0.21875, Y: 0
W: 0.5625, H: 1.

The height should always be 1, unless I eventually decide to put something above or below the main camera. The width depends on the aspect ratio, 
and how much space is used by other things on each side.

Since I'm moving the camera all the way over to the left so I can make a larger minimap, I've decided to set the X value to 0, and disable the 
"leftbackground element in the UI_HUD gameobject.

New values: x: 0, Y: 0, W: 0.5625, H: 1.
*************************************************************************************************************************************************/


using UnityEngine;

public class CAM_Center : MonoBehaviour
{
    
    void Start()
    {
        Debug.Log("Screen width: " + Screen.width);
        Debug.Log("Screen height: " + Screen.height);
    }
}
