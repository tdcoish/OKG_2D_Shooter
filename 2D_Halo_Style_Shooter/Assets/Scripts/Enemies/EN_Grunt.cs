using UnityEngine;
using System.Collections.Generic;

/*********************************************************************
For now only shoots the needler.
*********************************************************************/
public class EN_Grunt : Actor
{
    public enum STATE{READY_TO_FIRE, RUNNING_TO_VANTAGE_POINT, FLEEING_FROM_PLAYER}
    public STATE                    mState = STATE.READY_TO_FIRE;

    public float                    _spd = 1.5f;
    public float                    _shotIntervalTime = 2f;
    float                           mLastShotTmStmp;
    public float                    _reloadTime = 5f;
    float                           mReloadTmStmp;
    public float                    _startFleeDistance = 4f;
    public float                    _fleeSpd = 3f;
    public float                    _fleeTime = 2f;
    float                           mFleeTmStmp;
    Vector2                         fleeDir;

    Rigidbody2D                     cRigid;    
    public DIRECTION                mHeading;
    public Vector2                  mTrueHeading;

    public Vector2Int               mGoalTilePathing;

    public PJ_EN_Needler            PF_NeedlerRound;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
    }
    public override void RUN_Update()
    {
        switch(mState){
            case STATE.READY_TO_FIRE: FRUN_FireSolutionOkay(); break;
            case STATE.RUNNING_TO_VANTAGE_POINT: FRUN_MoveToVantagePoint(); break;
            case STATE.FLEEING_FROM_PLAYER: FRUN_Flee(); break;
        }

    }

    void FRUN_Flee()
    {
        cRigid.velocity = fleeDir * _fleeSpd;
        if(Time.time - mFleeTmStmp > _fleeTime){
            mState = STATE.READY_TO_FIRE;
            cRigid.velocity = Vector2.zero;
        }
    }

    void FRUN_FireSolutionOkay()
    {
        // What should the ideal spot be? 
        // May need to know where all the other enemies are, because we want to spread out.
        if(Vector2.Distance(rOverseer.rPC.transform.position, transform.position) < _startFleeDistance){
            mState = STATE.FLEEING_FROM_PLAYER;
            fleeDir = -1f * (rOverseer.rPC.transform.position - transform.position).normalized;
            mFleeTmStmp = Time.time;
            return;
        }

        if(Time.time - mLastShotTmStmp > _shotIntervalTime){
            PJ_EN_Needler n = Instantiate(PF_NeedlerRound, transform.position, transform.rotation);
            n.FShootAt(rOverseer.rPC.transform.position, transform.position, this.gameObject);
            mLastShotTmStmp = Time.time;
        }

        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        Vector2 dif = (rOverseer.rPC.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dif, 1000f, mask);
        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                Debug.Log("Lost sight of player");
                FENTER_MoveToVantagePoint();
            }
        }
    }
    void FENTER_MoveToVantagePoint() 
    {   
        MAN_Pathing pather = rOverseer.GetComponent<MAN_Pathing>();
        MAN_Helper h = rOverseer.GetComponent<MAN_Helper>();
        mState = STATE.RUNNING_TO_VANTAGE_POINT;
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

    void FRUN_MoveToVantagePoint()
    {
        Vector2 dest = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mGoalTilePathing);
        Vector2 dif = dest - (Vector2)transform.position; 
        cRigid.velocity = _spd * dif.normalized;

        if(Vector2.Distance(transform.position, dest) < 0.1f){
            // Debug.Log("Hit vantage point spot");
            FENTER_FiringState();
        }

        // We also want to check if we can see the player.
        // We might also want to check if we have a different vantage point, although maybe not.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        if(rOverseer.GetComponent<MAN_Helper>().FCanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position, 1f, mask)){
            FENTER_FiringState(); 
        }
    }

    void FENTER_FiringState()
    {
        mState = STATE.READY_TO_FIRE;
        cRigid.velocity = Vector2.zero;
    }

}
