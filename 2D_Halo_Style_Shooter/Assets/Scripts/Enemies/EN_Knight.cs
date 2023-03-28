using UnityEngine;

public class EN_Knight : Actor
{
    public enum STATE{HUNTING, BOOMER_CHARGE, BOOMER_RECOVER, SLASH_CHARGE, SLASH_CUTTING, SLASH_RECOVER}
    public STATE                        mState = STATE.HUNTING;
    EN_KnightAnimator                   cKnightAnim;
    public Rigidbody2D                  cRigid;
    public PC_Cont                      rPC;

    public float                        _spd = 5f;
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

    public PJ_Boomerang                 PF_Boomerang;
    public EN_KnightHitbox              gSlashHitbox;


    public DIRECTION                    mHeading;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        rPC = FindObjectOfType<PC_Cont>();
        cKnightAnim = GetComponent<EN_KnightAnimator>();

        // We need to make the boomerang distance be based on the actual time that it takes to get to the player.
        _boomerTimeToApex = _boomerThrowDistanceTriggerMax / _boomerSpd;
        _boomerTimeWaitingForReturn = _boomerTimeToApex * 2f;
        gSlashHitbox.gameObject.SetActive(false);

        mHeading = DIRECTION.UP;
    }

    public override void RUN_Update()
    {
        switch(mState){
            case STATE.HUNTING: FHunting(); break;
            case STATE.BOOMER_CHARGE: FChargingBoomerang(); break;
            case STATE.BOOMER_RECOVER: FBoomerRecover(); break;
            case STATE.SLASH_CHARGE: FChargeSlash(); break;
            case STATE.SLASH_CUTTING: FSlashing(); break;
            case STATE.SLASH_RECOVER: FAttackRecovery(); break;
        }
        cKnightAnim.FAnimate();
    }


    public void FHunting()
    {
        MAN_Helper helper = FindObjectOfType<MAN_Helper>();
        mHeading = helper.FGetCardinalDirection(cRigid.velocity.normalized);

        float disToPlayer = Vector2.Distance(transform.position, rPC.transform.position);
        Vector2 vDirToPlayer = (rPC.transform.position - transform.position).normalized;
        if(mGoalLongRange){
            if(disToPlayer < _boomerThrowDistanceTriggerMax && disToPlayer > _boomerThrowDistanceTriggerMin){
                // Throw boomerang
                FEnterChargeBoomer();
            }else if(disToPlayer > _boomerThrowDistanceTriggerMax){
                // Move to player.
                cRigid.velocity = vDirToPlayer * _spd;
            }else if(disToPlayer < _boomerThrowDistanceTriggerMin){
                // Move from player.
                cRigid.velocity = vDirToPlayer * -_spd;
            }

            if(disToPlayer < _changeToShortRangeDistance){
                mGoalLongRange = false;
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
        mState = STATE.SLASH_CHARGE;
        mSlashChargeTmStmp = Time.time;
        mSlashTargetSpot = rPC.transform.position;
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
        mState = STATE.SLASH_CUTTING;
        cRigid.velocity = ((Vector3)mSlashTargetSpot - transform.position).normalized * _basicAtkMoveSpd;
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
        mState = STATE.SLASH_RECOVER;
    }
    public void FAttackRecovery()
    {
        if(Time.time - mAtkEndTmStmp > _basicAtkRecoverTimeLength){
            mState = STATE.HUNTING;
        }
    }
    public void FEnterChargeBoomer()
    {
        cRigid.velocity = Vector2.zero;
        mBoomerChargeTmStmp = Time.time;
        mState = STATE.BOOMER_CHARGE;
        mBoomerangTargetSpot = rPC.transform.position;
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        mHeading = h.FGetCardinalDirection(((Vector3)mBoomerangTargetSpot - transform.position).normalized);
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
        mState = STATE.BOOMER_RECOVER;
        mBoomerRecTmStmp = Time.time;
    }
    public void FBoomerRecover()
    {
        if(Time.time - mBoomerRecTmStmp > _boomerTimeWaitingForReturn){
            mState = STATE.HUNTING;
        }
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
