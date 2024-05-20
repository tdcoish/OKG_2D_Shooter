/****************************************************************************************************
Okay. The new basic enemy, the NPC. These are the totally unthinking reddit-tier golems who make up
the mass of the golem army. The current design is just them slowly shambling their way to the player, 
and then attacking you with a very difficult to dodge melee swipe. I think each individual one should 
be dodgeable, but I'm not sure. We can also just go the Vampire Survivors route and have the enemies
just damage you in a radius around them. That way you can't just run through them. Saves me the need 
to create a melee. Hell, saves me the need to create any states but shambling and Hitstunned.
****************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EN_NPC : EN_Base
{
    public EN_NPCAnim                   cAnim;
    public uint                         kShambling = 1<<2;
    public float                        _preExplosionTime = 0.5f;
    public float                        mPreExplosionTmStmp;
    public float                        _damRadius = 1.5f;
    public float                        _damTick = 0.1f;
    public float                        _attackRadius = 1f;
    public float                        _damagePerSecond = 40f;
    public float                        mLastDamTmStmp;

    public override void F_CharSpecStart()
    {
        kState = kShambling;
    }
    
    public override void F_CharSpecUpdate()
    {
        if(kState == kShambling){
            F_RunShambling();
        }else if(kState == kStunned){
            F_RunStunRecovery();
        }

        cAnim.FAnimate();
    }

    public override void EXIT_Stun()
    {
        kState = kShambling;
    }

    public void F_RunShambling()
    {
        if(rOverseer.rPC == null){
            return;
        }
        // Just follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(hit.collider == null){
            Debug.Log("NPC raycast hit null. Weird");
            return;
        }
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            if(mPath != null){
                mPath.RemoveAt(0);
                Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
                cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;
            }else{
                Debug.Log("NPC path null.");
            }

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }
        
        PC_Cont rPC = rOverseer.rPC;
        if(Vector3.Distance(rPC.transform.position, transform.position) < _attackRadius){
            // Attack player.
            rPC.F_GetZappedByNPC(_damagePerSecond);
        }

        transform.up = cRigid.velocity.normalized;
    }

}
