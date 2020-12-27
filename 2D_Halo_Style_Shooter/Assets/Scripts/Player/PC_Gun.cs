/*************************************************************************************
This one will be the assault rifle. I'll do the plasma rifle later.
*************************************************************************************/
using UnityEngine;

public class PC_Gun : MonoBehaviour
{
    public enum STATE{CAN_FIRE, RELOADING}
    public STATE                    mState;

    public PJ_PC_Bullet             PF_Bullet;

    public float                    _fireInterval;      // gap between shots, not RPS.
    private float                   mLastFireTmStmp;
    public float                    _reloadTime;
    [HideInInspector]
    public float                    mReloadTmStmp;
    public int                      _clipSize;
    [HideInInspector]
    public int                      mClipAmt;

    void Start()
    {
        mClipAmt = _clipSize;
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
        if(Time.time - mLastFireTmStmp > _fireInterval)
        {
            PJ_PC_Bullet p = Instantiate(PF_Bullet, transform.position, transform.rotation);
            Vector3 vDif = msPos - transform.position;
            p.cRigid.velocity = vDif.normalized * p._spd;

            mLastFireTmStmp = Time.time;
            mClipAmt--;
            if(mClipAmt <= 0){
                mReloadTmStmp = Time.time;
                mState = STATE.RELOADING;
            }
        }
    }

    public void FAttemptReload()
    {
        if(mClipAmt == _clipSize)
        {
            Debug.Log("Can't reload, full");
            return;
        }else{
            Debug.Log("Reloading");
            mReloadTmStmp = Time.time;
            mState = STATE.RELOADING;
        }
    }

    void RUN_CanFire()
    {
    }

    void RUN_Reloading()
    {
        if(Time.time - mReloadTmStmp > _reloadTime){
            Debug.Log("Done reloading");
            mState = STATE.CAN_FIRE;
            mClipAmt = _clipSize;
        }
    }



}
