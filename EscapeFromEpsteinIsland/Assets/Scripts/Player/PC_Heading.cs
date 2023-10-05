/************************************************************************************************
I really hate games where you have to whip the mouse back and forth to execute properly, so in this 
game I'm just not going to do that. Instead, there will be two limitations on movement. 

First, the player character can only rotate at a certain speed. If the mouse is moved side to side 
above that speed, it takes them a while to catch up. 

Second, the mouse can only move forward and backwards at a certain rate. So if you want to throw 
a holy water at your feet, then another at the edge of the screen, then you can't just snap your mouse
there, it's going to take a bit of time.
************************************************************************************************/


using UnityEngine;

public class PC_Heading : MonoBehaviour
{
    public float                            _maxTurnRateInDegreesPerSecond = 10f;
    public float                            _maxVertDistanceInUnitsPerSecond = 0.5f;
    public Vector2                          mCurHeadingSpot;

    public void F_Start()
    {
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mCurHeadingSpot = msPos;
    }

    public void FUpdateHeadingSpot()
    {   
        // For now we shift the heading a bit off the actual mouse position.
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // First, figure out what the new heading should be.
        Vector2 vDirToMsPos = (msPos - (Vector2)transform.position).normalized;
        Vector2 vDirToCurTargetPos = (mCurHeadingSpot - (Vector2)transform.position).normalized;
        float turnThisFrame = Time.deltaTime * _maxTurnRateInDegreesPerSecond / Mathf.Rad2Deg;
        Vector3 vNewDir = Vector3.RotateTowards(vDirToCurTargetPos, vDirToMsPos, turnThisFrame, 0.0f);

        // Second, figure out the new distance to the heading spot.
        float disToMs = Vector2.Distance(msPos, transform.position);
        float disToCurTargetPos = Vector2.Distance(mCurHeadingSpot, transform.position);
        float disDifference = disToMs - disToCurTargetPos;
        float newDis;
        if(Mathf.Abs(disDifference) < Time.deltaTime * _maxVertDistanceInUnitsPerSecond){
            newDis = disToMs;
        }else{
            // Have to normalize the unit, since we just want to know if we're increasing or decreasing.
            float mag = 1f; if(disDifference < 0f) mag = -1f;
            // Find the distance between the player and the current target position
            newDis = disToCurTargetPos + (mag * Time.deltaTime * _maxVertDistanceInUnitsPerSecond);
        }
        // The new heading spot is just the combination of the distance and heading.
        mCurHeadingSpot = transform.position + vNewDir * newDis;

        transform.up = vNewDir;
    }

}
