﻿/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EN_NeedlerTurret : MonoBehaviour
{
    private enum STATE{FIRING, RELOADING}
    STATE                               mState = STATE.FIRING;

    public EN_Misc                      cMisc;
    public A_HealthShields              cHpShlds;

    public PJ_EN_Needler                PF_Needler;

    public GunData                      mGunD;
    public ClipGunData                  mClipD;


    void Start()
    {
        cHpShlds = GetComponent<A_HealthShields>();
        cMisc = GetComponent<EN_Misc>();
        mClipD.mAmt = mClipD._size;
    }

    void Update()
    {
        switch(mState){
            case STATE.FIRING: RUN_Firing(); break;
            case STATE.RELOADING: RUN_Reloading(); break;
        }

        cMisc.FUpdateUI();
    }
    
    void RUN_Firing()
    {
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval){
            // Fire Projectile.
            Debug.Log("Fired proj");
            PJ_EN_Needler p = Instantiate(PF_Needler, transform.position, transform.rotation);
            mGunD.mLastFireTmStmp = Time.time;
            p.mProjD.rOwner = gameObject;

            mClipD.mAmt--;
            if(mClipD.mAmt <= 0){
                mState = STATE.RELOADING;
                mClipD.mReloadTmStmp = Time.time;
                Debug.Log("Reloading");
            }
        }
    }

    void RUN_Reloading()
    {
        if(Time.time - mClipD.mReloadTmStmp > mClipD._reloadTime){
            mState = STATE.FIRING;
            mClipD.mAmt = mClipD._size;
            Debug.Log("Done Reloading");
        }
    }

}
