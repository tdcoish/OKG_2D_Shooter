using UnityEngine;



// Class for the spell, not the particle.
public class PC_FireboltSpell : MonoBehaviour
{    
    public enum STATE {READY, UNREADY}
    public STATE                mState = STATE.READY;

    PC_Cont                     cPC;

    public float                _fireInterval;
    public float                mFireTmStmp;
    
    public PJ_PC_Firebolt       PF_Firebolt;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
    }

    public void FRunFireSpellUpdate()
    {
        if(mState == STATE.UNREADY){
            if(Time.time - mFireTmStmp > _fireInterval){
                mState = STATE.READY;
            }
        }
    }

    public bool FAttemptFire(Vector3 msPos, Vector3 shotPoint)
    {
        if(mState == STATE.READY){
            if(cPC.mCurMana < cPC._manaDrainPerShot){
                return false;
            }
            msPos.z = 0f;
            PJ_PC_Firebolt p = Instantiate(PF_Firebolt, shotPoint, transform.rotation);
            Vector3 vDif = msPos - shotPoint;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p._spd;

            mFireTmStmp = Time.time;
            mState = STATE.UNREADY;
            return true;
        }
        return false;
    }
}
