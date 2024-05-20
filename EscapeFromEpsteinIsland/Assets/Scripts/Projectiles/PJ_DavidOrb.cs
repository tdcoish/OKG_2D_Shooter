using UnityEngine;

public class PJ_DavidOrb : PJ_Base
{
    public bool                             _killSelfUponHittingOtherOrb = false;
    public void FFireOrb(Vector3 normalizedDir)
    {
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        cRigid.velocity = normalizedDir * mProjD._spd;
    }
}
