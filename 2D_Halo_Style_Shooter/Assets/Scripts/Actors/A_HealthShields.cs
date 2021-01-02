/*************************************************************************************
A health/shields component. I'm just going to say that this thing gets to have full access, because
C# is a shitty language that loves side effects.
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

[System.Serializable]
public struct Health
{
    public float                        _max;
    public float                        mAmt;
}

public class A_HealthShields : MonoBehaviour
{
    public bool                             _hasShieldsEver = false;

    public Shields                          mShields;
    public Health                           mHealth;

    // This is going to eventually have a shitload of side effects. Prepare thine anus.
    public void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        float healthDam = 0f;

        if(_hasShieldsEver && mShields.mStrength > 0f)
        {
            float leftoverDamage = 0f;

            // No matter what, the shields reset the recharge. Man, "Broken" was a terrible name for this effect.
            mShields.mState = Shields.STATE.BROKEN;
            mShields.mBrokeTmStmp = Time.time;
            // do damage to shields first.
            float modifier = 1f;
            if(type ==  DAMAGE_TYPE.PLASMA){
                modifier = 2.0f;
            }
            if(type == DAMAGE_TYPE.BULLET){
                modifier = 0.5f;
            }

            mShields.mStrength -= amt * modifier;
            if(mShields.mStrength < 0f){
                Debug.Log("Damage spilled over");
                leftoverDamage = Mathf.Abs(mShields.mStrength);
                leftoverDamage /= modifier;         // to get pure damage.
                healthDam = leftoverDamage;
                healthDam /= modifier;              // for now always assume plasma/bullet.
                mShields.mStrength = 0f;
            }
        }else{
            healthDam = amt;
            if(type == DAMAGE_TYPE.PLASMA){
                healthDam *= 0.5f;
            }else if(type == DAMAGE_TYPE.BULLET){
                healthDam *= 2.0f;
            }
        }

        mHealth.mAmt -= healthDam;
        // Debug.Log("Health Dam: " + healthDam);
    }

    public Shields FRUN_UpdateShieldsData(Shields copiedData)
    {
        switch(copiedData.mState)
        {
            case Shields.STATE.FULL: copiedData = FRUN_UpdateShieldsFull(copiedData); break;
            case Shields.STATE.RECHARGING: copiedData = FRUN_UpdateShieldsRecharging(copiedData); break;
            case Shields.STATE.BROKEN: copiedData = FRUN_UpdateShieldsBroken(copiedData); break;
        }
        return copiedData;
    }

    public Shields FRUN_UpdateShieldsFull(Shields copiedData)
    {
        if(copiedData.mStrength < copiedData._max){
            Debug.Log("Mistake: Shields in full state while not fully charged.");
            copiedData.mState = Shields.STATE.BROKEN;
        }
        return copiedData;
    }

    public Shields FRUN_UpdateShieldsBroken(Shields copiedData)
    {
        if(Time.time - copiedData.mBrokeTmStmp > copiedData._brokenTime){
            copiedData.mState = Shields.STATE.RECHARGING;
        }
        return copiedData;
    }

    public Shields FRUN_UpdateShieldsRecharging(Shields copiedData)
    {
        copiedData.mStrength += Time.deltaTime * copiedData._rechSpd;
        if(copiedData.mStrength >= copiedData._max){
            copiedData.mStrength = copiedData._max;
            copiedData.mState = Shields.STATE.FULL;
        }
        return copiedData;
    }
}
