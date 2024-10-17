using UnityEngine;
using System.Collections.Generic;

/*********************************************************************
For now only shoots the needler.
*********************************************************************/
public class EN_Grunt : EN_Base
{
    public uint                     kReadyToFire = 1<<2;
    public uint                     kRunningToVantagePoint = 1<<3;
    public uint                     kFleeingFromPlayer = 1<<4;
    public uint                     kReloading = 1<<5;

    public float                    _shotIntervalTime = 2f;
    float                           mLastShotTmStmp;
    public float                    _reloadTime = 5f;
    float                           mReloadTmStmp;
    public int                      _maxShotsInClip = 5;
    int                             mShotsRemaining;
    public float                    _startFleeDistance = 4f;
    public float                    _fleeSpd = 3f;
    public float                    _fleeTime = 2f;
    float                           mFleeTmStmp;
    Vector2                         fleeDir;
    public Vector2                  mTrueHeading;
    public Vector2Int               mGoalTilePathing;
    public PJ_EN_Needler            PF_NeedlerRound;

    public Sprite                   rNormal;
    public Sprite                   rFlee;
    public Sprite                   rStun;
    public Sprite                   rReload;

    public override void F_CharSpecStart()
    {
        kState = kReadyToFire;
        mShotsRemaining = _maxShotsInClip;
    }
    public override void F_CharSpecUpdate()
    {
        if(rOverseer.rPC == null) return;

        if(kState == kReadyToFire){
            FRUN_FireSolutionOkay();
        }else if(kState == kRunningToVantagePoint){
            FRUN_MoveToVantagePoint();
        }else if(kState == kFleeingFromPlayer){
            FRUN_Flee();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }else if(kState == kReloading){
            FRUN_Reloading();
        }

        F_Animate();
    }

    void FRUN_Flee()
    {
        cRigid.velocity = fleeDir * _fleeSpd;
        if(Time.time - mFleeTmStmp > _fleeTime){
            kState = kReadyToFire;
            cRigid.velocity = Vector2.zero;
        }
    }

    void FRUN_FireSolutionOkay()
    {
        if(mShotsRemaining <= 0){
            kState = kReloading;
            mReloadTmStmp = Time.time;
            return;
        }

        // What should the ideal spot be? 
        // May need to know where all the other enemies are, because we want to spread out.
        if(Vector2.Distance(rOverseer.rPC.transform.position, transform.position) < _startFleeDistance){
            kState = kFleeingFromPlayer;
            fleeDir = -1f * (rOverseer.rPC.transform.position - transform.position).normalized;
            mFleeTmStmp = Time.time;
            return;
        }

        if(Time.time - mLastShotTmStmp > _shotIntervalTime){
            PJ_EN_Needler n = Instantiate(PF_NeedlerRound, transform.position, transform.rotation);
            n.FShootAt(rOverseer.rPC.transform.position, transform.position, this.gameObject);
            mLastShotTmStmp = Time.time;
            mShotsRemaining--;
        }

        if(!F_CanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position)){
            FENTER_MoveToVantagePoint();
        }
    }
    void FRUN_Reloading()
    {
        // Copied from the fire solution state. Getting close interrupts their reload.
        if(Vector2.Distance(rOverseer.rPC.transform.position, transform.position) < _startFleeDistance){
            kState = kFleeingFromPlayer;
            fleeDir = -1f * (rOverseer.rPC.transform.position - transform.position).normalized;
            mFleeTmStmp = Time.time;
            return;
        }

        if(Time.time - mReloadTmStmp > _reloadTime){
            kState = kReadyToFire;
            mShotsRemaining = _maxShotsInClip;
            mLastShotTmStmp = Time.time;
        }

    }

    void FENTER_MoveToVantagePoint() 
    {   
        MAN_Pathing pather = rOverseer.GetComponent<MAN_Pathing>();
        MAN_Helper h = rOverseer.GetComponent<MAN_Helper>();
        kState = kRunningToVantagePoint;
        Debug.Log("Lost sight of player, moving to vantage point now");

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
        kState = kReadyToFire;
        cRigid.velocity = Vector2.zero;
    }
    public override void EXIT_PoiseBreak(){
        kState = kReadyToFire;
        mShotsRemaining = 0;
    }
    public void F_Animate()
    {
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        if(kState == kFleeingFromPlayer){
            s.sprite = rFlee;
        }else if(kState == kPoiseBroke){
            s.sprite = rStun;
        }else if(kState == kRunningToVantagePoint || kState == kReadyToFire){
            s.sprite = rNormal;
        }else if(kState == kReloading){
            s.sprite = rReload;
        }
    }


}
