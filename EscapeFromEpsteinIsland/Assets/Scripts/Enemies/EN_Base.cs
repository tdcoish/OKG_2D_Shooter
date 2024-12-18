using UnityEngine;
using System.Collections.Generic;

public class EN_Base : Actor
{
    public int                          _endlessScore = 1;
    public int                          _arcadeKillScore = 100;
    // Theoretically could have states of ALIVE, STUNNED, and DEATH_ANIMATION, or something like that.
    // But we'll cross that bridge when we get there.
    public uint                         kActive = 1<<0;
    public uint                         kPoiseBroke = 1<<1; 
    // public uint                         
    public uint                         kState;
    public float                        _maxPoise = 0f;
    public float                        mPoise;
    public float                        _poiseRecPercentPerSec = 10f;
    public float                        mStunTmStmp;
    public float                        _poiseRecTime = 1f;
    public float                        _stunInitialVelocity = 1f;
    public float                        _stunMeleeAccMult = 5f;
    public float                        mStunInitialVel;
    public float                        _wallDamagePerUnitVelocity = 20f;
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
        mPoise = _maxPoise;

        F_CharSpecStart();
    }

    public override void RUN_Update()
    {
        void RecAppropriateAmountOfPoise()
        {
            float poiseRecPerSec = _poiseRecPercentPerSec * _maxPoise;
            mPoise += poiseRecPerSec * 0.01f * Time.deltaTime;
            if(mPoise > _maxPoise) mPoise = _maxPoise;
        }
        RecAppropriateAmountOfPoise();
        cHpShlds.FRUN_UpdateShieldsData();
        bool shieldsBroken = false; if(kState == kPoiseBroke) shieldsBroken = true;
        gUI.FUpdateShieldHealthPoiseBars(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max, cHpShlds.mShields.mStrength, cHpShlds.mShields._max, mPoise, _maxPoise, shieldsBroken);
        F_CharSpecUpdate();
    }

    // Simplifying to ignore damage type for now.
    // Making them not flinch when the shields take all the damage. 
    public void FTakeDamage(float amt, DAMAGE_TYPE type, GameObject damagingObject = null)
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
            mPoise -= healthDam;
            if(mPoise <= 0f){
                ENTER_PoiseBreak(type, damagingObject);
            }
        }

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            if(GetComponent<EN_BPBertha>() != null){
                GetComponent<EN_BPBertha>().F_Death();
            }if(GetComponent<BS_Soros>() != null){
                GetComponent<BS_Soros>().F_Death();   
            }else{
                rOverseer.FRegisterDeadEnemy(this);
            }
        }
    }
    public void ENTER_PoiseBreak(DAMAGE_TYPE type, GameObject damagingObject = null)
    {
        // Because hitting the wall while stunned does not reset the stun.
        if(type != DAMAGE_TYPE.WALL){
            mStunTmStmp = Time.time;
        }

        if(GetComponent<EN_Beamer>()){
            GetComponent<EN_Beamer>().cLineRender.enabled = false;
        }
        if(GetComponent<EN_BPBertha>()){
            if(kState == GetComponent<EN_BPBertha>().kPreExplosion){
                return;
            }
        }

        kState = kPoiseBroke;
        mPoise = 0f;

        Vector2 damObjectPosition = new Vector2();
        if(type == DAMAGE_TYPE.WALL){
            if(damagingObject != null) damObjectPosition = damagingObject.transform.position;
        }else{
            if(rOverseer.rPC != null){
                damObjectPosition = rOverseer.rPC.transform.position;
            }
        }

        if(damObjectPosition != null && rOverseer.rPC != null){
            mStunInitialVel = _stunInitialVelocity;
            if(type == DAMAGE_TYPE.SLASH) mStunInitialVel *= _stunMeleeAccMult;
            cRigid.velocity = (transform.position - rOverseer.rPC.transform.position).normalized * mStunInitialVel;
            // if(type == DAMAGE_TYPE.WALL) transform.up = cRigid.velocity.normalized * -1f;
        }else{
            cRigid.velocity = Vector2.zero;
        }

        // hack because I don't want to do vector math to calculate surface normals and all that.
        if(type == DAMAGE_TYPE.WALL || type == DAMAGE_TYPE.HOLYWATER){
            cRigid.velocity = Vector2.zero;
    }

        if(GetComponent<EN_Elite>()){
            GetComponent<EN_Elite>().rBatonHitbox.gameObject.SetActive(false);
        }
    }

    // Ideally, we'd change the colour of the poise break bar.
    public void F_RunStunRecovery()
    {
        mPoise = 0f;
        if(Time.time - mStunTmStmp > _poiseRecTime){
            cRigid.velocity = Vector2.zero;
            mPoise = _maxPoise;
            EXIT_PoiseBreak();
        }else{
            float percentDone = (Time.time - mStunTmStmp) / _poiseRecTime;
            percentDone = 1f - percentDone;         // take inverse percent.
            cRigid.velocity = cRigid.velocity.normalized * mStunInitialVel * percentDone;
        }
    }

    public virtual void EXIT_PoiseBreak(){}

    public bool F_CanSeePlayer()
    {
        if(rOverseer.rPC == null) return false;

        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC", "ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, Mathf.Infinity, mask);
        if(hit.collider == null){
            return false;
        }
        if(!hit.collider.GetComponent<PC_Cont>()){
            return false;
        }

        return true;
    }

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
        }else if(col.GetComponent<PJ_HolyWater>()){
            PJ_HolyWater h = col.GetComponent<PJ_HolyWater>();
            FTakeDamage(h._damage, DAMAGE_TYPE.PLASMA);
            h.Explode();
        }
        else if(col.GetComponent<PJ_Base>()){
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
        }else if(col.GetComponent<ENV_TileRock>()){
            if(kState == kPoiseBroke){
                float dam = _wallDamagePerUnitVelocity * cRigid.velocity.magnitude;
                FTakeDamage(dam, DAMAGE_TYPE.WALL, col.gameObject);
            }else if(GetComponent<EN_Hunter>()){
                EN_Hunter h = GetComponent<EN_Hunter>();
                if(h.kState == h.kLeaping){
                    float dam = _wallDamagePerUnitVelocity * cRigid.velocity.magnitude;
                    mStunTmStmp = Time.time;
                    h.gLeapHitbox.gameObject.SetActive(false);
                    FTakeDamage(dam, DAMAGE_TYPE.WALL, col.gameObject);
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.GetComponent<PC_SwordHitbox>()){
            FTakeDamage(80f, DAMAGE_TYPE.SLASH);
            col.gameObject.GetComponentInParent<PC_Cont>().FHeal(col.gameObject.GetComponentInParent<PC_Melee>()._healAmtFromSuccessfulHit);
        }
    }

    public bool F_CheckCanSeePlayer(Vector2 playerPos, Vector2 ourPos)
    {
        Vector2 vDir = playerPos - ourPos;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(ourPos, vDir.normalized, Mathf.Infinity, mask);
        // If we can see the player, immediately go to charging.
        if(hit.collider != null){
            Debug.DrawLine(ourPos, hit.collider.gameObject.transform.position, Color.green);
            if(hit.collider.GetComponent<PC_Cont>()){
                return true;
            }
        }
        return false;
    }

    // Problem is that we're hitting ourselves sometimes.
    public bool F_CanSeePlayerFromAllCornersOfBox(Vector2 playerPos, Vector2 castPos, float boxSize = 0.1f)
    {
        Vector2 workingPos = castPos;
        workingPos.x -= boxSize; workingPos.y -= boxSize;
        if(F_CheckCanSeePlayer(playerPos, workingPos)){
            workingPos.x = castPos.x + boxSize;
            if(F_CheckCanSeePlayer(playerPos, workingPos)){
                workingPos = castPos; workingPos.y += boxSize; workingPos.x -= boxSize;
                if(F_CheckCanSeePlayer(playerPos, workingPos)){
                    workingPos.x = castPos.x + boxSize;
                    if(F_CheckCanSeePlayer(playerPos, workingPos)){
                        return true;
                    }
                }
            }
        }
        return false;
    }

}
