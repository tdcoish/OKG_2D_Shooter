/*************************************************************************************
Has to manage his gun. Also move around. Also do cute behaviours.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;


public class EN_Elite : Actor
{
    public enum STATE{LONG_RANGE, CLOSING, MELEEING}
    public STATE                        mState;

    public PC_Cont                      rPC;
    public EN_PRifle                    cRifle;

    public A_HealthShields              cHpShlds;

    public GameObject                   gShotPoint;
    public GameObject                   PF_Particles;

    // Copy from the EN_Knight.

    public UI_EN                        gUI;

    public override void RUN_Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRifle = GetComponent<EN_PRifle>();
        cHpShlds = GetComponent<A_HealthShields>();

        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = 75f;
        cHpShlds.mShields.mState = Shields.STATE.FULL;

        mState = STATE.LONG_RANGE;
    }

    // For now he just fires his rifle.
    public override void RUN_Update()
    {
        switch(mState){
            case STATE.LONG_RANGE: FRUN_LongRange(); break;
            case STATE.CLOSING: FRUN_Closing(); break;
            case STATE.MELEEING: FRUN_Meleeing(); break;
        }

        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);

        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);
        gUI.FUpdateShieldHealthBars(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max, cHpShlds.mShields.mStrength, cHpShlds.mShields._max, true);
    }

    void FRUN_LongRange()
    {
        cRifle.FAttemptFire(rPC, gShotPoint.transform.position);

    }
    void FRUN_Closing()
    {

    }
    void FRUN_Meleeing()
    {

    }


    // For now, just say that plasma damage does 2x to shields, 1/2 to health, and vice versa for human weapon.
    public void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        // No matter what, the shields reset the recharge. Man, "Broken" was a terrible name for this effect.
        cHpShlds.mShields.mState = Shields.STATE.BROKEN;
        cHpShlds.mShields.mBrokeTmStmp = Time.time;
        // do damage to shields first.
        float modifier = 1f;
        if(type == DAMAGE_TYPE.PLASMA){
            modifier = 2.0f;
        }
        if(type == DAMAGE_TYPE.BULLET){
            modifier = 0.5f;
        }
        // should be properly handling the spill over, but it's fine.
        float healthDam = (amt * modifier) - cHpShlds.mShields.mStrength;
        cHpShlds.mShields.mStrength -= amt * modifier;
        if(cHpShlds.mShields.mStrength < 0f) cHpShlds.mShields.mStrength = 0f;
        if(healthDam > 0f){     // shields could not fully contain the attack.
            healthDam /= modifier * modifier;
            cHpShlds.mHealth.mAmt -= healthDam;
        }
        // for now, just have the same modifier amounts, but in reverse.
        Debug.Log("Health Dam: " + healthDam);

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void RUN_TryToFire()
    {

    }

    public Shields RUN_UpdateShieldsData(Shields copiedData)
    {
        switch(copiedData.mState)
        {
            case Shields.STATE.FULL: copiedData = RUN_UpdateShieldsFull(copiedData); break;
            case Shields.STATE.RECHARGING: copiedData = RUN_UpdateShieldsRecharging(copiedData); break;
            case Shields.STATE.BROKEN: copiedData = RUN_UpdateShieldsBroken(copiedData); break;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsFull(Shields copiedData)
    {
        if(copiedData.mStrength < copiedData._max){
            Debug.Log("Mistake: Shields in full state while not fully charged.");
            copiedData.mState = Shields.STATE.BROKEN;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsBroken(Shields copiedData)
    {
        if(Time.time - copiedData.mBrokeTmStmp > copiedData._brokenTime){
            copiedData.mState = Shields.STATE.RECHARGING;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsRecharging(Shields copiedData)
    {
        copiedData.mStrength += Time.deltaTime * copiedData._rechSpd;
        if(copiedData.mStrength >= copiedData._max){
            copiedData.mStrength = copiedData._max;
            copiedData.mState = Shields.STATE.FULL;
        }
        return copiedData;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>())
        {
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            FTakeDamage(10f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PC_SwordHitbox>()){
            FTakeDamage(80f, DAMAGE_TYPE.SLASH);
        }
    }
}
