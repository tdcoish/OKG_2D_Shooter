/*************************************************************************************
Spawns with an image of a six pointed star. Then, lines of orbs fly out towards all six 
points of the star.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class SPL_StarDavid2 : MonoBehaviour
{
    public enum STATE{CHARGING, FIRING, DONE}
    public STATE                            mState = STATE.CHARGING;

    public float                            mCreatedTmStmp;
    public float                            _startFiringDelay = 0.5f;
    public float                            _fireRate = 0.1f;
    public float                            _burstRate = 1f;
    [HideInInspector]
    public float                            mFireTmStmp;
    [HideInInspector]
    public float                            mBurstTmStmp;
    public int                              _totalBursts = 5;
    public int                              _shotsPerBurst = 5;
    [HideInInspector]
    public int                              mShotsFiredThisBurst = 0;
    [HideInInspector]
    public int                              mBurstsAlreadyExpelled = 0;
    public float                            _fadeawayTime = 1f;
    public float                            _rotationRateDegreesPerSecond = 30f;

    public SpriteRenderer                   rSprite;

    public List<GameObject>                 rStarPoints;
    public PJ_DavidOrb                      PF_OrbProjectile;

    void Start()
    {
        mCreatedTmStmp = Time.time;
    }

    public void Update()
    {
        switch(mState)
        {
            case STATE.CHARGING: RUN_Charging(); break;
            case STATE.FIRING: RUN_Firing(); break;
            case STATE.DONE: RUN_Done(); break;
        }
    }

    public void RUN_Charging()
    {
        float percentCharged = (Time.time - mCreatedTmStmp) / _startFiringDelay;
        float turnThisFrame = Time.deltaTime * _rotationRateDegreesPerSecond * percentCharged;
        transform.Rotate(0f,0f,turnThisFrame);
        // Still need some visual representation of the charging.

        // for(int i=0; i<rOrbs.Count; i++){
        //     SpriteRenderer sr = rOrbs[i].GetComponent<SpriteRenderer>();
        //     sr.color = new Color(1f, 1f, 1f, percentCharged);
        // }

        if(Time.time - mCreatedTmStmp > _startFiringDelay){
            ENTER_FiringState();
        }
    }

    public void ENTER_FiringState()
    {
        mState = STATE.FIRING;
        mBurstTmStmp = Time.time - _burstRate;
    }

    public void RUN_Firing()
    {
        float turnThisFrame = Time.deltaTime * _rotationRateDegreesPerSecond;
        transform.Rotate(0f,0f,turnThisFrame);

        if(Time.time - mBurstTmStmp > _burstRate){
            if(Time.time - mFireTmStmp > _fireRate){
                for(int i=0; i<rStarPoints.Count; i++){
                    // Fire projectiles away from the center.
                    PJ_DavidOrb d = Instantiate(PF_OrbProjectile, rStarPoints[i].transform.position, transform.rotation);
                    Vector2 vDir = (rStarPoints[i].transform.position - transform.position).normalized;
                    d.FFireOrb(vDir);
                }

                mFireTmStmp = Time.time;
                mShotsFiredThisBurst++;
                if(mShotsFiredThisBurst >= _shotsPerBurst){
                    mBurstsAlreadyExpelled++;
                    mBurstTmStmp = Time.time;
                    if(mBurstsAlreadyExpelled >= _totalBursts){
                        mState = STATE.DONE;
                    }
                    mShotsFiredThisBurst = 0;
                }
            }
        }
       
    }
    public void RUN_Done()
    {
        // Not sure. I guess we just fade away?
        float percentFaded = (Time.time - mFireTmStmp) / _fadeawayTime;
        rSprite.color = new Color(1f, 1f, 1f, (1f-percentFaded));
        float turnThisFrame = Time.deltaTime * _rotationRateDegreesPerSecond * (1f-percentFaded);
        transform.Rotate(0f,0f,turnThisFrame);

        if(percentFaded >= 1f){
            Destroy(gameObject);
        }
    }
}
