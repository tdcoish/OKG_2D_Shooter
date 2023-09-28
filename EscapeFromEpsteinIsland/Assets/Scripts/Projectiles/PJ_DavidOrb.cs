using UnityEngine;

public class PJ_DavidOrb : PJ_Base
{

    public void FFireOrb(Vector3 normalizedDir)
    {
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        cRigid.velocity = normalizedDir * mProjD._spd;
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_DavidOrb>())
        {
            FDeath();
        }
    }
}
