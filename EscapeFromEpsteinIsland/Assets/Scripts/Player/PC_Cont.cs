/*************************************************************************************
Basic Design. Stamina regens automatically when you're not sprinting or attacking. However,
movement, and especially melee, builds up your power/mana bar again. 

Unfortunately, I think we need to bite the bullet and create the UI at this point.

Need to separate the mouse position with the actual targeting position.

Need to make the melee stuff work on Q/E, so the grenades/mines work on RMB. You can do
the powerslide thing by sprinting and holding the Q/E buttons, or maybe space. Figure that out 
later. Ugh. Do need another state where they can't fire if in BATTACK_RECOVERY
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class PC_Cont : Actor
{
    public bool                             _debugInvinsible = false;
    // Currently used mostly for animation. Subject to change.
    // Some of these states should be substates. For example, idle and running.
    public enum STATE {IDLE, RUNNING, WINDUP, SLASHING, BATTACK_RECOVERY, SHOT_RECOVERY, THROW_RECOVERY, SLIDE_CHARGE, SLIDING, SLIDE_FINISH}
    public STATE                            mState;

    public Rigidbody2D                      cRigid;
    public A_HealthShields                  cHpShlds;
    public PC_Melee                         cMelee;
    public PC_Guns                          cGuns;
    public PC_AnimDebug                     cAnim;
    public PC_GrenThrower                   cGrenader;
    public PC_Heading                       cHeadSpot;

    public GameObject                       gShotPoint;
    public GameObject                       PF_DeathParticles;
    public GameObject                       PF_BloodParticles;

    public float                            _spd;
    public float                            _spdFwdMult = 1f;
    public float                            _spdBckMult;
    public float                            _spdSideMult;
    public float                            _spdShotRecentMult;

    public bool                             _debugGunsNoCooldowns = false;
    public bool                             _debugInfiniteStamina = false;
    public bool                             _useTempInvinsible = false;
    public float                            _tempInvinsibleTime = 0.1f;
    public bool                             mTempInvinsible = false;
    public float                            mTempInvinsibleTmStmp;
    public float                            _grenadeThrowRecoveryTime = 0.25f;

    public bool                             mFlyingAfterDamage = false;
    public float                            _flyingTime = 0.5f; // should change depending on what hit us.
    public float                            mFlyingTimeStmp;

    // Going back to TAB between melee mode and casting mode. Crysis style weapon switch being deleted.
    // public bool                             mMeleeMode = true;

    // There is no longer shared weapon energy, only cooldowns.
    public float                            _sprintSpdBoost = 1.5f;
    public bool                             mIsSprinting = false;
    public bool                             mMoving = false;
    public float                            _autoAimMaxDis = 0.25f;
    public bool                             mHasActiveTarget = false;
    public Actor                            rCurTarget;         // Wish I could use hash.

    public MAN_Helper                       rHelper;

    public Collider2D                       rSwordHurtBox;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        cHpShlds = GetComponent<A_HealthShields>();
        cMelee = GetComponent<PC_Melee>();
        cAnim = GetComponent<PC_AnimDebug>();
        cGuns = GetComponent<PC_Guns>();
        cGuns.F_Start();
        cGrenader = GetComponent<PC_GrenThrower>();
        cGrenader.F_Start();
        cHeadSpot = GetComponent<PC_Heading>();
        cHeadSpot.F_Start();

        rHelper = FindObjectOfType<MAN_Helper>();
        if(rHelper == null){
            Debug.Log("No helper in scene");
        }

        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = cHpShlds.mShields._max;
        cHpShlds.mShields.mState = Shields.STATE.FULL;
        mState = STATE.IDLE;
    }

    public override void RUN_Update()
    {
        switch(mState)
        {
            case STATE.IDLE: RUN_IdleAndMoving(); break;
            case STATE.RUNNING: RUN_IdleAndMoving(); break;
            case STATE.THROW_RECOVERY: RUN_GrenadeThrowRecovery(); break;
            case STATE.SHOT_RECOVERY: RUN_ShotRecovery(); break;
            case STATE.WINDUP: RUN_WindupAndSlashingAndBAtkRec(); break;
            case STATE.SLASHING: RUN_WindupAndSlashingAndBAtkRec(); break;
            case STATE.BATTACK_RECOVERY: RUN_WindupAndSlashingAndBAtkRec(); break;
        }

        F_FigureOutActiveTarget();

        if(mTempInvinsible){
            if(Time.time - mTempInvinsibleTmStmp > _tempInvinsibleTime){
                mTempInvinsible = false;
            }
        }
        if(mFlyingAfterDamage){
            if(Time.time - mFlyingTimeStmp > _flyingTime){
                mFlyingAfterDamage = false;
            }
        }

        // Now do stamina and mana as well.
        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);
        cHeadSpot.FUpdateHeadingSpot();

        cAnim.FRUN_Animation();
        cAnim.FRUN_Shields(cHpShlds.mShields.mStrength / cHpShlds.mShields._max);
        cAnim.FFlashDependingOnLastHitTime(cHpShlds.mLastHitTmStmp, cHpShlds.mShields.mStrength);

        // They should switch modes immediately, but not necessarily switch states, such as if they are hitstunned.
        // if(Input.GetKeyDown(KeyCode.Tab)){
        //     mMeleeMode = !mMeleeMode;
        // }


        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_DeathParticles, transform.position, transform.rotation);
            Destroy(gameObject);
            rOverseer.FHandlePlayerDied();
        }

        if(_debugInvinsible){
            cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
            cHpShlds.mShields.mStrength = cHpShlds.mShields._max;
        }
    }

    public void RUN_GrenadeThrowRecovery()
    {
        cRigid.velocity = Vector2.zero;
        if(Time.time - cGrenader.mLastThrowTmStmp > _grenadeThrowRecoveryTime){
            mState = STATE.RUNNING;
        }
    }
    
    // Might have more fine tuned shot recovery time.
    public void RUN_ShotRecovery()
    {
        if(Input.GetMouseButton(1)){
            cGuns.F_SetGunsToRecover();
            ENTER_WindupForSlash();
            return;
        }
        // Problem is we need to update gun even when we aren't holding down fire.
        cGuns.F_CheckInputHandleFiring(cHeadSpot.mCurHeadingSpot, gShotPoint.transform.position, Input.GetMouseButton(0));   
        cRigid.velocity = Vector2.zero;
        if(Time.time - cGuns.mFireTmStmp > cGuns._salvoRecTime*1.1f){
            cGuns.mCurFireInterval = cGuns._fireInterval*cGuns._shotSpeedIncRate;
            mState = STATE.RUNNING;
        }
        // Makes it so you can in fact move while shooting.
        cRigid.velocity = HandleInputForVel();

        cGrenader.FRunGrenadeLogic();
        if(Time.time - cGrenader.mLastThrowTmStmp < _grenadeThrowRecoveryTime){
            mState = STATE.THROW_RECOVERY;
            cGuns.F_SetGunsToRecover();
        }
    }

    public void RUN_IdleAndMoving()
    {
        //RotateToMouse();
        cRigid.velocity = HandleInputForVel();

        if(cRigid.velocity != Vector2.zero){
            mState = STATE.RUNNING;
        }else{
            mState = STATE.IDLE;
        }
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vDir = msPos - (Vector2)transform.position;

        cGuns.F_CheckInputHandleFiring(cHeadSpot.mCurHeadingSpot, gShotPoint.transform.position, Input.GetMouseButton(0));   
        // For now, testing melee on RMB.
        if(Input.GetMouseButton(1)){
            ENTER_WindupForSlash();
        }

        cGrenader.FRunGrenadeLogic();
        if(Time.time - cGrenader.mLastThrowTmStmp < _grenadeThrowRecoveryTime){
            mState = STATE.THROW_RECOVERY;
        }
    }

    // Basically figure out which enemies are close enough to be the active target.
    public void F_FigureOutActiveTarget()
    {
        List<Actor> closeEnoughActors = new List<Actor>();
        for(int i=0; i<rOverseer.rActors.Count; i++){
            if(Vector2.Distance(rOverseer.rActors[i].transform.position, cHeadSpot.mCurHeadingSpot) < _autoAimMaxDis){
                if(!rOverseer.rActors[i].GetComponent<PC_Cont>()){
                    closeEnoughActors.Add(rOverseer.rActors[i]);
                }
            }
        }
        if(closeEnoughActors.Count == 0){
            mHasActiveTarget = false;
            return;
        }
        int indClosest = 0; float shortestDis = Vector2.Distance(closeEnoughActors[0].transform.position, cHeadSpot.mCurHeadingSpot);
        for(int i=1; i<closeEnoughActors.Count; i++){
            float dis = Vector2.Distance(closeEnoughActors[i].transform.position, cHeadSpot.mCurHeadingSpot);
            if(dis < shortestDis){
                shortestDis = dis; indClosest = i;
            }
        }
        mHasActiveTarget = true;
        rCurTarget = closeEnoughActors[indClosest];
    }

    public void ENTER_WindupForSlash()
    {
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cMelee.FStartMelee();
        mState = STATE.WINDUP;
        // Hack, but it works.
        cGuns.mSalvoInd = 0; cGuns.mState = PC_Guns.STATE.REC_BURST;
    }
    public void RUN_WindupAndSlashingAndBAtkRec()
    {
        cMelee.FRUN_Melee();
        if(cMelee.mState == PC_Melee.STATE.S_Windup) mState = STATE.WINDUP;
        if(cMelee.mState == PC_Melee.STATE.S_Slashing) mState = STATE.SLASHING;
        if(cMelee.mState == PC_Melee.STATE.S_Recover) mState = STATE.BATTACK_RECOVERY;

        // Figure that shit out.
        if(cMelee.mState == PC_Melee.STATE.S_NOT_Meleeing){
            mState = STATE.IDLE;
        }
    }

    // Movement is now different depending on where the player is looking.
    public Vector3 HandleInputForVel()
    {
        // a dot of 0.7 corresponds with 45* in that direction.
        float GetDotMult(Vector2 vDirToMs, Vector2 vInputDir)
        {
            float dot = Vector2.Dot(vDirToMs.normalized, vInputDir);
            if(dot >= 0.65f){
                return 1f;
            }else if(dot <= -0.65f){
                return _spdBckMult;
            }else{
                return _spdSideMult;
            }
        }

		Vector2 vDir = cHeadSpot.mCurHeadingSpot - (Vector2)transform.position;
		float angle = Mathf.Atan2(vDir.y, vDir.x) * Mathf.Rad2Deg;
		angle -= 90;

        // want the normalized speed halfway in between the x and y max speeds.
        Vector2 vVel = new Vector2();
        float mult = 1f;
        if(mState == STATE.SLASHING || mState == STATE.BATTACK_RECOVERY){
            mult *= cMelee._slashMoveSpdMult;
        }
        if(mState == STATE.SHOT_RECOVERY){
            mult *= cGuns._shotRecoverySpeedMult;
        }

        if(Input.GetKey(KeyCode.A)){
            mult *= GetDotMult(vDir, -Vector2.right);
            vVel.x -= _spd * mult;
        }
        if(Input.GetKey(KeyCode.D)){
            mult *= GetDotMult(vDir, Vector2.right);
            vVel.x += _spd * mult;
        }
        if(Input.GetKey(KeyCode.W)){
            mult *= GetDotMult(vDir, Vector2.up);
            vVel.y += _spd * mult;
        }
        if(Input.GetKey(KeyCode.S)){
            mult *= GetDotMult(vDir, -Vector2.up);
            vVel.y -= _spd * mult;
        }

        float totalMult = Mathf.Abs(vVel.x/_spd) + Mathf.Abs(vVel.y/_spd);
        if(vVel.x != 0f && vVel.y != 0f){
            totalMult /= 2f;
        }
        vVel = Vector3.Normalize(vVel) * _spd * totalMult;
        if(vVel.magnitude != 0){
            mMoving = true;
        }else{
            mMoving = false;
        }
        return vVel;
    }

    // 
    void SetTargetingReticule()
    {

    }

    private void RotateToMouse(){
		Camera c = Camera.main;
		Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);

		Vector2 distance = msPos - (Vector2)transform.position;
        // We don't actually rotate the player anymore.
		// float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
		// angle -= 90;
		// transform.eulerAngles = new Vector3(0, 0, angle);

	}

    void CheckInvinsibilitiesMaybeTakeDamage(float amt, DAMAGE_TYPE type)
    {
        if(_debugInvinsible) return;

        if(_useTempInvinsible){
            if(mTempInvinsible) return;

            mTempInvinsible = true;
            mTempInvinsibleTmStmp = Time.time;
        }
        cHpShlds.FTakeDamage(amt, type);
        Instantiate(PF_BloodParticles, transform.position, transform.rotation);
    }
    void OnTriggerEnter2D(Collider2D col)
    {

        if(col.GetComponent<PJ_Base>()){
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    return;
                }
            }

            // If an enemy grenade hit us, just make its velocity stop, and it explodes.
            if(col.GetComponent<PJ_EN_PGrenade>()){
                col.GetComponent<PJ_EN_PGrenade>().FEnter_Landed();
                return;
            }
            // Note, will have to change a bit for the needler.
            if(p.mProjD._DAM_TYPE != DAMAGE_TYPE.NO_DAMAGE){
                CheckInvinsibilitiesMaybeTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);
            }

            p.FDeath();
        }

        if(col.GetComponent<EN_KnightHitbox>()){
            CheckInvinsibilitiesMaybeTakeDamage(50f, DAMAGE_TYPE.SLASH);
        }
        if(col.GetComponent<EN_HunterLeapBox>()){
            CheckInvinsibilitiesMaybeTakeDamage(70f, DAMAGE_TYPE.HUNTER_LEAP);
        }
        if(col.GetComponent<PJ_Boomerang>()){
            CheckInvinsibilitiesMaybeTakeDamage(50f, DAMAGE_TYPE.BOOMERANG);
            Destroy(col.gameObject);
        }

        // For explosions, we might also want to be pushed away from the center.
        if(col.GetComponent<EX_Gren>()){
            EX_Gren p = col.GetComponent<EX_Gren>();
            CheckInvinsibilitiesMaybeTakeDamage(p._dam, p._DAM_TYPE);
        }
        if(col.GetComponent<EX_HBlast>()){
            EX_HBlast b = col.GetComponent<EX_HBlast>();
            CheckInvinsibilitiesMaybeTakeDamage(b._damage, DAMAGE_TYPE.EXPLOSION);
        }
        if(col.GetComponent<EX_PlayerMine>()){
            CheckInvinsibilitiesMaybeTakeDamage(80f, DAMAGE_TYPE.EXPLOSION);
        }

        // If the hunter collided with us.
        // if(col.GetComponent<EN_Hunter>()){
        //     EN_Hunter h = col.GetComponent<EN_Hunter>();
        //     if(h.mState == EN_Hunter.STATE.LEAPING){
        //         CheckInvinsibilitiesMaybeTakeDamage(h._leapDmg, DAMAGE_TYPE.MELEE);
        //         mFlyingAfterDamage = true;
        //         mFlyingTimeStmp = Time.time;
        //         Debug.Log("sent flying");
        //         cRigid.velocity = h.GetComponent<Rigidbody2D>().velocity * 2f;
        //     }
        // }

        // This needs to be fixed, where we don't take damage if they hit our melee box.
        // Apparently, Unity makes this incredibly painful with triggers as opposed to colliders.
        // It's not worth it to me to fix this bullshit now, but I do worry that we will also take 
        // damage from projectiles when we swing our sword. 
        if(col.GetComponent<EN_FloodInfectionForm>()){
            EN_FloodInfectionForm f = col.GetComponent<EN_FloodInfectionForm>();
            CheckInvinsibilitiesMaybeTakeDamage(f._damage, DAMAGE_TYPE.EXPLOSION);
            //rCombat.FregisterDeadEnemy(f);
            Destroy(f.gameObject);
        }

        if(col.GetComponent<PJ_MineShot>()){
            PJ_MineShot m = col.GetComponent<PJ_MineShot>();
            CheckInvinsibilitiesMaybeTakeDamage(m.mCurDam, DAMAGE_TYPE.PLASMA);
            Destroy(col.gameObject);
        }

        if(col.GetComponent<EL_BatonHitbox>()){
            CheckInvinsibilitiesMaybeTakeDamage(40f, DAMAGE_TYPE.PLASMA);
        }

        if(col.GetComponent<EN_TwerkHitbox>()){
            CheckInvinsibilitiesMaybeTakeDamage(50f, DAMAGE_TYPE.BASIC);
        }

        if(col.GetComponent<EN_JeqKnifeHitbox>()){
            CheckInvinsibilitiesMaybeTakeDamage(40f, DAMAGE_TYPE.BASIC);
        }
        
        if(col.GetComponent<EN_FTM_FistHitbox>()){
            CheckInvinsibilitiesMaybeTakeDamage(40f, DAMAGE_TYPE.BASIC);
        }

        if(col.GetComponent<Pk_Powerup>()){
            Debug.Log("Picked up powerup");
            Pk_Powerup p = col.GetComponent<Pk_Powerup>();
            cHpShlds.mHealth.mAmt += p._healthRestore;
            if(cHpShlds.mHealth.mAmt > cHpShlds.mHealth._max){
                cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
            }
            p.F_Death();
        }
    }

    // void OnCollisionEnter2D(Collision2D col)
    // {
    //     if(col.gameObject.GetComponent<EN_FloodInfectionForm>()){
    //         EN_FloodInfectionForm f = col.gameObject.GetComponent<EN_FloodInfectionForm>();

    //         if(col.collider.IsTouching(cMelee.rCollider)){
    //             Debug.Log("Hit sword");
    //         }else{
    //             CheckInvinsibilitiesMaybeTakeDamage(f._damage, DAMAGE_TYPE.EXPLOSION);
    //             Destroy(f.gameObject);
    //         }
    //     }
    // }

    public void F_GetZappedByNPC(float dps)
    {
        // Debug.Log("Zapped by NPC");
        cHpShlds.FTakeDamage(dps * Time.deltaTime, DAMAGE_TYPE.ENEMYTOUCH);
    }
    public void F_ReceiveTroonBloodSpotDamage(float damage)
    {
        cHpShlds.FTakeDamage(damage, DAMAGE_TYPE.PLASMA);
    }
    public void F_ReceiveBerthaExplodeDamage(float damage)
    {
        cHpShlds.FTakeDamage(damage, DAMAGE_TYPE.PLASMA);
    }

    public void OnTriggerStay2D(Collider2D col)
    {
        // Debug.Log("Inside: " + col.gameObject);
        // if(col.GetComponent<EN_NPC_AttackVolume>()){
        //     Debug.Log("Inside NPC attack volume");
        //     float dam = 100f * Time.deltaTime;
        //     cHpShlds.FTakeDamage(dam, DAMAGE_TYPE.ENEMYTOUCH);
        // }
        // I just installed VIM extension. Not entirely sure about this.     
    }

    public void FHandleDamExternal(float amt, DAMAGE_TYPE _TYPE)
    {
        if(_debugInvinsible)
        {
            return;
        }
        cHpShlds.FTakeDamage(amt, _TYPE);
    }

    public void FHeal(float amt)
    {
        cHpShlds.mHealth.mAmt += amt;
        if(cHpShlds.mHealth.mAmt > cHpShlds.mHealth._max){
            cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        }
    }

}
