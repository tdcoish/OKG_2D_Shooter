/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct Shields
{
    public enum STATE{FULL, BROKEN, RECHARGING}
    public STATE                            mState;
    public float                            _max;
    public float                            mStrength;
    public float                            _brokenTime;
    public float                            mBrokeTmStmp;
    public float                            _rechSpd;
}

public class PC_Shields : MonoBehaviour
{

    public Shields                          mShields;

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
        switch(mShields.mState)
        {
            case Shields.STATE.FULL: RUN_Full(); break;
            case Shields.STATE.BROKEN: RUN_Broken(); break;
            case Shields.STATE.RECHARGING: RUN_Recharging(); break;
        }
    }

    public void FTakeDamage(float amt)
    {
        Debug.Log("Shields took damage.");
        mShields.mStrength -= amt;
        if(mShields.mStrength < 0f) mShields.mStrength = 0f;
        mShields.mState = Shields.STATE.BROKEN;
        mShields.mBrokeTmStmp = Time.time;
    }

    public void RUN_Full()
    {
        // do nothing?
    }
    public void RUN_Broken()
    {
        if(Time.time - mShields.mBrokeTmStmp > mShields._brokenTime){
            mShields.mState = Shields.STATE.RECHARGING;
        }
    }
    public void RUN_Recharging()
    {
        mShields.mStrength += Time.deltaTime * mShields._rechSpd;

        if(mShields.mStrength >= mShields._max)
        {
            mShields.mStrength = mShields._max;
            mShields.mState = Shields.STATE.FULL;
            Debug.Log("Shields max");
        }
    }
}
