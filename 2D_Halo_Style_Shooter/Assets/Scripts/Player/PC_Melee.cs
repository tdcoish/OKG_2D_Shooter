/*************************************************************************************
Need to spawn in a hitbox once we're finally slashing.

*************************************************************************************/
using UnityEngine;

public class PC_Melee : MonoBehaviour
{
    private PC_Cont                     cPC;

    public enum STATE{S_NOT_Meleeing, S_Windup, S_Slashing, S_Recover}
    public STATE                        mState;
    public float                        _windupTime = 0.25f;
    public float                        mWindTmStmp;
    public float                        _slashingMoveSpd = 1f;
    public float                        _slashTime = 0.5f;
    public float                        mSlashTmStmp;
    public float                        _recoverTime = 0.25f;
    public float                        mRecTmStmp;
    public float                        _dam;

    public Vector2                      mMeleeDir;

    private Rigidbody2D                 cRigid;

    public PC_SwordHitbox               rHitbox;


    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        cRigid = GetComponent<Rigidbody2D>();
        rHitbox = GetComponentInChildren<PC_SwordHitbox>();
        rHitbox.gameObject.SetActive(false);
    }

    // MsPos already in world coordinates. 
    public void FStartMelee(Vector2 msPos)
    {
        Debug.Log("Melee started");
        Vector2 dir = msPos - (Vector2)transform.position;
        mMeleeDir = dir.normalized;

        mState = STATE.S_Windup;
        mWindTmStmp = Time.time;
        cRigid.velocity = Vector2.zero;
    }

    public void FRUN_Melee()
    {
        switch(mState)
        {
            case STATE.S_Windup: RUN_Windup(); break;
            case STATE.S_Slashing: RUN_Slashing(); break;
            case STATE.S_Recover: RUN_Recover(); break;
        }
    }

    void RUN_Windup()
    {
        if(Time.time - mWindTmStmp > _windupTime){
            Debug.Log("All wound up, striking");
            mState = STATE.S_Slashing;
            mSlashTmStmp = Time.time;
            
            rHitbox.gameObject.SetActive(true);
            Vector2 offset = new Vector2();
            switch(cPC.mHeading){
                case DIRECTION.DOWN: offset = new Vector2(0f, -1f); break;
                case DIRECTION.DOWNLEFT: offset = new Vector2(-1f, -1f).normalized; break;
                case DIRECTION.DOWNRIGHT: offset = new Vector2(1f, -1f).normalized; break;
                case DIRECTION.RIGHT: offset = new Vector2(1f, 0f); break;
                case DIRECTION.LEFT: offset = new Vector2(-1f, 0f); break;
                case DIRECTION.UPRIGHT: offset = new Vector2(1f, 1f).normalized; break;
                case DIRECTION.UPLEFT: offset = new Vector2(-1f, 1f).normalized; break;
                case DIRECTION.UP: offset = new Vector2(0f, 1f); break;
            }
            rHitbox.transform.position = (Vector2)transform.position + offset;
        }
    }

    void RUN_Slashing()
    {
        cRigid.velocity = mMeleeDir.normalized * _slashingMoveSpd;

        if(Time.time - mSlashTmStmp > _slashTime){
            Debug.Log("Slashed. Moving to recover.");
            mState = STATE.S_Recover;
            mRecTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            rHitbox.gameObject.SetActive(false);
        }
    }

    void RUN_Recover()
    {
        if(Time.time - mRecTmStmp > _recoverTime){
            Debug.Log("All recovered. Can slash again");
            mState = STATE.S_NOT_Meleeing;
        }
    }

}
