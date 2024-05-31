/*****************
Is given a spot that it needs to get to. Then, finds out how to fly a circular path to that destination.
The math is interesting. I essentially need to find the path of a circle, and I know the diameter is 
just the distance to the final spot.

It has been successfully created, and this code will stand as an example. However, I'm not sure that a 
purely math based solution is what I actually wanted. The problem is that you can still effectively dodge 
these projectiles by moving to the side. They would need to go deeper, and come at the player flatter, 
to accomplish the goal of forcing forwards/backwards strafing.

Alternatively, we just make the projectile much wider than straight, so it can better catch the player.
*****************/
using UnityEngine;

public class PJ_CirclePath : PJ_Base
{

    public Vector2                  pAimSpot;
    public float                    _turnRate;

    public void FShootMe(Vector2 destPos, Vector2 startingPos, Vector2 vStartDir)
    {
        cRigid.velocity = vStartDir.normalized * mProjD._spd;

        pAimSpot = destPos;
        // This is the diameter of the circular path.
        float dis = Vector2.Distance(startingPos, pAimSpot);
        // Path is semi-circle
        float lengthOfPath = Mathf.PI * dis / 2f;
        float timeTillHit = lengthOfPath / mProjD._spd;

        // Since it will be going in the opposite direction at the end.
        float totalAngleChange = 180f;

        float angleChangePerSecond = totalAngleChange / timeTillHit;
        _turnRate = angleChangePerSecond;

        // Have to do this, since we figure out the lifespan upon shooting.
        Destroy(gameObject, timeTillHit);
    }

    public void Update()
    {
        // transform.rotation = Quaternion.LookRotation(cRigid.velocity.normalized);
        Vector2 vDif = pAimSpot - (Vector2)transform.position;
        float angleDif = Vector3.Angle(cRigid.velocity.normalized, vDif.normalized);
        Vector2 vNewHeading = Vector3.RotateTowards(cRigid.velocity.normalized, vDif.normalized, (Mathf.Deg2Rad* _turnRate)*Time.deltaTime, 0f); 
        cRigid.velocity = vNewHeading.normalized * mProjD._spd;
        transform.up = vNewHeading.normalized;

    }

}
