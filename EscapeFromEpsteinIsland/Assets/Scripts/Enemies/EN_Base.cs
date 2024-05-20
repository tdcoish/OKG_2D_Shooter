using UnityEngine;
using System.Collections.Generic;

public class EN_Base : Actor
{
    // Theoretically could have states of ALIVE, STUNNED, and DEATH_ANIMATION, or something like that.
    // But we'll cross that bridge when we get there.
    public uint                         kActive = 1<<0;
    public uint                         kStunned = 1<<1; 
    public uint                         kState;
    public float                        _stunRecTime = 1f;
    public float                        mStunTmStmp;
    public GameObject                   gShotPoint;
    public GameObject                   PF_Particles;
    public Rigidbody2D                  cRigid;
    public A_HealthShields              cHpShlds;
    public float                        _spd = 1f;
    public List<Vector2Int>             mPath;
    public UI_EN                        gUI;

    public virtual void F_CharSpecStart(){}
    public virtual void F_CharSpecUpdate(){}

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        cHpShlds = GetComponent<A_HealthShields>();
        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = cHpShlds.mShields._max;
        cHpShlds.mShields.mState = Shields.STATE.FULL;
        
        F_CharSpecStart();
    }

    public override void RUN_Update()
    {
        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);
        gUI.FUpdateShieldHealthBars(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max, cHpShlds.mShields.mStrength, cHpShlds.mShields._max, cHpShlds._hasShieldsEver);

        F_CharSpecUpdate();
    }

    // Simplifying to ignore damage type for now.
    // Making them not flinch when the shields take all the damage. 
    public void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        // No matter what, the shields reset the recharge. Man, "Broken" was a terrible name for this effect.
        cHpShlds.mShields.mState = Shields.STATE.BROKEN;
        cHpShlds.mShields.mBrokeTmStmp = Time.time;

        // should be properly handling the spill over, but it's fine.
        float healthDam = amt - cHpShlds.mShields.mStrength;
        cHpShlds.mShields.mStrength -= amt;
        if(cHpShlds.mShields.mStrength < 0f) cHpShlds.mShields.mStrength = 0f;
        if(healthDam > 0f){     // shields could not fully contain the attack.
            cHpShlds.mHealth.mAmt -= healthDam;
            ENTER_Stun();
        }

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            if(GetComponent<EN_BPBertha>() != null){
                GetComponent<EN_BPBertha>().F_Death();
            }else{
                rOverseer.FRegisterDeadEnemy(this);
            }
        }
    }
    public void ENTER_Stun()
    {
        // for now we make them stunned each time, except the exploding bertha
        if(GetComponent<EN_BPBertha>()){
            if(GetComponent<EN_BPBertha>().kState == GetComponent<EN_BPBertha>().kPreExplosion){
                return;
            }
        }
        if(GetComponent<EN_Beamer>()){
            GetComponent<EN_Beamer>().cLineRender.enabled = false;
        }

        kState = kStunned;
        mStunTmStmp = Time.time;
        cRigid.velocity = Vector2.zero;

        if(GetComponent<EN_Elite>()){
            GetComponent<EN_Elite>().rBatonHitbox.gameObject.SetActive(false);
        }
    }

    public void F_RunStunRecovery()
    {
        if(Time.time - mStunTmStmp > _stunRecTime){
            EXIT_Stun();
        }
    }

    public virtual void EXIT_Stun(){}

    public void FAcceptHolyWaterDamage(float amt)
    {
        FTakeDamage(amt, DAMAGE_TYPE.HOLYWATER);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>()){
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            PJ_PC_Firebolt f = col.GetComponent<PJ_PC_Firebolt>();
            FTakeDamage(10f, DAMAGE_TYPE.PLASMA);
            f.F_Die();
        }else if(col.GetComponent<PC_SwordHitbox>()){
            FTakeDamage(80f, DAMAGE_TYPE.SLASH);
            col.GetComponentInParent<PC_Cont>().FHeal(col.GetComponentInParent<PC_Melee>()._healAmtFromSuccessfulHit);
        }else if(col.GetComponent<EX_PlayerMine>()){
            FTakeDamage(80f, DAMAGE_TYPE.EXPLOSION);   
        }else if(col.GetComponent<WP_RotatingOrb>()){
            WP_RotatingOrb o = col.GetComponent<WP_RotatingOrb>();
            if(o.rPC.mState == PC_RotOrbController.STATE.COLD) return;
            FTakeDamage(o._damage, DAMAGE_TYPE.EXPLOSION);  
            o.FRegisterHitEnemy(); 
        }else if(col.GetComponent<PJ_PC_RadShot>()){
            PJ_PC_RadShot temp = col.GetComponent<PJ_PC_RadShot>();
            FTakeDamage(temp.mProjD._damage, temp.mProjD._DAM_TYPE); 
            temp.FDeath();
        }else if(col.GetComponent<PJ_Base>()){
            //--------- making enemies not do friendly fire
            return;
            //------------
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    return;
                }
            }
            // Note, will have to change a bit for the needler.
            if(p.mProjD._DAM_TYPE != DAMAGE_TYPE.NO_DAMAGE){
                FTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);
            }

            p.FDeath();
        }
    }

}
