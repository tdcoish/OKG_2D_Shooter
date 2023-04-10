using UnityEngine;
using System.Collections.Generic;

public class EN_Beamer : Actor
{
    public enum STATE{LOOKING_FOR_VANTAGE, CHARGING, COOLDOWN}
    public STATE                        mState;
    public float                        _chargeTime = 4f;
    public float                        mChargeTmStmp;
    public float                        _cooldownTime = 1f;
    public float                        mCooldownTmStmp;
    public float                        _spd = 2f;
    public float                        _damage = 40f;
    public float                        _turnRateDegrees = 30f;
    Rigidbody2D                         cRigid;
    EN_BeamerAnimator                   cAnim;
    LineRenderer                        cLineRender;
    public DIRECTION                    mHeading;
    public Vector2                      mTrueHeading;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mState = STATE.LOOKING_FOR_VANTAGE;
        cAnim = GetComponent<EN_BeamerAnimator>();
        cLineRender = GetComponent<LineRenderer>();
        mTrueHeading = Vector2.up;
    }

    public override void RUN_Update()
    {
        // Move to player.
        // Actually for now don't bother making this one move. 
        switch(mState){
            case STATE.LOOKING_FOR_VANTAGE: RUN_Vantage(); break;
            case STATE.CHARGING: RUN_Charging(); break;
            case STATE.COOLDOWN: RUN_Cooldown(); break;
        }

        cAnim.FAnimate();
    }

    void RUN_Vantage()
    {
        // Eventually make him run to a vantage spot. For now it's just stationary.
        mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(mTrueHeading);

        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        // If we can see the player, immediately go to charging.
        if(hit.collider.GetComponent<PC_Cont>()){
            mState = STATE.CHARGING; 
            mChargeTmStmp = Time.time;
            return;
        }
    }
    void RUN_Charging()
    {
        // If can't see the player, go back to hunting.
        Vector2 vDirToPlayer = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDirToPlayer.normalized, 1000f, mask);
        // If we can see the player, immediately go to charging.
        if(!hit.collider.GetComponent<PC_Cont>()){
            mState = STATE.LOOKING_FOR_VANTAGE; 
            cLineRender.enabled = false;
            return;
        }

        // Rotate towards the player at a certain fixed rate. 
        Vector2 vGoal = (rOverseer.rPC.transform.position - transform.position).normalized; 
        Vector2 newHeading = Vector3.RotateTowards(mTrueHeading, vGoal, Mathf.Deg2Rad *_turnRateDegrees * Time.deltaTime, 0.0f);
        mTrueHeading = newHeading.normalized;
        mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(mTrueHeading);

        // The line should render at huge distances, but be along the path that we are looking.
        cLineRender.enabled = true;
        List<Vector3> points = new List<Vector3>();
        points.Add(transform.position);
        float dis = Vector2.Distance(transform.position, rOverseer.rPC.transform.position);
        Vector2 endpoint = (Vector2)transform.position + (mTrueHeading * dis);
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
            mState = STATE.COOLDOWN;
            mCooldownTmStmp = Time.time;

            // If we hit the player, make them take damage.
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, mTrueHeading.normalized, 1000f, mask);
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
            mState = STATE.LOOKING_FOR_VANTAGE;
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
