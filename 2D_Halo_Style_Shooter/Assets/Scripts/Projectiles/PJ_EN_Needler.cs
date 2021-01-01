/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_EN_Needler : PJ_Base
{
    private PC_Cont                     rPC;
    public float                        _turnRate;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();

        // This is incredibly important due to how I do this later on.
        Vector2 vDif = rPC.transform.position - transform.position;
        cRigid.velocity = vDif.normalized * mProjD._spd;  
    }

    // for now, perfect homing on the player. Eventually, maximum angle turn per second/rate.
    void Update()
    {
        // transform.rotation = Quaternion.LookRotation(cRigid.velocity.normalized);
        Vector2 vDif = rPC.transform.position - transform.position;

        float angleDif = Vector3.Angle(cRigid.velocity.normalized, vDif.normalized);
        Vector2 vNewHeading = Vector3.RotateTowards(cRigid.velocity.normalized, vDif.normalized, (Mathf.Deg2Rad* _turnRate)*Time.deltaTime, 0f); 
        cRigid.velocity = vNewHeading.normalized * mProjD._spd;

        // cRigid.velocity = vDif * _spd;
    }
}
