/*************************************************************************************
Plasma rifle. It shoots plasma bolts.
*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct PlasmaGunData
{
    public float                                    _maxHeat;
    public float                                    _heatPerShot;
    public float                                    mHeat;
    public float                                    _cooldownRate;          // not the same as below, we don't need a full cooldown from overheating.
    public float                                    _cooldownTime;
    public float                                    mOverheatTmStmp;
}

public class PC_PRifle : MonoBehaviour
{
    public enum STATE{CAN_FIRE, OVERHEATED}
    public STATE                                    mState;

    public GunData                                  mGunD;
    public PlasmaGunData                            mPlasmaD;

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
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval)
        {
            msPos.z = 0f;
            PJ_PC_Plasmoid p = Instantiate(PF_Plasmoid, transform.position, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p._spd;

            mGunD.mLastFireTmStmp = Time.time;
            
            mPlasmaD.mHeat += mPlasmaD._heatPerShot;

            if(mPlasmaD.mHeat > mPlasmaD._maxHeat){
                Debug.Log("Gun overheating");
                mPlasmaD.mOverheatTmStmp = Time.time;
                mState = STATE.OVERHEATED;
                mPlasmaD.mHeat = mPlasmaD._maxHeat;
            }
        }
    }

    void RUN_CanFire()
    {
        mPlasmaD.mHeat -= mPlasmaD._cooldownRate * Time.deltaTime;
        if(mPlasmaD.mHeat < 0f) mPlasmaD.mHeat = 0f;
    }
    void RUN_Overheated()
    {
        mPlasmaD.mHeat -= mPlasmaD._cooldownRate * Time.deltaTime;

        if(Time.time - mPlasmaD.mOverheatTmStmp > mPlasmaD._cooldownTime){
            Debug.Log("Cooled Down");
            mState = STATE.CAN_FIRE;
        }
    }
}
