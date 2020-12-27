/*************************************************************************************
Plasma rifle. It shoots plasma bolts.
*************************************************************************************/
using UnityEngine;

public class PC_PRifle : MonoBehaviour
{
    public enum STATE{CAN_FIRE, OVERHEATED}
    public STATE                                    mState;

    public float                                    _maxHeat;
    public float                                    _heatPerShot;
    public float                                    mHeat;
    public float                                    _fireInterval;
    public float                                    mLastFireTmStmp;
    public float                                    _cooldownRate;          // not the same as below, we don't need a full cooldown from overheating.
    public float                                    _cooldownTime;
    private float                                   mOverheatTmStmp;

    public PJ_PC_Plasmoid                           PF_Plasmoid;

    public void FRunGun()
    {
        switch(mState)
        {
            case STATE.CAN_FIRE: RUN_CanFire(); break;
            case STATE.OVERHEATED: RUN_Overheated(); break;
        }
    }

    public void FAttemptFire(Vector3 msPos)
    {
        msPos.z = 0f;

        if(mState != STATE.CAN_FIRE){
            return;
        }
        if(Time.time - mLastFireTmStmp > _fireInterval)
        {
            msPos.z = 0f;
            PJ_PC_Plasmoid p = Instantiate(PF_Plasmoid, transform.position, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p._spd;

            mLastFireTmStmp = Time.time;
            
            mHeat += _heatPerShot;

            if(mHeat > _maxHeat){
                Debug.Log("Gun overheating");
                mOverheatTmStmp = Time.time;
                mState = STATE.OVERHEATED;
                mHeat = _maxHeat;
            }
        }
    }

    void RUN_CanFire()
    {
        mHeat -= _cooldownRate * Time.deltaTime;
        if(mHeat < 0f) mHeat = 0f;
    }
    void RUN_Overheated()
    {
        mHeat -= _cooldownRate * Time.deltaTime;

        if(Time.time - mOverheatTmStmp > _cooldownTime){
            Debug.Log("Cooled Down");
            mState = STATE.CAN_FIRE;
        }
    }
}
