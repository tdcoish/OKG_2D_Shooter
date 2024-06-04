/*
Wait, we can always just make them turn to their transform.left/transform.right, so there's a much 
better way of doing this.
*/

using UnityEngine;

public class PJ_Weave : PJ_Base
{
    // A bad term, but basically if they're turning to their left, or turning to their right.
    public float                    mTransformRightMultiple;
    public float                    _turnRate;
    public float                    mCreatedTmStmp;

    public void FShootMe(Vector2 destPos, Vector2 startingPos, Vector2 vStartDir)
    {
        transform.up = vStartDir.normalized;
        Vector2 vStraightToGoal = (destPos - startingPos).normalized;
        if(Vector2.Dot(vStraightToGoal, transform.right) > 0f){
            mTransformRightMultiple = 1f;
        }else{
            mTransformRightMultiple = -1f;
        }

        cRigid.velocity = vStartDir.normalized * mProjD._spd;
        // This is the diameter of the circular path.
        float dis = Vector2.Distance(startingPos, destPos);
        // Path is full circle in this case.
        float lengthOfPath = Mathf.PI * dis;
        float timeTillHit = lengthOfPath / mProjD._spd;
        float totalAngleChange = 360f;
        float angleChangePerSecond = totalAngleChange / timeTillHit;
        _turnRate = angleChangePerSecond;

        Destroy(gameObject, timeTillHit);
    }

    public void Update()
    {
        Vector2 vNewHeading = Vector3.RotateTowards(transform.up, transform.right * mTransformRightMultiple, (Mathf.Deg2Rad* _turnRate)*Time.deltaTime, 0f); 
        cRigid.velocity = vNewHeading.normalized * mProjD._spd;
        transform.up = vNewHeading.normalized;
    }
}
