using UnityEngine;
using System.Collections.Generic;

/**************************************************************
Right now the mine gets close to the player, then detonates. When it detonates it releases a bunch of plasmoids that spread out, and do progressively less damage further they 
spread out, before dwindling to nothing. 
**************************************************************/

public class EN_MovingMine : Actor
{
    public enum State {HUNTING, PRE_EXPLOSION}
    public State                        mState;
    public float                        _preExplosionTime = 0.5f;
    public float                        mPreExplosionTmStmp;
    public float                        _spd = 2f;
    public float                        _damage = 20f;
    public float                        _explosionTriggerDistance = 0.5f;
    Rigidbody2D                         cRigid;
    EN_MineAnimator                     cAnim;
    public PJ_MineShot                  PF_MineShot;
    public DIRECTION                    mHeading;

    List<Vector2Int>                    mPath;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mState = State.HUNTING;
        cAnim = GetComponent<EN_MineAnimator>();
    }

    public void FENTER_PreExplosion()
    {
        mState = State.PRE_EXPLOSION;
        cRigid.velocity = Vector2.zero;
        mPreExplosionTmStmp = Time.time;
    }
    public void FRUN_PreExplosion()
    {
        if(Time.time - mPreExplosionTmStmp > _preExplosionTime){
            void ShootOutMineShot(Vector2 dir){
                PJ_MineShot s = Instantiate(PF_MineShot, transform.position, transform.rotation);
                s.FRUN_Start(dir);
            }
            Vector2 shotDir = Vector2.zero; 
            shotDir.x = 1f;ShootOutMineShot(shotDir);
            shotDir.y = 1f;ShootOutMineShot(shotDir);
            shotDir.y = -1f;ShootOutMineShot(shotDir);
            shotDir.x = -1f; shotDir.y = 0f;ShootOutMineShot(shotDir);
            shotDir.y = 1f;ShootOutMineShot(shotDir);
            shotDir.y = -1f;ShootOutMineShot(shotDir);
            shotDir.x = 0f; shotDir.y = 1f;ShootOutMineShot(shotDir);
            shotDir.y = -1f; ShootOutMineShot(shotDir);

            rOverseer.FRegisterDeadEnemy(this);
            return;
        }
    }

    public void FRUN_Hunting()
    {
        if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) < _explosionTriggerDistance){
            // For now just spawn the shot in all 8 directions.
            FENTER_PreExplosion();
            return;
        }

        // Just follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            mPath.RemoveAt(0);
            Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
            cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }

        mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(cRigid.velocity.normalized);
    }

    public override void RUN_Update()
    {
        switch(mState){
            case State.HUNTING: FRUN_Hunting(); break;
            case State.PRE_EXPLOSION: FRUN_PreExplosion(); break;
        }

        cAnim.FAnimate();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PC_SwordHitbox>()){
            Debug.Log("Hit by sword, time to die.");
            rOverseer.FRegisterDeadEnemy(this);
        }
        if(col.GetComponent<PJ_PC_Firebolt>()){
            Debug.Log("Hit by firebolt. Also dying");
            rOverseer.FRegisterDeadEnemy(this);
        }
    }

}
