using UnityEngine;
using System.Collections.Generic;

public class EN_Knight : EN_Base
{
    public uint                         kHunting = 1<<2;
    public uint                         kBoomerCharge = 1<<3;
    public uint                         kBoomerRecover = 1<<4;
    public uint                         kSlashCharge = 1<<5;
    public uint                         kSlashCutting = 1<<6;
    public uint                         kSlashRecover = 1<<7;

    EN_KnightAnimator                   cKnightAnim;
    public bool                         mGoalLongRange = true;

    public float                        _boomerThrowDistanceTriggerMax = 16f;
    public float                        _boomerThrowDistanceTriggerMin = 14f;
    public float                        _boomerChargeTime = 1.5f;
    float                               mBoomerChargeTmStmp;
    public float                        _boomerSpd = 8f;
    public float                        _boomerTimeToApex;
    float                               _boomerTimeWaitingForReturn;
    float                               mBoomerRecTmStmp;
    Vector2                             mBoomerangTargetSpot;
    public float                        _changeToShortRangeDistance = 6f;
    public float                        _changeToLongRangeDistance = 10f;
    public float                        _basicAtkDistanceTrigger = 2f;
    public float                        _basicAtkChargeTime = 1f;
    float                               mSlashChargeTmStmp;
    public float                        _basicAtkMoveSpd = 10f;
    public float                        _basicAtkTimeLength = 0.1f;
    float                               mAtkTmStmp;
    public float                        _basicAtkRecoverTimeLength = 1f;
    float                               mAtkEndTmStmp;
    Vector2                             mSlashTargetSpot;
    public float                        _maxHealth = 200f;
    public float                        mHealth;

    public float                        _bloodDropInterval = 0.5f;
    public float                        mBloodDropTmStmp;
    public EN_TroonBloodDrop            PF_BloodDrop;

    public PJ_Boomerang                 PF_Boomerang;
    public EN_KnightHitbox              gSlashHitbox;

    public float                        _pathingUpdateRate = 0.2f;
    float                               pathUpdateTmStmp = -1f;

    public override void F_CharSpecStart()
    {
        cKnightAnim = GetComponent<EN_KnightAnimator>();

        // We need to make the boomerang distance be based on the actual time that it takes to get to the player.
        _boomerTimeToApex = _boomerThrowDistanceTriggerMax*1.5f / _boomerSpd;
        _boomerTimeWaitingForReturn = _boomerTimeToApex * 2f;
        gSlashHitbox.gameObject.SetActive(false);
        kState = kHunting;
    }

    public override void F_CharSpecUpdate()
    {
        if(kState == kStunned){
            F_RunStunRecovery();
        }else if(kState == kHunting){
            FHunting();
        }else if(kState == kBoomerCharge){
            FChargingBoomerang();
        }else if(kState == kBoomerRecover){
            FBoomerRecover();
        }else if(kState == kSlashCharge){
            FChargeSlash();
        }else if(kState == kSlashCutting){
            FSlashing();
        }else if(kState == kSlashRecover){
            FAttackRecovery();
        } 
        cKnightAnim.FAnimate();

        CheckAndDropBloodDecal();
    }

    void CheckAndDropBloodDecal()
    {
        if(Time.time - _bloodDropInterval > mBloodDropTmStmp){
            mBloodDropTmStmp = Time.time;
            EN_TroonBloodDrop b = Instantiate(PF_BloodDrop, transform.position, transform.rotation);
            b.FRunStart(rOverseer);
        }
    }

    /************************************************
    If they can see the player, just move to him. Question is whether I'll allow the other enemies to block their view.
    ************************************************/
    bool CanSeePlayer(Vector2 pos)
    {
        Vector2 dif = (Vector2)rOverseer.rPC.transform.position - pos;
        LayerMask mask = LayerMask.GetMask("PC") | LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(pos, dif.normalized, 1000f, mask);

        bool debugDrawLines = false;
        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                if(debugDrawLines){
                    Debug.DrawLine(pos, hit.collider.gameObject.transform.position, Color.grey);
                }
            }
            if(hit.collider.GetComponent<PC_Cont>()){
                if(debugDrawLines){
                    Debug.DrawLine(pos, hit.collider.gameObject.transform.position, Color.green);
                }
                return true;
            }
        }

        return false;
    }

    bool CanSeePlayerFromAllCornersOfBox(Vector2 pos, float size)
    {
        Vector2 workingPos = pos;
        workingPos.x -= size; workingPos.y -= size;
        if(CanSeePlayer(workingPos)){
            workingPos.x = pos.x + size;
            if(CanSeePlayer(workingPos)){
                workingPos = pos; workingPos.y += size; workingPos.x -= size;
                if(CanSeePlayer(workingPos)){
                    workingPos.x = pos.x + size;
                    if(CanSeePlayer(workingPos)){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /************************************************************************************************************************
    When hunting, the knight looks for an area to throw the boomerang. This requires the area in between the player and the knight
    to be free of blocks. What that means is that we can't simply pick one spot in particular to move to. We have to pick a 
    range. Perhaps we can sample in a circle around the spot until we find an appropriate area that is free of obstruction.


    ************************************************************************************************************************/

    // Need to do pathing here.
    public void FHunting() 
    {
        if(rOverseer.rPC == null){
            return;
        }
        MAN_Helper helper = FindObjectOfType<MAN_Helper>();
        transform.up = cRigid.velocity.normalized;
        MAN_Pathing pather = rOverseer.GetComponent<MAN_Pathing>();

        float disToPlayer = Vector2.Distance(transform.position, rOverseer.rPC.transform.position);
        Vector2 vDirToPlayer = (rOverseer.rPC.transform.position - transform.position).normalized;
        if(mGoalLongRange){
            if(Time.time - pathUpdateTmStmp > _pathingUpdateRate){
                pathUpdateTmStmp = Time.time;
                MSC_SquareMarker[] markers = FindObjectsOfType<MSC_SquareMarker>();
                foreach(MSC_SquareMarker m in markers){
                    Destroy(m.gameObject);
                }

                if(disToPlayer < _changeToShortRangeDistance){
                    mGoalLongRange = false;
                    Debug.Log("switching to short range");
                    return;
                }

                if(disToPlayer < _boomerThrowDistanceTriggerMax && disToPlayer > _boomerThrowDistanceTriggerMin){
                    Debug.Log("Right dis");
                    if(CanSeePlayerFromAllCornersOfBox(transform.position, 0.5f)){
                        Debug.Log("Enter charge boomer");
                        FEnterChargeBoomer();
                        return;
                    }
                }
                
                // Ripped out old code that relied on node pathfinding. New code just moves away from the player. 
                if(disToPlayer < _boomerThrowDistanceTriggerMin){
                    cRigid.velocity = vDirToPlayer * -1f * _spd;
                }else if(disToPlayer > _boomerThrowDistanceTriggerMax){
                    cRigid.velocity = vDirToPlayer * _spd;
                }

            }
            
        }else{
            if(disToPlayer < _basicAtkDistanceTrigger){
                // attack the player
                FEnterChargeSlash();
            }else{
                // move to the player
                cRigid.velocity = vDirToPlayer * _spd;
            }
            
            if(disToPlayer > _changeToLongRangeDistance){
                mGoalLongRange = true;
            }
        }
    }

    public void FEnterChargeSlash()
    {
        kState = kSlashCharge;
        mSlashChargeTmStmp = Time.time;
        mSlashTargetSpot = rOverseer.rPC.transform.position;
        cRigid.velocity = Vector2.zero;
    }
    public void FChargeSlash()
    {
        if(Time.time - mSlashChargeTmStmp > _basicAtkChargeTime){
            FEnterBasicAttack();
        }
    }
    public void FEnterBasicAttack()
    {
        gSlashHitbox.gameObject.SetActive(true);
        mAtkTmStmp = Time.time;
        kState = kSlashCutting;
        cRigid.velocity = ((Vector3)mSlashTargetSpot - transform.position).normalized * _basicAtkMoveSpd;
        transform.up = cRigid.velocity.normalized;
    }
    public void FSlashing()
    {
        if(Time.time - mAtkTmStmp > _basicAtkTimeLength){
            FEnterSlashRecover();
        }
    }
    public void FEnterSlashRecover()
    {
        gSlashHitbox.gameObject.SetActive(false);
        cRigid.velocity = Vector2.zero;
        mAtkEndTmStmp = Time.time;
        kState = kSlashRecover;
    }
    public void FAttackRecovery()
    {
        if(Time.time - mAtkEndTmStmp > _basicAtkRecoverTimeLength){
            kState = kHunting;
        }
    }
    public void FEnterChargeBoomer()
    {
        cRigid.velocity = Vector2.zero;
        mBoomerChargeTmStmp = Time.time;
        kState = kBoomerCharge;
        mBoomerangTargetSpot = rOverseer.rPC.transform.position;
        transform.up = ((Vector3)mBoomerangTargetSpot - transform.position).normalized;
    }
    public void FChargingBoomerang()
    {
        if(Time.time - mBoomerChargeTmStmp > _boomerChargeTime){
            PJ_Boomerang b = Instantiate(PF_Boomerang, transform.position, transform.rotation);
            Vector2 vel = (mBoomerangTargetSpot - (Vector2)transform.position).normalized;
            b.FThrowBoomerang(_boomerTimeToApex, _boomerSpd, vel);

            FEnterBoomerRecover();
        }
    }
    public void FEnterBoomerRecover()
    {
        kState = kBoomerRecover;
        mBoomerRecTmStmp = Time.time;
    }
    public void FBoomerRecover()
    {
        if(Time.time - mBoomerRecTmStmp > _boomerTimeWaitingForReturn){
            kState = kHunting;
        }
    }
    public override void EXIT_Stun()
    {
        Debug.Log("Should be hunting now.");
        kState = kHunting;
    }

}
