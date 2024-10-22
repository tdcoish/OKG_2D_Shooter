using UnityEngine;

public class EN_Shielder : EN_Base
{
    // public uint                         kFleeing = 1<<2;
    public uint                         kMovingToShield = 1<<3;
    public uint                         kShielding = 1<<4; 

    public float                        _shieldGift = 100f;
    public float                        _shieldCastTime = 0.5f;
    public float                        mShieldCastStartTmStmp;
    public float                        _shieldCastDistance = 0.5f;
    public EN_Base                      rTarget;

    public Sprite                       rMoving;
    public Sprite                       rHitstun;
    public Sprite                       rCasting;

    public override void F_CharSpecStart()
    {
        kState = kMovingToShield;
        mShieldCastStartTmStmp = Time.time - _shieldCastTime;
    }
    public override void F_CharSpecUpdate()
    {
        if(rOverseer.rPC == null) return;

        if(kState == kMovingToShield){
            FRUN_MovingToShield();
        }else if(kState == kShielding){
            FRUN_CastingShield();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }

        F_Animate();
    }

    void F_EnterShieldCasting(EN_Base closest)
    {
        cRigid.velocity = Vector2.zero;
        rTarget = closest;
        kState = kShielding;
        mShieldCastStartTmStmp = Time.time;
    }

    void FRUN_CastingShield()
    {
        if(rTarget == null){
            kState = kMovingToShield;
            return;
        }

        if(Time.time - mShieldCastStartTmStmp > _shieldCastTime){
            A_HealthShields a = rTarget.GetComponent<A_HealthShields>();
            a.mShields.mStrength = _shieldGift;
            if(a.mShields._max < _shieldGift){
                a.mShields._max = _shieldGift;
            }
            kState = kMovingToShield;
        }
    }

    void FRUN_MovingToShield()
    {
        // Basically, find the closest enemy that is unshielded.
        EN_Base FindClosestUnshieldedEnemy()
        {
            EN_Base closest = null;
            float nearest = 10000f;
            for(int i=0; i<rOverseer.rActors.Count; i++){
                if(rOverseer.rActors[i] == this) continue;
                if(rOverseer.rActors[i].GetComponent<PC_Cont>()) continue;
                if(rOverseer.rActors[i].GetComponent<EN_Base>() == null) continue;

                EN_Base b = rOverseer.rActors[i].GetComponent<EN_Base>();
                A_HealthShields a = b.GetComponent<A_HealthShields>();
                if(a.mShields.mStrength <= 0f){
                    float tempDis = Vector3.Distance(rOverseer.rActors[i].transform.position, transform.position);
                    if(tempDis < nearest){
                        nearest = tempDis;
                        closest = b;
                    }
                }
            }
            return closest;
        }

        EN_Base closest = FindClosestUnshieldedEnemy();
        if(closest == null){
            cRigid.velocity = Vector2.zero;
            return;
        }

        if(Vector2.Distance(closest.transform.position, transform.position) < _shieldCastDistance){
            F_EnterShieldCasting(closest);
            return;
        }

        LayerMask mask = LayerMask.GetMask("ENEMIES"); mask |= LayerMask.GetMask("ENV_Obj");
        // Yes, "CanRayTraceDirectlyToPlayer should be renamed, since we pass in the layermask."
        if(rOverseer.GetComponent<MAN_Helper>().FCanRaytraceDirectlyToEnemy(closest.transform.position, transform.position, mask)){
            // Just move directly to them.
            // Debug.Log("Can see directly");
            cRigid.velocity = _spd * (closest.transform.position - transform.position).normalized;
        }else{
            // Debug.Log("Something blocking");
            Vector2Int startNode = rOverseer.cPather.FFindClosestValidTile(transform.position);
            Vector2Int endNode = rOverseer.cPather.FFindClosestValidTile(closest.transform.position);
            mPath = rOverseer.cPather.FCalcPath(startNode, endNode);
            if(mPath != null){
                mPath.RemoveAt(0);      // ! is this correct?
                Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
                cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;
            }else{
                Debug.Log("Shielder path null.");
            }
        }
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kMovingToShield;
    }
    
    public void F_Animate()
    {
        EN_Shielder cShielder = GetComponent<EN_Shielder>();
        if(cShielder == null) return;
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cShielder.kState == cShielder.kPoiseBroke){
            sRender.sprite = rHitstun;
        }else if(cShielder.kState == cShielder.kMovingToShield){
            sRender.sprite = rMoving;
        }else if(cShielder.kState == cShielder.kShielding){
            sRender.sprite = rCasting;
        }else{
            Debug.Log("State not covered");
        }
    }

}
