/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Shields : MonoBehaviour
{
    public enum STATE{FULL, BROKEN, RECHARGING}
    public STATE                            mState;

    public float                            _maxShield = 1f;
    public float                            mShieldStrength;
    public float                            _shieldBrokenTime = 3f;
    private float                           mShldBrokeTmStmp;
    public float                            _shieldRechSpd = 0.25f;

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
            Debug.Log("Shields better now, start recharging");
            mState = STATE.RECHARGING;
        }
    }
    public void RUN_Recharging()
    {
        mShieldStrength += Time.deltaTime * _shieldRechSpd;

        if(mShieldStrength >= _maxShield)
        {
            mShieldStrength = _maxShield;
            Debug.Log("SHields full strength, not recharging.");
            mState = STATE.FULL;
        }
    }
}
