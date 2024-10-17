/****************************************************************************************************
Antifas are the NPCs, but on steroids. 

Has a protective circular ring of stench at all times. 
Throws water bottle full of piss at the player.
****************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EN_Antifa : EN_Base
{
    public EN_AntifaAnim                cAnim;
    public uint                         kShambling = 1<<2;
    public uint                         kWindup = 1<<3;
    public uint                         kThrowRec = 1<<4;

    public float                        _windupTime = 3f;
    public float                        mWindupTmStmp;
    public float                        _throwRecTime = 1f;
    public float                        mThrowTmStmp;
    public float                        _switchToCloseRange = 2.5f;
    public float                        _pissBottleThrowRange = 3f;

    public float                        _damTick = 0.1f;
    public float                        mLastDamTmStmp;
    public float                        _attackRadius = 1f;
    public float                        _damagePerSecond = 40f;

    public PJ_PissBottle                PF_PissBottle;

    public override void F_CharSpecStart()
    {
        kState = kShambling;
    }
    
    public override void F_CharSpecUpdate()
    {
        if(kState == kShambling){
            F_RunShambling();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }else if(kState == kWindup){
            F_RunWindup();
        }else if(kState == kThrowRec){
            F_RunThrowRec();
        }

        cAnim.FAnimate();
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kShambling;
    }

    public void F_RunWindup()
    {
        // If we can't see the player, start tracking again.
        // URGENT! The raycast function took the wrong parameters.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC", "ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, Mathf.Infinity, mask);
        if(hit.collider == null){
            Debug.Log("NPC raycast hit null. Weird");
            return;
        }
        if(!hit.collider.GetComponent<PC_Cont>()){
            kState = kShambling;
            return;
        }

        // If the player gets close, try to just chase them.
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _switchToCloseRange){
            kState = kShambling;
            return;
        }

        // Otherwise continue windup.
        cRigid.velocity = Vector2.zero;
        if(rOverseer.rPC == null) return;
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
        if(Time.time - mWindupTmStmp > _windupTime){
            kState = kThrowRec;
            mThrowTmStmp = Time.time;
            PJ_PissBottle p = Instantiate(PF_PissBottle, transform.position, transform.rotation);
            p.FRunStart(rOverseer.rPC.transform.position);
        }
    }
    public void F_RunThrowRec()
    {
        cRigid.velocity = Vector2.zero;
        if(Time.time - mThrowTmStmp > _throwRecTime){
            kState = kShambling;
        }
    }

    public void F_RunShambling()
    {
        if(rOverseer.rPC == null){
            return;
        }

        if(!F_CanSeePlayerFromAllCornersOfBox(rOverseer.rPC.transform.position, transform.position, 0.1f)){
            // Pathfind to player.
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
            cRigid.velocity = (rOverseer.rPC.transform.position - transform.position).normalized * _spd;

            float disToPC = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
            if(disToPC < _pissBottleThrowRange && disToPC > _switchToCloseRange){
                kState = kWindup;
                mWindupTmStmp = Time.time;
                return;
            }
        }
       
        PC_Cont rPC = rOverseer.rPC;
        if(Vector3.Distance(rPC.transform.position, transform.position) < _attackRadius){
            // Attack player.
            rPC.F_GetZappedByNPC(_damagePerSecond);
        }

        transform.up = cRigid.velocity.normalized;
    }

}
