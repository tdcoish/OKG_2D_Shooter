/*************************************************************************************
Need to spawn in a hitbox once we're finally slashing.

*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class PC_Melee : MonoBehaviour
{
    private PC_Cont                     cPC;
    private PC_Heading                  cHeading;

    public enum STATE{S_NOT_Meleeing, S_Windup, S_Slashing, S_Recover}
    public STATE                        mState;
    public float                        _windupTime = 0.25f;
    public float                        mWindTmStmp;
    public float                        _slashTime = 0.5f;
    public float                        mSlashTmStmp;
    public float                        _recoverTime = 0.25f;
    public float                        mRecTmStmp;
    public float                        _dam;
    public float                        _healAmtFromSuccessfulHit = 5f;
    public float                        _slashMoveSpdMult = 0.5f;

    private Rigidbody2D                 cRigid;

    public PC_SwordHitbox               rHitbox;
    public Collider2D                   rCollider;

    public List<AudioClip>                  rSlashClips;
    public AudioSource                      mSlashPlayer;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        cHeading = GetComponent<PC_Heading>();
        cRigid = GetComponent<Rigidbody2D>();
        rHitbox = GetComponentInChildren<PC_SwordHitbox>();
        rHitbox.gameObject.SetActive(false);
    }

    // MsPos already in world coordinates. 
    public void FStartMelee()
    {
        mState = STATE.S_Windup;
        mWindTmStmp = Time.time;
    }

    public void FRUN_Melee()
    {
        // New version we can still move while meleeing.
        cRigid.velocity = cPC.HandleInputForVel();

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
        if(Time.time - mSlashTmStmp > _slashTime){
            mState = STATE.S_Recover;
            mRecTmStmp = Time.time;
            rHitbox.gameObject.SetActive(false);
        }
    }

    void RUN_Recover()
    {
        if(Time.time - mRecTmStmp > _recoverTime){
            mState = STATE.S_NOT_Meleeing;
        }
    }

}
