using UnityEngine;
using System.Collections.Generic;

public class EN_BPBertha : EN_Base
{
    public uint                         kFollowingPlayer = 1<<2; 
    public uint                         kPreExplosion = 1<<3; 

    public float                        _explosionChargeTime = 4f;
    public float                        mChargeTmStmp;
    public float                        _disToTriggerPreExplosion;
    EN_BPBerthaAnim                     cAnim;

    public EX_BPBertha                  PF_Explosion;

    public override void F_CharSpecStart()
    {
        kState = kFollowingPlayer;
        cAnim = GetComponent<EN_BPBerthaAnim>();
    }

    public override void F_CharSpecUpdate()
    {
        if(rOverseer.rPC == null) return;
        // Move to player.
        // Actually for now don't bother making this one move. 
        if(kState == kStunned){
            bool preExplosionTriggered = F_CheckDistanceToPlayerAndStartPreExplosion();
            if(!preExplosionTriggered){
                F_RunStunRecovery();
            }
        }else if(kState == kFollowingPlayer){
            F_Run_FollowPlayer();
        }else if(kState == kPreExplosion){
            F_Run_PreExplosion();
        }
        cAnim.FAnimate();
    }

    // Holy side effects batman.
    public bool F_CheckDistanceToPlayerAndStartPreExplosion()
    {
        if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) < _disToTriggerPreExplosion){
            kState = kPreExplosion;
            mChargeTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            return true;
        }
        return false;
    }
    public void F_Run_FollowPlayer()
    {
        if(F_CheckDistanceToPlayerAndStartPreExplosion()){
            return;
        }

        // Otherwise follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(hit.collider == null) return;
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            if(mPath == null){
                return;
            }
            mPath.RemoveAt(0);
            Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
            cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }

    }
    public void F_Run_PreExplosion()
    {
        if(Time.time - mChargeTmStmp > _explosionChargeTime){
            F_Death();
        }
    }
    public override void EXIT_Stun()
    {
        kState = kFollowingPlayer;
    }

    public void F_Death()
    {
        // Create huge explosion
        EX_BPBertha e = Instantiate(PF_Explosion, transform.position, transform.rotation);
        e.F_Start(rOverseer, this);
        rOverseer.FRegisterDeadEnemy(this);
    }
}
