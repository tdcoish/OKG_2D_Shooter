﻿/*************************************************************************************
Wew. Gonna need a whole lot of stats here.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class EN_Hunter : Actor
{
    public enum STATE{LOOKING_FOR_VANTAGE_POINT, LONG_RANGE, CLOSE_RANGE, LEAPING, FLYING_AFTER_DAMAGED, RECOVER_FROM_LEAP}
    public STATE                    mState;
    
    // for now sort of shuffle around at long range.
    public float                    _shuffleDirectionTime = 1.5f;
    public float                    mShuffleTmStmp;
    public float                    _shuffleSpd = 0.5f;
    public Vector2                  mShuffleDir;

    // Adding their firing as part of their character.
    public float                    _shotChargeTime = 3f;
    public float                    mChargeTmStmp;
    public PJ_EN_HunterBlast        PF_HunterBlast;

    public EN_Misc                  cMisc;

    private Rigidbody2D             cRigid;

    // Currently only used for LOOKING_FOR_VANTAGE_POINT. Subject to change.
    public Vector2Int               mGoalTilePathing;

    // First two used for only the first frame to delay state change. Hacks.
    public Vector2                  rHittingHunterVelocity;
    public bool                     mJustSentFlying = false;
    public float                    mFlyingTimeStmp;
    public float                    _flyingTime = 0.5f;

    public float                    _disEnterLeapRange = 5f;
    public float                    _disEnterCloseRange = 10f;
    public float                    _disEnterLongRange = 15f;
    public float                    _chaseSpd = 3f;
    public float                    _leapSpd = 6f;
    public float                    _leapTime = 3f;
    public float                    _leapDmg = 80f;
    private float                   mLeapTmStmp;
    public float                    _recoverTime = 1f;
    private float                   mRecoverTmStmp;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        cMisc = GetComponent<EN_Misc>();
        if(cMisc == null){
            Debug.Log("No en misc");
        }
        cMisc.cHpShlds.mHealth.mAmt = cMisc.cHpShlds.mHealth._max;
        mState = STATE.LONG_RANGE;
    }

    public override void RUN_Update()
    {
        switch(mState){
            case STATE.CLOSE_RANGE: RUN_CloseRange(); break;
            case STATE.LONG_RANGE: RUN_LongRange(rOverseer.GetComponent<MAN_Pathing>()); break;
            case STATE.LOOKING_FOR_VANTAGE_POINT: RUN_MoveToVantagePoint(rOverseer.GetComponent<MAN_Pathing>()); break;
            case STATE.LEAPING: RUN_Leap(); break;
            case STATE.RECOVER_FROM_LEAP: RUN_RecoverFromLeap(); break;
            case STATE.FLYING_AFTER_DAMAGED: RUN_RecoverFromFlyingDam(); break;
        }

        cMisc.gUI.FUpdateShieldHealthBars(cMisc.cHpShlds.mHealth.mAmt, cMisc.cHpShlds.mHealth._max);

    }

    public bool FCanRaytraceDirectlyToPlayer(PC_Cont rPC, Vector2 pos)
    {
        if(rPC == null){
            Debug.Log("Trying to raytrace to player that does not exist");
            return false;
        }

        Vector2 dif = (Vector2)rPC.transform.position - pos;
        RaycastHit2D hit = Physics2D.Raycast(pos, dif.normalized);

        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                Debug.DrawLine(pos, hit.collider.gameObject.transform.position, Color.grey);
            }
            if(hit.collider.GetComponent<PC_Cont>()){
                Debug.DrawLine(pos, hit.collider.gameObject.transform.position, Color.green);
                return true;
            }
        }
        return false;
    }

    // Have to find the right tile here.
    void ENTER_MoveToVantagePoint(MAN_Pathing pather)
    {   
        mState = STATE.LOOKING_FOR_VANTAGE_POINT;
        Debug.Log("Lost sight of player, moving to vantage point now");

        // sort all the tiles from closest to the hunter to furthest away.
        List<Vector2Int> tilesSortedClosestToFurthest = new List<Vector2Int>();
        int countValidTiles = 0;
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // have to be a valid tile, and have to be able to see the player.
                if(!pather.mPathingTiles[x,y].mCanPath) continue;

                Vector2 tilePos = pather.GetComponent<MAN_Helper>().FGetWorldPosOfTile(new Vector2Int(x,y));
                if(!FCanRaytraceDirectlyToPlayer(pather.GetComponent<Man_Combat>().rPC, tilePos)){
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
        // Debug.Log("SHould have found valid goal " + mGoalTilePathing);

        // Visual Debugging.
        MAN_Helper help = pather.GetComponent<MAN_Helper>();
        Instantiate(pather.GetComponent<MAN_Helper>().PF_Blue3, help.FGetWorldPosOfTile(mGoalTilePathing), transform.rotation);
        return;
    }

    // We've already figured out where we're moving to.
    void RUN_MoveToVantagePoint(MAN_Pathing pather)
    {
        Vector2 dest = pather.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mGoalTilePathing);
        Vector2 dif = dest - (Vector2)transform.position; 
        cRigid.velocity = _chaseSpd * dif.normalized;

        if(Vector2.Distance(transform.position, dest) < 0.1f){
            // Debug.Log("Hit vantage point spot");
            ENTER_LongRangeState();
        }
    }

    void ENTER_LongRangeState(){
        mState = STATE.LONG_RANGE;
        mChargeTmStmp = Time.time;
    }
    // Now there's a second state where we can't see the player and have to figure out where they are.
    void RUN_LongRange(MAN_Pathing pather){
        // Pick a new direction every now and then to shuffle towards
        if(Time.time - mShuffleTmStmp > _shuffleDirectionTime){
            mShuffleDir = Random.insideUnitCircle.normalized;
            mShuffleTmStmp = Time.time;
        }
        cRigid.velocity = mShuffleDir * _shuffleSpd;

        // Also firing the projectile. Would need shot charge time to be set to Time.time upon entering shot charge state.
        if(Time.time - mChargeTmStmp > _shotChargeTime){
            PJ_EN_HunterBlast rHunterBlast = Instantiate(PF_HunterBlast, transform.position, transform.rotation);
            rHunterBlast.mDestination = cMisc.rPC.transform.position;
            Vector3 vDir = cMisc.rPC.transform.position - transform.position;
            vDir = Vector3.Normalize(vDir);
            rHunterBlast.cRigid.velocity = vDir * rHunterBlast.mProjD._spd;

            mChargeTmStmp = Time.time;
        }

        if(Vector3.Distance(cMisc.rPC.transform.position, transform.position) < _disEnterCloseRange){
            // Debug.Log("Enter Close Range");
            mState = STATE.CLOSE_RANGE;
        }else if(!FCanRaytraceDirectlyToPlayer(pather.GetComponent<Man_Combat>().rPC, transform.position)){
            // Debug.Log("Lost sight of player");
            ENTER_MoveToVantagePoint(pather);
        }

    }
    void RUN_CloseRange(){
        // this is more interesting. We chase after the player, then we charge at them.
        float disToPly = Vector3.Distance(cMisc.rPC.transform.position, transform.position);
        Vector3 vDir = cMisc.rPC.transform.position - transform.position;
        cRigid.velocity = vDir.normalized * _chaseSpd;

        if(disToPly > _disEnterLongRange){
            // Debug.Log("Leave Close Range, going Long");
            ENTER_LongRangeState();
            return;
        }

        if(disToPly < _disEnterLeapRange){
            // Debug.Log("Entering Leap");
            mState = STATE.LEAPING;
            mLeapTmStmp = Time.time;
            return;
        }
    }
    void RUN_Leap()
    {
        if(mJustSentFlying){
            mJustSentFlying = false;
            mState = STATE.FLYING_AFTER_DAMAGED;
            mFlyingTimeStmp = Time.time;
            cRigid.velocity = rHittingHunterVelocity * 0.5f;
            return;
        }

        // just assume for now we're going in the right direction.
        cRigid.velocity = cRigid.velocity.normalized * _leapSpd;

        if(Time.time - mLeapTmStmp > _leapTime){
            // Debug.Log("Done leaping, recovering");
            mState = STATE.RECOVER_FROM_LEAP;
            mRecoverTmStmp = Time.time;
            return;
        }
    }
    void RUN_RecoverFromLeap()
    {
        cRigid.velocity = Vector3.zero;
        
        if(Time.time - mRecoverTmStmp > _recoverTime){
            // Debug.Log("Done recovering, enter long range");
            ENTER_LongRangeState();
        }
    }

    void RUN_RecoverFromFlyingDam()
    {
        if(Time.time - mFlyingTimeStmp > _flyingTime){
            // Debug.Log("Recovered from getting hit by fellow hunter");
            ENTER_LongRangeState();
        }
    }

    // deal with getting hit.
    void OnTriggerEnter2D(Collider2D col)
    {
        // Debug.Log("Hit: " + col.gameObject);
        if(col.GetComponent<PJ_Base>()){
            PJ_Base proj = col.GetComponent<PJ_Base>();
            FTakeDamage(proj.mProjD._damage);
        }

        // If the hunter collided with us.
        // Have to delay this one frame. 
        if(col.GetComponent<EN_Hunter>()){
            EN_Hunter h = col.GetComponent<EN_Hunter>();
            if(h.mState == EN_Hunter.STATE.LEAPING){
                // Debug.Log("Hunter smacked by leaping hunter");
                FTakeDamage(_leapDmg);
                mJustSentFlying = true;
                rHittingHunterVelocity = h.GetComponent<Rigidbody2D>().velocity;
            }
        }

        if(col.GetComponent<EX_HBlast>()){
            EX_HBlast b = col.GetComponent<EX_HBlast>();
            FTakeDamage(b._damage);
        }
        if(col.GetComponent<PC_SwordHitbox>()){
            // Debug.Log("hit by sword");
            FTakeDamage(10000f);
        }
    }

    // For now, just say that plasma damage does 2x to shields, 1/2 to health, and vice versa for human weapon.
    public void FTakeDamage(float amt)
    {
        cMisc.cHpShlds.mHealth.mAmt -= amt;
        if(cMisc.cHpShlds.mHealth.mAmt < 0f) cMisc.cHpShlds.mHealth.mAmt = 0f;
        // for now, just have the same modifier amounts, but in reverse.
        Debug.Log("Health Dam: " + amt);

        if(cMisc.cHpShlds.mHealth.mAmt <= 0f){
            //Instantiate(PF_Particles, transform.position, transform.rotation);
            Debug.Log("Guess hunter died");
            rOverseer.FRegisterDeadEnemy(this);
        }
    }

}
