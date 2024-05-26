using UnityEngine;
using System.Collections.Generic;

public class SPL_MenorahOfPain : MonoBehaviour
{
    // Basically just fires projectiles along each of the "rails" of the attack. 
    public enum STATE{CHARGING, FIRING, FADING}
    public STATE                        mState;
    public float                        _chargeTime = 0.5f;
    public float                        mChargeTmStmp;
    public int                          _shotsToFire = 20;
    public int                          mShotCounter = 0;
    public float                        _fireInterval = 0.1f;
    public float                        mFireTmStmp;
    public float                        _fadeTime = 0.5f;
    public float                        mFadeTmStmp;
    // Spots projectiles move towards before changing direction and speeds
    public List<GameObject>             rFiringPoints;
    public PJ_WP_Menorah                PF_Projectile;
    public float                        _projStartSpd = 0.5f;
    public SpriteRenderer               rRenderer;

    void Start()
    {
        mChargeTmStmp = Time.time;
        rRenderer = GetComponent<SpriteRenderer>();
        mState = STATE.CHARGING;
    }

    void Update()
    {
        switch(mState){
            case STATE.CHARGING: FRUN_Charging(); break;
            case STATE.FIRING: FRUN_Firing(); break;
            case STATE.FADING: FRUN_Fading(); break;
        }
    }

    public void FRUN_Charging()
    {
        if(Time.time - mChargeTmStmp > _chargeTime){
            mState = STATE.FIRING;
        }
    }
    public void FRUN_Firing()
    {
        if(Time.time - mFireTmStmp > _fireInterval){
            // Fiddle with behaviour later.
            for(int i=0; i<rFiringPoints.Count; i++){
                PJ_WP_Menorah p = Instantiate(PF_Projectile, rFiringPoints[i].transform.position, transform.rotation);
                p.cRigid.velocity = transform.up * p.mProjD._spd;
            }
            mFireTmStmp = Time.time;
            mShotCounter++;
            if(mShotCounter >= _shotsToFire){
                mState = STATE.FADING;
                mFadeTmStmp = Time.time;
            }
        }
    }
    public void FRUN_Fading()
    {
        float percentFaded = (Time.time - mFadeTmStmp) / _fadeTime;
        rRenderer.color = new Color(1f, 1f, 1f, (1f-percentFaded));

        if(percentFaded >= 1f){
            Destroy(gameObject);
        }
    }
}
