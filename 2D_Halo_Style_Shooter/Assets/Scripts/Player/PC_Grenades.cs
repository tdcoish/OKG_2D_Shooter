/*************************************************************************************
The new design is that grenades take a certain amount of mana/energy to throw.

*************************************************************************************/
using UnityEngine;

public class PC_Grenades : MonoBehaviour
{
    public float                        _cooldownTime = 4f;
    float                               mLastThrowTmStmp;
    PC_Cont                             cPC;
    public PJ_PC_Gren                   PF_Grenade;
    Rigidbody2D                         cRigid;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        cRigid = GetComponent<Rigidbody2D>();
        mLastThrowTmStmp = Time.time - _cooldownTime;
    }

    public bool FTryToThrowGrenade(Vector3 msPos, Vector3 throwStartPoint)
    {
        // msPos.z = 0f;
        if(Time.time - mLastThrowTmStmp > _cooldownTime){
            if(cPC.mCurEnergy < cPC._energyDrainPerNade){
                return false;
            }    
            mLastThrowTmStmp = Time.time;
            PJ_PC_Gren g = Instantiate(PF_Grenade, throwStartPoint, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            vDif = Vector3.Normalize(vDif);
            g.cRigid.velocity = vDif * g._spd;
            g.mState = PJ_PC_Gren.STATE.IN_AIR;
            g.mLandingSpot = msPos;

            return true;
        }

        return false;
    }
}
