/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Shields : MonoBehaviour
{
    public enum STATE{FULL, BROKEN, RECHARGING}
    public STATE                            mState;

    public float                            _maxShield = 100f;
    public float                            mShieldStrength;
    public float                            _shieldBrokenTime = 3f;
    private float                           mShldBrokeTmStmp;
    public float                            _shieldRechSpd = 25f;

    private PC_Cont                         cPC;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        if(!cPC){
            Debug.Log("Shields not attached to a player");
        }
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
        if(mShieldStrength >= _maxShield)
        {
            mShieldStrength = _maxShield;
            Debug.Log("SHields full strength, not recharging.");
            mState = STATE.FULL;
        }
    }
}
