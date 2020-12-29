/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct PRifleD
{
    public GunData                          mGunD;
    public PlasmaGunData                    mPlasmaD;
}

public class EN_PRifle : MonoBehaviour
{
    private enum STATE{
        FIRING,
        COOLING_DOWN
    }
    private STATE                           mState = STATE.FIRING;

    public PJ_EN_Plasmoid                   PF_Plasmoid;

    public PRifleD                          mData;

    void Start()
    {
        mData.mGunD.mLastFireTmStmp = Time.time;
    }

    // God how I wish you could easily make functional functions in C#.
    public PRifleD FRunUpdate(PRifleD copiedData)
    {
        switch(mState)
        {
            case STATE.FIRING: copiedData = RUN_FiringState(copiedData); break;
            case STATE.COOLING_DOWN: copiedData = RUN_CooldownState(copiedData); break;
        }
        return copiedData;
    }

    public void FAttemptFire(PC_Cont rPC, Vector3 vShotPoint)
    {
        if(mState == STATE.COOLING_DOWN){
            return;
        }
        if(Time.time - mData.mGunD.mLastFireTmStmp > mData.mGunD._fireInterval){
            mData.mGunD.mLastFireTmStmp = Time.time;

            PJ_EN_Plasmoid rPlasmoid = Instantiate(PF_Plasmoid, vShotPoint, transform.rotation);
            Vector3 vDir = rPC.transform.position - vShotPoint;
            vDir = Vector3.Normalize(vDir);
            rPlasmoid.cRigid.velocity = vDir * rPlasmoid._spd;

            mData.mPlasmaD.mHeat += mData.mPlasmaD._heatPerShot;
        }
    }

    PRifleD RUN_FiringState(PRifleD copiedData)
    {

        if(copiedData.mPlasmaD.mHeat >= 100f)
        {
            mState = STATE.COOLING_DOWN;
            copiedData.mPlasmaD.mOverheatTmStmp = Time.time;
            Debug.Log("Weapon Overheated, cooling down");
        }

        return copiedData;
    }

    PRifleD RUN_CooldownState(PRifleD copiedData)
    {
        copiedData.mPlasmaD.mHeat -= copiedData.mPlasmaD._cooldownRate * Time.deltaTime;

        if(Time.time - copiedData.mPlasmaD.mOverheatTmStmp > copiedData.mPlasmaD._cooldownTime)
        {
            mState = STATE.FIRING;
            Debug.Log("Cooldown Over");
        }

        return copiedData;
    }
}
