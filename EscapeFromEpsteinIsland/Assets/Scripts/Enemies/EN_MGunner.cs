/*************************************************************************************
Machine gunner just sets up his guns to shoot, and flees when the player gets close.

Want them to lay down suppressive fire when the player runs away.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class EN_MGunner : EN_Base
{
    public uint                     kFiring = 1<<2;
    public uint                     kRunningToVantagePoint = 1<<3;
    public uint                     kFleeingFromPlayer = 1<<4;
    public uint                     kReloading = 1<<5;

    public float                    _shotSpread = 10f;
    public int                      _shotsPerSalvo = 10;
    float                           mLastShotTmStmp;
    int                             mShotCounter = 0;
    public float                    _shotInterval;
    public float                    _salvoInterval;
    float                           mSalvoEndedTmStmp;
    public PJ_EN_Plasmoid           PF_Projectile;

    public float                    _startFleeDistance = 4f;
    public float                    _fleeSpd = 3f;
    public float                    _fleeTime = 2f;
    float                           mFleeTmStmp;
    Vector2                         fleeDir;
    public Vector2                  mTrueHeading;
    public Vector2Int               mGoalTilePathing;
    // Machine Gunner is not supposed to be a laxadaisical turner.
    public float                    _turnRateDegPerSec = 360f;

    public Sprite                   rNormal;
    public Sprite                   rFlee;
    public Sprite                   rStun;
    public Sprite                   rReload;

    public override void F_CharSpecStart()
    {
        kState = kFiring;
    }
    public override void F_CharSpecUpdate()
    {
        if(rOverseer.rPC == null) return;

        if(kState == kFiring){
            FRUN_Firing(); 
        }else if(kState == kReloading){
            FRUN_Reloading();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }
        F_Animate();
    }

    public void FRUN_Firing()   
    {
        // For now, all we do is the firing part.
        Vector3 vDirToPC = (rOverseer.rPC.transform.position - gShotPoint.transform.position).normalized;
        Vector2 newHeading = Vector3.RotateTowards(transform.up, vDirToPC, Mathf.Deg2Rad *_turnRateDegPerSec * Time.deltaTime, 0.0f);
        transform.up = newHeading.normalized;

        if(Time.time - mLastShotTmStmp > _shotInterval){

            void ShootBullet(Vector2 vDirToPC, float angle)
            {
                PJ_EN_Plasmoid p = Instantiate(PF_Projectile, gShotPoint.transform.position, transform.rotation);
                float radiansOfTurn = angle*Mathf.Deg2Rad;
                Vector2 vDir = vDirToPC;
                vDir.x = vDirToPC.x * Mathf.Cos(radiansOfTurn) - vDir.y * Mathf.Sin(radiansOfTurn);
                vDir.y = vDirToPC.x * Mathf.Sin(radiansOfTurn) + vDir.y * Mathf.Cos(radiansOfTurn);
                p.cRigid.velocity = vDir.normalized * p.mProjD._spd;
                p.transform.up = p.cRigid.velocity.normalized;
                p.mProjD.rOwner = gameObject;
            }

            // ShootBullet(vDirToPC, _shotSpread*2f);
            ShootBullet(vDirToPC, _shotSpread);
            ShootBullet(vDirToPC, 0f);
            ShootBullet(vDirToPC, -_shotSpread);
            // ShootBullet(vDirToPC, -_shotSpread*2f);

            mLastShotTmStmp = Time.time;
            mShotCounter++;
            if(mShotCounter >= _shotsPerSalvo){
                mSalvoEndedTmStmp = Time.time;
                kState = kReloading;
            }
        }
    }
    public void FRUN_Reloading()
    {
        if(Time.time - mSalvoEndedTmStmp > _salvoInterval){
            kState = kFiring;
            mShotCounter = 0;
        }
    }

    void FRUN_Flee()
    {
        cRigid.velocity = fleeDir * _fleeSpd;
        if(Time.time - mFleeTmStmp > _fleeTime){
            kState = kFiring;
            cRigid.velocity = Vector2.zero;
        }
    }

    // void FRUN_FireSolutionOkay()
    // {
    //     if(mShotsRemaining <= 0){
    //         kState = kReloading;
    //         mReloadTmStmp = Time.time;
    //         return;
    //     }

    //     // What should the ideal spot be? 
    //     // May need to know where all the other enemies are, because we want to spread out.
    //     if(Vector2.Distance(rOverseer.rPC.transform.position, transform.position) < _startFleeDistance){
    //         kState = kFleeingFromPlayer;
    //         fleeDir = -1f * (rOverseer.rPC.transform.position - transform.position).normalized;
    //         mFleeTmStmp = Time.time;
    //         return;
    //     }

    //     if(Time.time - mLastShotTmStmp > _shotIntervalTime){
    //         // PJ_EN_Needler n = Instantiate(PF_NeedlerRound, transform.position, transform.rotation);
    //         // n.FShootAt(rOverseer.rPC.transform.position, transform.position, this.gameObject);
    //         // mLastShotTmStmp = Time.time;
    //         // mShotsRemaining--;
    //     }

    //     LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
    //     Vector2 dif = (rOverseer.rPC.transform.position - transform.position).normalized;
    //     RaycastHit2D hit = Physics2D.Raycast(transform.position, dif, 1000f, mask);
    //     if(hit.collider != null){
    //         if(!hit.collider.GetComponent<PC_Cont>()){
    //             Debug.Log("Lost sight of player");
    //             FENTER_MoveToVantagePoint();
    //         }
    //     }
    // }

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
        kState = kFiring;
        cRigid.velocity = Vector2.zero;
    }
    public override void EXIT_PoiseBreak(){
        kState = kFiring;
        mLastShotTmStmp = Time.time;
        mSalvoEndedTmStmp = Time.time;
        mShotCounter = 0;
    }
    public void F_Animate()
    {
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        if(kState == kFleeingFromPlayer){
            s.sprite = rFlee;
        }else if(kState == kPoiseBroke){
            s.sprite = rStun;
        }else if(kState == kRunningToVantagePoint || kState == kFiring){
            s.sprite = rNormal;
        }else if(kState == kReloading){
            s.sprite = rReload;
        }
    }

}
