/********************************************************************************
Moisha the Mossad Sniper. Tries to get to a vantage point, has slow rotation, locks on and 
fires if he's charged up his shot enough.
********************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class EN_Beamer : EN_Base
{ 
    public uint                         kLookingForVantage = 1<<2; 
    public uint                         kSettingUpShot = 1<<3; 
    public uint                         kChargingShot = 1<<4; 
    public uint                         kCooldown = 1<<5; 

    public float                        _chargeTime = 4f;
    public float                        mChargeTmStmp;
    public float                        _cooldownTime = 1f;
    public float                        mCooldownTmStmp;
    public float                        _damage = 40f;
    public float                        _turnRateDegSettingUpShot;
    public float                        _turnRateDegChargingShot;
    EN_BeamerAnimator                   cAnim;
    public LineRenderer                 cLineRender;

    public override void F_CharSpecStart()
    {
        kState = kLookingForVantage;
        cAnim = GetComponent<EN_BeamerAnimator>();
        cLineRender = GetComponent<LineRenderer>();
    }
    public override void F_CharSpecUpdate()
    {
        if(rOverseer.rPC == null) return;
        // Move to player.
        // Actually for now don't bother making this one move. 
        if(kState == kStunned){
            RUN_Stunned();
        }else if(kState == kLookingForVantage){
            RUN_FindAndMoveToVantageSpot();
        }else if(kState == kSettingUpShot){
            RUN_SettingUpShot();
        }else if(kState == kChargingShot){
            RUN_Charging();
        }else if(kState == kCooldown){
            RUN_Cooldown();
        }
        cAnim.FAnimate();
    }

    // Player could even be behind us. Point is that we have line of sight to them.
    public bool F_LineOfSightToPlayer()
    {
        // Eventually make him run to a vantage spot. For now it's just stationary.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        // If we can see the player, immediately go to charging.
        if(hit.collider.GetComponent<PC_Cont>()){
            return true;
        }

        return false;
    }

    void RUN_FindAndMoveToVantageSpot()
    {
        // If we can see the player, immediately go to charging.
        if(F_LineOfSightToPlayer()){
            kState = kSettingUpShot; 
            mChargeTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            return;
        }

        // Manually place sniper locations.
        // Figure out which ones can see the player
        List<ENV_SniperSpot> validSpots = new List<ENV_SniperSpot>();
        for(int i=0; i<rOverseer.rSniperSpots.Count; i++){
            if(rOverseer.rSniperSpots[i].mCanSeePlayer){
                validSpots.Add(rOverseer.rSniperSpots[i]);
            }
        }
        // If none can, then just don't move.
        if(validSpots.Count == 0){
            Debug.Log("No sniper spots can see player. Weird.");
            cRigid.velocity = Vector2.zero;
            return;
        }
        // Ideally, move to location close to us and far from player.
        // For now, just move to location closest to us.
        float shortestDis = Vector2.Distance(transform.position, validSpots[0].transform.position);
        int indShortest = 0;
        for(int i=1; i<validSpots.Count; i++){
            if(Vector2.Distance(transform.position, validSpots[i].transform.position) < shortestDis){
                shortestDis = Vector2.Distance(transform.position, validSpots[i].transform.position);
                indShortest = i;
            }
        }

        // Now build and follow the path.
        MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
        Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
        Vector2Int sniperSpotNode = p.FFindClosestValidTile(validSpots[indShortest].transform.position);
        mPath = p.FCalcPath(ourNode, sniperSpotNode);
        if(mPath == null){
            cRigid.velocity = Vector2.zero;
            return;
        }
        // since we're already on the start node.
        mPath.RemoveAt(0);
        Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
        cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;

        transform.up = cRigid.velocity;

        // // If they can't see the player, they need to pick a vantage point where they would like to see the player.
        // if(Input.GetMouseButton(0)){
        //     // Spawn in something that shows where the sniper would like to be.
        //     MSC_SquareMarker[] markers = FindObjectsOfType<MSC_SquareMarker>();
        //     foreach(MSC_SquareMarker m in markers){
        //         Destroy(m.gameObject);
        //     }
        //     Instantiate(PF_ChosenSpot, validSpots[indShortest].transform.position, transform.rotation);
        // }
    }

    void RUN_SettingUpShot()
    {
        if(!F_LineOfSightToPlayer()){
            kState = kLookingForVantage; 
            cLineRender.enabled = false;
            return;
        }

        // Rotate towards the player at a certain fixed rate. 
        Vector2 vDirToPlayer = (rOverseer.rPC.transform.position - transform.position).normalized; 
        Vector2 newHeading = Vector3.RotateTowards(transform.up, vDirToPlayer, Mathf.Deg2Rad *_turnRateDegSettingUpShot * Time.deltaTime, 0.0f);
        transform.up = newHeading.normalized;

        // Now, we want to see that they are looking towards the player, or at least close enough.
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up.normalized, mask);
        if(hit.collider != null){
            // If we can't see the player, immediately go to find another vantage point.
            if(hit.collider.GetComponent<PC_Cont>()){
                kState = kChargingShot; 
            }
        }
    }

    void RUN_Charging()
    {
        if(rOverseer.rPC == null) return;
        // If can't see the player, go back to hunting.
        Vector2 vDirToPlayer = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDirToPlayer.normalized, 1000f, mask);
        // If we can't see the player, immediately go to find another vantage point.
        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                kState = kLookingForVantage; 
                cLineRender.enabled = false;
                return;
            }
        }

        // Rotate towards the player at a certain fixed rate. 
        Vector2 vGoal = (rOverseer.rPC.transform.position - transform.position).normalized; 
        Vector2 newHeading = Vector3.RotateTowards(transform.up, vGoal, Mathf.Deg2Rad *_turnRateDegChargingShot * Time.deltaTime, 0.0f);
        transform.up = newHeading.normalized;

        // The line should render at huge distances, but be along the path that we are looking.
        cLineRender.enabled = true;
        List<Vector3> points = new List<Vector3>();
        points.Add(transform.position);
        float dis = Vector2.Distance(transform.position, rOverseer.rPC.transform.position);
        Vector2 endpoint = (Vector2)transform.position + ((Vector2)transform.up * dis);
        points.Add(endpoint);
        cLineRender.startWidth = 0.1f; cLineRender.endWidth = 0.1f;
        Color startCol = new Color(1f,1f,1f,0.5f);
        float percentDone = (Time.time - mChargeTmStmp) / _chargeTime;
        Color endCol = new Color(1f, (1f-percentDone), (1f-percentDone), 0.5f);
        cLineRender.startColor = startCol; cLineRender.endColor = endCol;
        cLineRender.SetPositions(points.ToArray());
        cLineRender.useWorldSpace = true;
        // cLineRender.sortingLayerID = 3;


        if(Time.time - mChargeTmStmp > _chargeTime){
            cLineRender.enabled = false;
            kState = kCooldown;
            mCooldownTmStmp = Time.time;

            // If we hit the player, make them take damage.
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, transform.up.normalized, 1000f, mask);
            // If we can see the player, immediately go to charging.
            if(hit2.collider != null){
                if(hit2.collider.GetComponent<PC_Cont>()){
                    Debug.Log("Hit the player");
                    hit2.collider.GetComponent<PC_Cont>().FHandleDamExternal(_damage, DAMAGE_TYPE.SNIPER);
                }
            }
        }
    }
    void RUN_Cooldown()
    {
        float percentDone = (Time.time - mCooldownTmStmp) / _cooldownTime;
        cLineRender.enabled = true;
        Color startCol = new Color(1f,1f,0f,1f-percentDone);
        Color endCol = new Color(1f,1f,0f,1f-percentDone);
        cLineRender.startColor = startCol; cLineRender.endColor = endCol;

        if(Time.time - mCooldownTmStmp > _cooldownTime){
            kState = kLookingForVantage;
        }
    }
    void RUN_Stunned()
    {
        cRigid.velocity = Vector2.zero;
        if(Time.time - mStunTmStmp > _stunRecTime){
            kState = kLookingForVantage;
        }
    }

}
