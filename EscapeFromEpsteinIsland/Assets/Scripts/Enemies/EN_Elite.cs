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

public class EN_Elite : EN_Base
{
    public uint                         kLongRangeFiring = 1<<2; 
    public uint                         kClosingToLongRangeFiringSPot = 1<<3; 
    public uint                         kSeekingLongRangeFiringSpot = 1<<4; 
    public uint                         kClosing = 1<<5; 
    public uint                         kPrepMelee = 1<<6; 
    public uint                         kMelee = 1<<7; 
    public uint                         kRecMelee = 1<<8; 

    public EN_PRifle                    cRifle;
    public EN_EliteAnimator             cAnim;

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
    public Vector2                      mSlashTargetSpot;
    public float                        _switchToMeleeChasingDistance = 10f;
    [HideInInspector]
    public float                        _giveUpMeleeChaseDistance;
    public float                        _maxFireDistance;

    public EL_BatonHitbox               rBatonHitbox;

    // Copy from the EN_Knight.    
    // Currently only used for LOOKING_FOR_VANTAGE_POINT. Subject to change.
    public Vector2Int                   mGoalTilePathing;

    public override void F_CharSpecStart()
    {
        cRifle = GetComponent<EN_PRifle>();
        cAnim = GetComponent<EN_EliteAnimator>();
        rBatonHitbox = GetComponentInChildren<EL_BatonHitbox>();
        rBatonHitbox.gameObject.SetActive(false);

        kState = kLongRangeFiring; 
        _giveUpMeleeChaseDistance = _switchToMeleeChasingDistance * 2f;
    }

    public override void F_CharSpecUpdate()
    {
        if(kState == kLongRangeFiring){
            FRUN_LongRangeFiringSpot();
        }else if(kState == kClosingToLongRangeFiringSPot){
            FRUN_CloseToLongRangeFiringSpot();
        }else if(kState == kSeekingLongRangeFiringSpot){
            RUN_MoveToVantagePoint();
        }else if(kState == kClosing){
            FRUN_Closing();
        }else if(kState == kPrepMelee){
            FRUN_PrepMelee();
        }else if(kState == kMelee){
            FRUN_Meleeing();
        }else if(kState == kRecMelee){
            FRUN_MeleeRecover();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }

        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);
        cAnim.FAnimate();
    }

    // Have to find the right tile here.
    void ENTER_MoveToVantagePoint()
    {   
        MAN_Pathing pather = rOverseer.GetComponent<MAN_Pathing>();
        MAN_Helper h = rOverseer.GetComponent<MAN_Helper>();
        kState = kSeekingLongRangeFiringSpot;
        // Debug.Log("Lost sight of player, moving to vantage point now");

        // sort all the tiles from closest to the hunter to furthest away.
        List<Vector2Int> tilesSortedClosestToFurthest = new List<Vector2Int>();
        int countValidTiles = 0;
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // have to be a valid tile, and have to be able to see the player.
                if(!pather.mAllTiles[x,y].mTraversable) continue;

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
        return;
    }

    void RUN_MoveToVantagePoint()
    {
        Vector2 dest = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mGoalTilePathing);
        Vector2 dif = dest - (Vector2)transform.position; 
        cRigid.velocity = _spd * dif.normalized;

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

        transform.up = cRigid.velocity.normalized;
    }

    void FENTER_LongRangeFiringState()
    {
        kState = kLongRangeFiring;
    }
    // Here he has to move to a decent spot to fire. 
    void FRUN_LongRangeFiringSpot()
    {
        if(rOverseer.rPC == null){
            return;
        }
        // Make them maneuver to a spot just like the hunters, where they can see the player better.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(!rOverseer.GetComponent<MAN_Helper>().FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position, 1f, mask))
        {
            ENTER_MoveToVantagePoint();
        }

        // Here we attempt to move straight to the player if he's too far away. 
        if(Vector3.Distance(transform.position, rOverseer.rPC.transform.position) > _maxFireDistance){
            FENTER_ClosingToLongRangeFiringSpot();
        }else{
            cRigid.velocity = Vector2.zero;
            cRifle.FAttemptFire(rOverseer.rPC, gShotPoint.transform.position);
            Vector2 vDirToPlayer = (rOverseer.rPC.transform.position - transform.position).normalized;
            transform.up = vDirToPlayer;
            
            if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) < _switchToMeleeChasingDistance){
                kState = kClosing;
            }
        }

    }
    void FENTER_ClosingToLongRangeFiringSpot()
    {
        kState = kClosingToLongRangeFiringSPot;
    }

    void FRUN_CloseToLongRangeFiringSpot()
    {
        if(rOverseer.rPC == null) return;
        if(Vector3.Distance(transform.position, rOverseer.rPC.transform.position) < _maxFireDistance*0.8f){
            FENTER_LongRangeFiringState();
        }else{
            Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
            cRigid.velocity = vDir.normalized * _spd;
        }
        transform.up = cRigid.velocity.normalized;
    }

    // Ultimately this needs more logic. Sometimes we want them to close from across the map. Other times we want them to back off. But when?
    void FRUN_Closing()
    {
        // For now they can't fire while closing. Subject to change.
        // cRifle.FAttemptFire(rOverseer.rPC, gShotPoint.transform.position);
        Vector2 vDirToPlayer = (rOverseer.rPC.transform.position - transform.position).normalized;
        cRigid.velocity = vDirToPlayer * _spd;
        transform.up = cRigid.velocity.normalized;

        if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) < _startMeleeDistance){
            kState = kPrepMelee;
            mMeleePrepTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            mSlashTargetSpot = rOverseer.rPC.transform.position;
        }

        if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) > _giveUpMeleeChaseDistance){
            kState = kLongRangeFiring;
        }
    }
    void FRUN_PrepMelee()
    {
        if(Time.time - mMeleePrepTmStmp > _meleePrepTime){
            kState = kMelee;
            mMeleeTmStmp = Time.time;
            cRigid.velocity = (mSlashTargetSpot - (Vector2)transform.position).normalized * _meleeSpd;
            rBatonHitbox.gameObject.SetActive(true);
        }
    }
    // Have to actually give them a melee hitbox.
    void FRUN_Meleeing()
    {
        if(Time.time - mMeleeTmStmp > _meleeTime){
            kState = kRecMelee;
            cRigid.velocity = Vector2.zero;
            mMeleeRecTmStmp = Time.time;
            rBatonHitbox.gameObject.SetActive(false);
        }
    }
    void FRUN_MeleeRecover()
    {
        if(Time.time - mMeleeRecTmStmp > _meleeRecoverTime){
            if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _switchToMeleeChasingDistance){
                kState = kClosing;
            }else{
                kState = kLongRangeFiring;
            }
        }
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kLongRangeFiring;
    }

    void RUN_TryToFire()
    {

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

    // public override void FAcceptHolyWaterDamage(float amt)
    // {
    //     FTakeDamage(amt, DAMAGE_TYPE.HOLYWATER);
    // }

}
