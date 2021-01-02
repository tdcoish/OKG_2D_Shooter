/*************************************************************************************
Handles the throwing of grenades.
*************************************************************************************/
using UnityEngine;

public class PC_Grenades : MonoBehaviour
{
    // Kinda ironic, but we're "shooting" grenades all the same.
    public GunData                          mGunD;

    public PJ_PC_FGren                      PF_Grenade;

    void Start()
    {
        
    }

    public void FTryToThrowGrenade(Vector3 msPos)
    {
        msPos.z = 0f;
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval)
        {
            mGunD.mLastFireTmStmp = Time.time;
            PJ_PC_FGren p = Instantiate(PF_Grenade, transform.position, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p.mGrenD._spdInAir;
            p.mGrenD.mState = GrenadeData.STATE.IN_AIR;
            p.mGrenD.vDest = msPos;
            
            // we own the grenades, the explosion can still hurt us.
            p.GetComponent<PJ_Base>().mProjD.rOwner = gameObject;
        }
    }
}
