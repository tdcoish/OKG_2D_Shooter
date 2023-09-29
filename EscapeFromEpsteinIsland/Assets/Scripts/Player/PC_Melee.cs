/*************************************************************************************
Need to spawn in a hitbox once we're finally slashing.

*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

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
    public float                        _healAmtFromSuccessfulHit = 5f;

    public Vector2                      mMeleeDir;

    private Rigidbody2D                 cRigid;

    public PC_SwordHitbox               rHitbox;

    public List<AudioClip>                  rSlashClips;
    public AudioSource                      mSlashPlayer;

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

            System.Random rand = new System.Random();
            int clipInd = rand.Next(rSlashClips.Count);
            mSlashPlayer.clip = rSlashClips[clipInd];
            mSlashPlayer.Play();
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
