/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EN_NeedlerTurret : MonoBehaviour
{
    private enum STATE{FIRING, RELOADING}
    STATE                               mState = STATE.FIRING;

    private PC_Cont                     rPC;

    public PJ_EN_Needler                PF_Needler;

    public float                        _fireRate = 1f;
    private float                       mFireTmStmp;
    public float                        _reloadTime = 3f;
    private float                       mReloadTmStmp;
    public int                          _magSize = 20;
    private int                         mNeedlesInMag;
    
    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        mNeedlesInMag = _magSize;
    }

    void Update()
    {
        switch(mState){
            case STATE.FIRING: RUN_Firing(); break;
            case STATE.RELOADING: RUN_Reloading(); break;
        }
    }
    
    void RUN_Firing()
    {
        if(Time.time - mFireTmStmp > _fireRate){
            // Fire Projectile.
            Debug.Log("Fired proj");
            Instantiate(PF_Needler, transform.position, transform.rotation);
            mFireTmStmp = Time.time;

            mNeedlesInMag--;
            if(mNeedlesInMag <= 0){
                mState = STATE.RELOADING;
                mReloadTmStmp = Time.time;
                Debug.Log("Reloading");
            }
        }
    }

    void RUN_Reloading()
    {
        if(Time.time - mReloadTmStmp > _reloadTime){
            mState = STATE.FIRING;
            mNeedlesInMag = _magSize;
            Debug.Log("Done Reloading");
        }
    }
}
