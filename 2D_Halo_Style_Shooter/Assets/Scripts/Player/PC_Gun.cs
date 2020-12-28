/*************************************************************************************
This one will be the assault rifle. I'll do the plasma rifle later.
*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct GunData
{
    public float                    _fireInterval;
    public float                    mLastFireTmStmp;

    public bool                     mIsActive;
}
[System.Serializable]
public struct ClipGunData
{
    public float                    _reloadTime;

    [HideInInspector]
    public float                    mReloadTmStmp;
    public int                      _clipSize;

    [HideInInspector]
    public int                      mClipAmt;
}

public class PC_Gun : MonoBehaviour
{
    public enum STATE{CAN_FIRE, RELOADING}
    public STATE                    mState;

    public PJ_PC_Bullet             PF_Bullet;

    public GunData                mGunD;
    public ClipGunData            mClipD;

    void Start()
    {
        mClipD.mClipAmt = mClipD._clipSize;
    }

    public void FRunGun()
    {
        switch(mState)
        {
            case STATE.CAN_FIRE: RUN_CanFire(); break;
            case STATE.RELOADING: RUN_Reloading(); break;
        }
    }

    public void FAttemptFire(Vector3 msPos)
    {
        if(mState != STATE.CAN_FIRE){
            return;
        }
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval)
        {
            msPos.z = 0f;
            PJ_PC_Bullet p = Instantiate(PF_Bullet, transform.position, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p._spd;

            mGunD.mLastFireTmStmp = Time.time;
            mClipD.mClipAmt--;
        }
        if(mClipD.mClipAmt <= 0){
            mClipD.mReloadTmStmp = Time.time;
            mState = STATE.RELOADING;
        }
    }

    public void FAttemptReload()
    {

        if(mState == STATE.RELOADING){
            Debug.Log("Can't reload, already reloading");
            return;
        }

        if(mClipD.mClipAmt == mClipD._clipSize)
        {
            Debug.Log("Can't reload, full");
            return;
        }else{
            Debug.Log("Reloading");
            mClipD.mReloadTmStmp = Time.time;
            mState = STATE.RELOADING;
        }
    }

    void RUN_CanFire()
    {
        // Might happen if we change states when the gun is empty.
        if(mClipD.mClipAmt <= 0){
            mClipD.mReloadTmStmp = Time.time;
            mState = STATE.RELOADING;
        }
    }

    void RUN_Reloading()
    {
        if(!mGunD.mIsActive){
            Debug.Log("Cna't reload, not active");
            mState = STATE.CAN_FIRE;
        }
        if(Time.time - mClipD.mReloadTmStmp > mClipD._reloadTime){
            Debug.Log("Done reloading");
            mState = STATE.CAN_FIRE;
            mClipD.mClipAmt = mClipD._clipSize;
        }
    }



}
