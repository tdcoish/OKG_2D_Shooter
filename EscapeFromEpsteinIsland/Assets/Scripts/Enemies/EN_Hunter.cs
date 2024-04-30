/*************************************************************************************
Hunter is a mess. Putting off the refactor of it for now.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class EN_Hunter : EN_Base
{
    public uint                     kLookingForVantagePoint = 1<<2;
    public uint                     kLongRange = 1<<3;
    public uint                     kCloseRange = 1<<4;
    public uint                     kPrepLeap = 1<<5;
    public uint                     kLeaping = 1<<6;
    public uint                     kRecoveringFromLeap = 1<<7;
    public uint                     kFlyingAfterDamaged = 1<<8;
    public uint                     kRotatingToPlayer = 1<<9;
    
    // for now sort of shuffle around at long range.
    public float                    _shuffleDirectionTime = 1.5f;
    public float                    mShuffleTmStmp;
    public float                    _shuffleSpd = 0.5f;
    public Vector2                  mShuffleDir;
    public float                    _maxRotationSpeedWhenTrackingToPlayer = 60f;
    public float                    _angleDifToPlayerToStartRotating = 30f;
    public float                    _angleDifToPlayerToStopRotation = 15f;

    // Adding their firing as part of their character.
    public float                    _shotChargeTime = 3f;
    public float                    mChargeTmStmp;
    public PJ_EN_HunterBlast        PF_HunterBlast;

    EN_HunterAnimator               cAnim;

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
    public float                    _prepLeapTime = 0.5f;
    float                           mPrepLeapTmStmp;
    public float                    _recoverTime = 1f;
    private float                   mRecoverTmStmp;

    public Vector2                      mTrueHeading;
    public EN_HunterLeapBox             gLeapHitbox;

    public override void F_CharSpecStart()
    {
        kState = kLongRange;
        cAnim = GetComponent<EN_HunterAnimator>();
        gLeapHitbox.gameObject.SetActive(false);
    }

    public override void F_CharSpecUpdate()
    {
        if(kState == kStunned){
            F_RunStunRecovery();
        }else if(kState == kCloseRange){
            RUN_CloseRange();
        }else if(kState == kLongRange){
            RUN_LongRange(rOverseer.GetComponent<MAN_Pathing>());
        }else if(kState == kLookingForVantagePoint){
            RUN_MoveToVantagePoint(rOverseer.GetComponent<MAN_Pathing>());
        }else if(kState == kPrepLeap){
            RUN_PrepLeap();
        }else if(kState == kLeaping){
            RUN_Leap();
        }else if(kState == kRecoveringFromLeap){
            RUN_RecoverFromLeap();
        }else if(kState == kFlyingAfterDamaged){
            RUN_RecoverFromFlyingDam();
        }else if(kState == kRotatingToPlayer){
            RUN_RotatingToPlayer();
        }
        cAnim.FAnimate();
    }

    public bool FCanRaytraceDirectlyToPlayer(Vector2 playerPos, Vector2 ourPos, LayerMask mask)
    {
        Vector2 dif = playerPos - ourPos;
        RaycastHit2D hit = Physics2D.Raycast(ourPos, dif.normalized, 1000f, mask);

        bool debugDrawLines = false;
        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                if(debugDrawLines){
                    Debug.DrawLine(ourPos, hit.collider.gameObject.transform.position, Color.grey);
                }
            }
            if(hit.collider.GetComponent<PC_Cont>()){
                if(debugDrawLines){
                    Debug.DrawLine(ourPos, hit.collider.gameObject.transform.position, Color.green);
                }
                return true;
            }
        }
        return false;
    }

    // Problem is that we're hitting ourselves sometimes.
    public bool FCanSeePlayerFromAllCornersOfBox(Vector2 playerPos, Vector2 castPos, float boxSize, LayerMask mask)
    {
        Vector2 workingPos = castPos;
        workingPos.x -= boxSize; workingPos.y -= boxSize;
        if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
            workingPos.x = castPos.x + boxSize;
            if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                workingPos = castPos; workingPos.y += boxSize; workingPos.x -= boxSize;
                if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                    workingPos.x = castPos.x + boxSize;
                    if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Have to find the right tile here.
    void ENTER_MoveToVantagePoint(MAN_Pathing pather)
    {   
        kState = kLookingForVantagePoint;
        Debug.Log("Lost sight of player, moving to vantage point now");

        // sort all the tiles from closest to the hunter to furthest away.
        List<Vector2Int> tilesSortedClosestToFurthest = new List<Vector2Int>();
        int countValidTiles = 0;
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // have to be a valid tile, and have to be able to see the player.
                if(!pather.mPathingTiles[x,y].mCanPath) continue;

                MAN_Helper h = pather.GetComponent<MAN_Helper>();
                Vector2 tilePos = h.FGetWorldPosOfTile(new Vector2Int(x,y));
                float paddingFromEdge = 1;
                LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
                if(!FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, tilePos, paddingFromEdge, mask)){
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
        // MAN_Helper help = pather.GetComponent<MAN_Helper>();
        // Instantiate(pather.GetComponent<MAN_Helper>().PF_Blue3, help.FGetWorldPosOfTile(mGoalTilePathing), transform.rotation);
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

        // We also want to check if we can see the player.
        // We might also want to check if we have a different vantage point, although maybe not.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position, 1f, mask)){
            ENTER_LongRangeState(); 
            Debug.Log("Can see PC");
        }
    }

    void ENTER_LongRangeState(){
        kState = kLongRange;
        mChargeTmStmp = Time.time;
    }
    // Now there's a second state where we can't see the player and have to figure out where they are.
    void RUN_LongRange(MAN_Pathing pather){
        if(rOverseer.rPC == null){
            return;
        }

        // Pick a new direction every now and then to shuffle towards
        if(Time.time - mShuffleTmStmp > _shuffleDirectionTime){
            mShuffleDir = Random.insideUnitCircle.normalized;
            mShuffleTmStmp = Time.time;
        }
        cRigid.velocity = mShuffleDir * _shuffleSpd;

        // Also firing the projectile. Would need shot charge time to be set to Time.time upon entering shot charge state.
        if(Time.time - mChargeTmStmp > _shotChargeTime){
            PJ_EN_HunterBlast rHunterBlast = Instantiate(PF_HunterBlast, gShotPoint.transform.position, transform.rotation);
            rHunterBlast.mDestination = rOverseer.rPC.transform.position;
            Vector3 vDir = (rOverseer.rPC.transform.position - transform.position).normalized;
            rHunterBlast.cRigid.velocity = vDir * rHunterBlast.mProjD._spd;

            mChargeTmStmp = Time.time;
        }

        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _disEnterCloseRange){
            // Debug.Log("Enter Close Range");
            kState = kCloseRange;
        }else if(!FCanRaytraceDirectlyToPlayer(pather.GetComponent<Man_Combat>().rPC.transform.position, transform.position, mask)){
            // Debug.Log("Lost sight of player");
            ENTER_MoveToVantagePoint(pather);
        }

        // For now, just always face the player.
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }
    void RUN_CloseRange(){
        // this is more interesting. We chase after the player, then we charge at them.
        float disToPly = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        Vector3 vDir = rOverseer.rPC.transform.position - transform.position;
        cRigid.velocity = vDir.normalized * _chaseSpd;
        transform.up = vDir.normalized;

        if(disToPly > _disEnterLongRange){
            // Debug.Log("Leave Close Range, going Long");
            ENTER_LongRangeState();
            return;
        }

        if(disToPly < _disEnterLeapRange){
            // Debug.Log("Entering Leap");
            ENTER_PrepLeap();
            return;
        }
    }
    void ENTER_PrepLeap()
    {
        kState = kPrepLeap;
        cRigid.velocity = Vector2.zero;
        mPrepLeapTmStmp = Time.time;
        mTrueHeading = (rOverseer.rPC.transform.position - transform.position).normalized;
    }
    void RUN_PrepLeap()
    {
        if(Time.time - mPrepLeapTmStmp > _prepLeapTime){
            kState = kLeaping;
            mLeapTmStmp = Time.time;
            cRigid.velocity = mTrueHeading.normalized * _leapSpd;
            gLeapHitbox.gameObject.SetActive(true);
        }
    }
    void RUN_Leap()
    {
        if(mJustSentFlying){
            mJustSentFlying = false;
            kState = kFlyingAfterDamaged;
            mFlyingTimeStmp = Time.time;
            cRigid.velocity = rHittingHunterVelocity * 0.5f;
            return;
        }

        // just assume for now we're going in the right direction.
        cRigid.velocity = cRigid.velocity.normalized * _leapSpd;

        if(Time.time - mLeapTmStmp > _leapTime){
            // Debug.Log("Done leaping, recovering");
            kState = kRecoveringFromLeap;
            mRecoverTmStmp = Time.time;
            gLeapHitbox.gameObject.SetActive(false);
            return;
        }
    }
    void RUN_RecoverFromLeap()
    {
        cRigid.velocity = Vector3.zero;
        
        if(Time.time - mRecoverTmStmp > _recoverTime){
            // Debug.Log("Done recovering, enter long range");
            ENTER_RotatingToPlayer();
        }
    }

    void RUN_RecoverFromFlyingDam()
    {
        if(Time.time - mFlyingTimeStmp > _flyingTime){
            // Debug.Log("Recovered from getting hit by fellow hunter");
            ENTER_RotatingToPlayer();
        }
    }
    void ENTER_RotatingToPlayer()
    {
        kState = kRotatingToPlayer;
        cRigid.velocity = Vector2.zero;
    }
    void RUN_RotatingToPlayer()
    {
        // TODO: Rotate them to the player.
        ENTER_LongRangeState();
    }
    public override void EXIT_Stun()
    {
        kState = kLongRange;
    }

}
