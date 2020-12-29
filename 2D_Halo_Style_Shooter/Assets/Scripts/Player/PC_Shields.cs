/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Shields : MonoBehaviour
{
    public enum STATE{FULL, BROKEN, RECHARGING}
    public STATE                            mState;

    public float                            _maxShield;
    public float                            mShieldStrength;
    public float                            _shieldBrokenTime;
    private float                           mShldBrokeTmStmp;
    public float                            _shieldRechSpd;

    private PC_Cont                         cPC;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        if(!cPC){
            Debug.Log("Shields not attached to a player");
        }
    }

    public void FRunShields()
    {
        switch(mState)
        {
            case STATE.FULL: RUN_Full(); break;
            case STATE.BROKEN: RUN_Broken(); break;
            case STATE.RECHARGING: RUN_Recharging(); break;
        }
    }

    public void FTakeDamage(float amt)
    {
        Debug.Log("Shields took damage.");
        mShieldStrength -= amt;
        Debug.Log("Shield strenght: " + mShieldStrength);
        if(mShieldStrength < 0f) mShieldStrength = 0f;
        mState = STATE.BROKEN;
        mShldBrokeTmStmp = Time.time;
    }

    public void RUN_Full()
    {
        // do nothing?
    }
    public void RUN_Broken()
    {
        if(Time.time - mShldBrokeTmStmp > _shieldBrokenTime){
            mState = STATE.RECHARGING;
        }
    }
    public void RUN_Recharging()
    {
        mShieldStrength += Time.deltaTime * _shieldRechSpd;

        if(mShieldStrength >= _maxShield)
        {
            mShieldStrength = _maxShield;
            mState = STATE.FULL;
        }
    }
}
