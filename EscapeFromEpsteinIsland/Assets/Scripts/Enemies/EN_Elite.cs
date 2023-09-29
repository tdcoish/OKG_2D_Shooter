/*************************************************************************************
Has to manage his gun. Also move around. Also do cute behaviours.

Want them to be smart enough to run away when their shields are recharging. 

The Elite is the first enemy that we're making stunnable. Eventually they all will be.

It's going to be a lot of work, but the elites need logic that makes them want to move away
from each other, so as to maximize the stress put on the player character.
Also, need them running away from the holy water.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class EN_Elite : Actor
{
    public enum STATE{LONG_RANGE_FIRING_SPOT, CLOSING_TO_LONG_RANGE_FIRING_SPOT, LOOKING_FOR_FIRING_SPOT, CLOSING, PREP_MELEE, MELEEING, RECOVER_FROM_MELEE, STUN}
    public STATE                        mState;

    public PC_Cont                      rPC;
    public EN_PRifle                    cRifle;
    public Rigidbody2D                  cRigid;
    public A_HealthShields              cHpShlds;
    public EN_EliteAnimator             cAnim;

    public GameObject                   gShotPoint;
    public GameObject                   PF_Particles;

    // Needs a max fire distance. Really, everything does.
    public float                        _grenadePrepTime = 2f;
    public float                        mGrenPrepTmStmp;
    public float                        _grenadeThrowRecoverTime = 1f;
    public float                        mGrenThrowTmStmp;
    public float                        _grenadeThrowSpd = 8f;
    public float                        _grenadePreDetonationTime = 1f;
    public float                        _startMeleeDistance = 2f;
    public float                        _meleePrepTime = 0.5f;
    public float                        mMeleePrepTmStmp;
    public float                        _meleeTime = 0.5f;
    public float                        mMeleeTmStmp;
    public float                        _meleeRecoverTime = 1f;
    public float                        mMeleeRecTmStmp;
    public float                        _meleeSpd = 5f;
    Vector2                             mSlashTargetSpot;
    public float                        _switchToMeleeChasingDistance = 10f;
    [HideInInspector]
    public float                        _switchToLongRangeDistance;
    public float                        _maxFireDistance;
    public float                        _stunRecTime = 1f;
    public float                        mStunTmStmp;
    public float                        _runSpd = 3f;

    public EL_BatonHitbox               rBatonHitbox;

    // Copy from the EN_Knight.    
    // Currently only used for LOOKING_FOR_VANTAGE_POINT. Subject to change.
    public Vector2Int                   mGoalTilePathing;

    public UI_EN                        gUI;

    public DIRECTION                    mHeading;

    public override void RUN_Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRifle = GetComponent<EN_PRifle>();
        cRigid = GetComponent<Rigidbody2D>();
        cAnim = GetComponent<EN_EliteAnimator>();
        cHpShlds = GetComponent<A_HealthShields>();
        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = cHpShlds.mShields._max;
        cHpShlds.mShields.mState = Shields.STATE.FULL;
        rBatonHitbox = GetComponentInChildren<EL_BatonHitbox>();
        rBatonHitbox.gameObject.SetActive(false);

        mState = STATE.LONG_RANGE_FIRING_SPOT;
        _switchToLongRangeDistance = _switchToMeleeChasingDistance * 0.8f;
    }

    // For now he just fires his rifle.
    public override void RUN_Update()
    {
        switch(mState){
            case STATE.LONG_RANGE_FIRING_SPOT: FRUN_LongRangeFiringSpot(); break;
            case STATE.CLOSING_TO_LONG_RANGE_FIRING_SPOT: FRUN_CloseToLongRangeFiringSpot(); break;
            case STATE.LOOKING_FOR_FIRING_SPOT: RUN_MoveToVantagePoint(); break;
            case STATE.CLOSING: FRUN_Closing(); break;
            case STATE.PREP_MELEE: FRUN_PrepMelee(); break;
            case STATE.MELEEING: FRUN_Meleeing(); break;
            case STATE.RECOVER_FROM_MELEE: FRUN_MeleeRecover(); break;
            case STATE.STUN: FRUN_StunRecover(); break;
        }
        
        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);

        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);
        gUI.FUpdateShieldHealthBars(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max, cHpShlds.mShields.mStrength, cHpShlds.mShields._max, true);

        cAnim.FAnimate();
    }

    // Have to find the right tile here.
    void ENTER_MoveToVantagePoint()
    {   
        MAN_Pathing pather = rOverseer.GetComponent<MAN_Pathing>();
        MAN_Helper h = rOverseer.GetComponent<MAN_Helper>();
        mState = STATE.LOOKING_FOR_FIRING_SPOT;
        Debug.Log("Lost sight of player, moving to vantage point now");

        // sort all the tiles from closest to the hunter to furthest away.
        List<Vector2Int> tilesSortedClosestToFurthest = new List<Vector2Int>();
        int countValidTiles = 0;
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // have to be a valid tile, and have to be able to see the player.
                if(!pather.mPathingTiles[x,y].mCanPath) continue;

                Vector2 tilePos = h.FGetWorldPosOfTile(new Vector2Int(x,y));
                float paddingFromEdge = 1;
                LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
                if(!h.FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, tilePos, paddingFromEdge, mask)){
                    continue;
                }
                tilesSortedClosestToFurthest.Add(new Vector2Int(x,y));
                countValidTiles++;
            }
        }
        Debug.Log("Valid tiles: " + countValidTiles);

        if(tilesSortedClosestToFurthest.Count == 0){
            Debug.Log("Weird, no tiles can see player.");
            return;
        }

        // Now we just find the one that is closest to the hunter.
        float shortestDis = 10000000f;
        int indClosest = -1;
        for(int i=0; i<tilesSortedClosestToFurthest.Count; i++){
            Vector2 tilePos = pather.GetComponent<MAN_Helper>().FGetWorldPosOfTile(tilesSortedClosestToFurthest[i]);
            float dis = Vector2.Distance(transform.position, tilePos);
            if(dis < shortestDis){
                shortestDis = dis;
                indClosest = i;
            }
        }
        
        mGoalTilePathing = tilesSortedClosestToFurthest[indClosest];
        Debug.Log("SHould have found valid goal " + mGoalTilePathing);
        Debug.Log("Current pos: " + h.FGetTileClosestToSpot(transform.position));

        // Visual Debugging.
        MAN_Helper help = pather.GetComponent<MAN_Helper>();
        Instantiate(pather.GetComponent<MAN_Helper>().PF_Blue3, help.FGetWorldPosOfTile(mGoalTilePathing), transform.rotation);
        return;
    }

    void RUN_MoveToVantagePoint()
    {
        Vector2 dest = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mGoalTilePathing);
        Vector2 dif = dest - (Vector2)transform.position; 
        cRigid.velocity = _runSpd * dif.normalized;

        if(Vector2.Distance(transform.position, dest) < 0.1f){
            // Debug.Log("Hit vantage point spot");
            FENTER_LongRangeFiringState();
        }

        // We also want to check if we can see the player.
        // We might also want to check if we have a different vantage point, although maybe not.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(rOverseer.GetComponent<MAN_Helper>().FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position, 1f, mask)){
            FENTER_LongRangeFiringState(); 
        }
    }

    void FENTER_LongRangeFiringState()
    {
        mState = STATE.LONG_RANGE_FIRING_SPOT;
    }
    // Here he has to move to a decent spot to fire. 
    void FRUN_LongRangeFiringSpot()
    {
        if(rOverseer.rPC == null){
            return;
        }
        // Make them maneuver to a spot just like the hunters, where they can see the player better.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(!rOverseer.GetComponent<MAN_Helper>().FCanSeePlayerFromAllCornersOfBox(rPC.transform.position, transform.position, 1f, mask))
        {
            ENTER_MoveToVantagePoint();
        }

        // Here we attempt to move straight to the player if he's too far away. 
        if(Vector3.Distance(transform.position, rPC.transform.position) > _maxFireDistance){
            FENTER_ClosingToLongRangeFiringSpot();
        }else{
            cRigid.velocity = Vector2.zero;
            cRifle.FAttemptFire(rPC, gShotPoint.transform.position);
            Vector2 vDirToPlayer = (rPC.transform.position - transform.position).normalized;
            mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(vDirToPlayer);
            
            if(Vector2.Distance(transform.position, rPC.transform.position) < _switchToMeleeChasingDistance){
                mState = STATE.CLOSING;
            }
        }

    }
    void FENTER_ClosingToLongRangeFiringSpot()
    {
        mState = STATE.CLOSING_TO_LONG_RANGE_FIRING_SPOT;
    }

    void FRUN_CloseToLongRangeFiringSpot()
    {
        if(rPC == null) return;
        if(Vector3.Distance(transform.position, rPC.transform.position) < _maxFireDistance*0.8f){
            FENTER_LongRangeFiringState();
        }else{
            Vector2 vDir = rPC.transform.position - transform.position;
            cRigid.velocity = vDir.normalized * _runSpd;
        }
    }

    // Ultimately this needs more logic. Sometimes we want them to close from across the map. Other times we want them to back off. But when?
    void FRUN_Closing()
    {
        cRifle.FAttemptFire(rPC, gShotPoint.transform.position);
        Vector2 vDirToPlayer = (rPC.transform.position - transform.position).normalized;
        cRigid.velocity = vDirToPlayer * _runSpd;
        mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(cRigid.velocity.normalized);

        if(Vector2.Distance(transform.position, rPC.transform.position) < _startMeleeDistance){
            mState = STATE.PREP_MELEE;
            mMeleePrepTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            mSlashTargetSpot = rPC.transform.position;
        }
    }
    void FRUN_PrepMelee()
    {
        if(Time.time - mMeleePrepTmStmp > _meleePrepTime){
            mState = STATE.MELEEING;
            mMeleeTmStmp = Time.time;
            cRigid.velocity = (mSlashTargetSpot - (Vector2)transform.position).normalized * _meleeSpd;
            rBatonHitbox.gameObject.SetActive(true);
        }
    }
    // Have to actually give them a melee hitbox.
    void FRUN_Meleeing()
    {
        if(Time.time - mMeleeTmStmp > _meleeTime){
            mState = STATE.RECOVER_FROM_MELEE;
            cRigid.velocity = Vector2.zero;
            mMeleeRecTmStmp = Time.time;
            rBatonHitbox.gameObject.SetActive(false);
        }
    }
    void FRUN_MeleeRecover()
    {
        if(Time.time - mMeleeRecTmStmp > _meleeRecoverTime){
            if(Vector3.Distance(rPC.transform.position, transform.position) < _switchToMeleeChasingDistance){
                mState = STATE.CLOSING;
            }else{
                mState = STATE.LONG_RANGE_FIRING_SPOT;
            }
        }
    }

    void FRUN_StunRecover()
    {
        if(Time.time - mStunTmStmp > _stunRecTime){
            // Figure out the correct state.
            mState = STATE.LONG_RANGE_FIRING_SPOT;
        }
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
        // Debug.Log("Health Dam: " + healthDam);

        // for now we make them stunned each time.
        ENTER_Stun();

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void ENTER_Stun()
    {
        mState = STATE.STUN;
        mStunTmStmp = Time.time;
        cRigid.velocity = Vector2.zero;
        rBatonHitbox.gameObject.SetActive(false);
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
        if(col.GetComponent<PJ_PC_Bullet>()){
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            FTakeDamage(10f, DAMAGE_TYPE.PLASMA);
            Destroy(col.gameObject);
        }else if(col.GetComponent<PC_SwordHitbox>()){
            FTakeDamage(80f, DAMAGE_TYPE.SLASH);
            col.GetComponentInParent<PC_Cont>().FHeal(col.GetComponentInParent<PC_Melee>()._healAmtFromSuccessfulHit);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            FTakeDamage(40f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PJ_Base>()){
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

    public override void FAcceptHolyWaterDamage(float amt)
    {
        FTakeDamage(amt, DAMAGE_TYPE.HOLYWATER);
    }

}
