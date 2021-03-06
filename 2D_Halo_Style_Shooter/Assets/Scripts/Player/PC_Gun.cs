﻿/*************************************************************************************
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
    public int                      _size;

    [HideInInspector]
    public int                      mAmt;
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
        mClipD.mAmt = mClipD._size;
    }

    public void FRunGun()
    {
        switch(mState)
        {
            case STATE.CAN_FIRE: RUN_CanFire(); break;
            case STATE.RELOADING: RUN_Reloading(); break;
        }
    }

    public void FAttemptFire(Vector3 msPos, Vector3 shotPoint)
    {
        if(mState != STATE.CAN_FIRE){
            return;
        }
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval)
        {
            msPos.z = 0f;
            PJ_PC_Bullet p = Instantiate(PF_Bullet, shotPoint, transform.rotation);
            Vector3 vDif = msPos - shotPoint;
            vDif = Vector3.Normalize(vDif);
            p.cRigid.velocity = vDif * p.mProjD._spd;

            mGunD.mLastFireTmStmp = Time.time;
            mClipD.mAmt--;
        }
        if(mClipD.mAmt <= 0){
            mClipD.mReloadTmStmp = Time.time;
            mState = STATE.RELOADING;
        }
    }

    public void FResetReload()
    {
        Debug.Log("Reload interrupted");
        mClipD.mReloadTmStmp = Time.time;
    }
    public void FAttemptReload()
    {

        if(mState == STATE.RELOADING){
            Debug.Log("Can't reload, already reloading");
            return;
        }

        if(mClipD.mAmt == mClipD._size)
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
        if(mClipD.mAmt <= 0){
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
            mClipD.mAmt = mClipD._size;
        }
    }



}
