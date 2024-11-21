/****************************************************************************************************
Intermediate refactoring step. I want to shove all the four guns into this one thing.

Will probably have a mini gun class, or perhaps struct, that all of them are composed with. 

It's time to add in the Beam Rifle. Use a line renderer, eventually with a shader. For now have it do
no damage. New idea. Instantly spawn a thing, draw a line to the gun, but the damage dealing thing is 
the orb, and it lasts for a brief while before attacking other things. 

Stashing here, having damage be steadily reduced past a certain point of heating, such as around 50%,
should make for interesting decisions. That way they are incentivized to let the gun cooldown more 
fully. Play with it. 

Since it could be irritating to have to constantly switch weapons per fire, a system where changing
weapons has some slight time cost seems appropriate. Perhaps each weapon has an active flag, where the 
first time you use it you have to charge it up, and then it starts firing much faster?

Also, possible that we use weapon select with middle mouse + mouse movement after all. Then we could have 
four weapons, plus grenades, or something else. Ugh. This is almost certainly the right idea. The problem
is that it's annoying to make. 

New system, only one gun active at any one time.

Eventually want to add AoE thing similar to Li Ming orb attack, that gets more powerful with distance.

I want to change the plasma rifle to fire in bursts of three/four, and also to increase in firing speed
the longer the player holds down the fire button. Also, maybe give it the Li Ming orb idea, where it 
does more damage, to a larger AoE, the further away it flies. 
****************************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class PC_Guns : MonoBehaviour
{
    PC_Cont                                 cPC;
    public enum STATE {READY, BURSTING, REC_BURST}
    public STATE                            mState;
    public float                            _fireInterval = 0.4f;
    public float                            mCurFireInterval;
    public float                            _minFireInterval = 0.1f;
    public float                            _shotSpeedIncRate = 1.5f;
    public float                            mFireTmStmp;
    public PJ_PC_Firebolt                   PJ_PRifle;
    public int                              _shotsPerSalvo = 3;
    public int                              mSalvoInd;
    public float                            _salvoRecTime = 0.5f;

    public float                            _shotRecoverySpeedMult = 0.8f;

    public List<AudioClip>                  rFireClips;
    public AudioSource                      mFirePlayer;

    public void F_Start()
    {
        cPC = GetComponent<PC_Cont>();
        mState = STATE.READY;
        mFireTmStmp = _fireInterval * -1f;
        mCurFireInterval = _fireInterval;
    }

    public void F_SetGunsToRecover()
    {
        mCurFireInterval = _fireInterval;
        mState = PC_Guns.STATE.REC_BURST;
        mSalvoInd = 0;
    }

    /********************************************************************************************
    When we click fire, or have the LMB already held down, we trigger another salvo of shots.
    ********************************************************************************************/
    public void F_CheckInputHandleFiring(Vector3 headingSpot, Vector3 shotPoint, bool LMBHeldDown)
    {
        if(mState == STATE.REC_BURST){
            if(Time.time - mFireTmStmp > _salvoRecTime){
                mState = STATE.READY;
                mSalvoInd = 0;
            }
        }

        if(mState == STATE.READY){
            if(LMBHeldDown){
                mState = STATE.BURSTING;
            }
        }

        if(mState != STATE.BURSTING) return;
        if(Time.time - mFireTmStmp < mCurFireInterval) return;

        headingSpot.z = 0f;
        Vector3 destination;
        if(!cPC.mHasActiveTarget || cPC.rCurTarget == null){
            destination = headingSpot;
        }else{
            destination = cPC.rCurTarget.transform.position;
        }
        Vector3 vDif = (destination - shotPoint).normalized;
        PJ_PC_Firebolt p = Instantiate(PJ_PRifle, shotPoint, transform.rotation);
        p.F_FireMe(vDif);
        System.Random rand = new System.Random();
        int clipInd = rand.Next(rFireClips.Count);
        mFirePlayer.clip = rFireClips[clipInd];
        mFirePlayer.Play();
        mFireTmStmp = Time.time;
        mSalvoInd++;
        if(mSalvoInd >= _shotsPerSalvo){
            mState = STATE.REC_BURST;
            mCurFireInterval /= _shotSpeedIncRate;
            if(mCurFireInterval < _minFireInterval) mCurFireInterval = _minFireInterval;
        }
        
        cPC.mState = PC_Cont.STATE.SHOT_RECOVERY;
    }

    /*
    public void F_SwitchWeapons(DT_Gun.TYPE newType)
    {
        DT_Gun.TYPE curActiveType = DT_Gun.TYPE.NA;
        for(int i=0; i<mGuns.Count; i++){
            if(mGuns[i].mActiveFlag){
                curActiveType = mGuns[i].mType;
            }
        }

        if(curActiveType == newType){
            Debug.Log("Tried to change to already equipped weapon");
            return;
        }
        for(int i=0; i<mGuns.Count; i++){
            mGuns[i].mActiveFlag = false;
            if(mGuns[i].mType == newType){
                mGuns[i].mActiveFlag = true;
            }
        }

        Debug.Log("Switched guns from: " + curActiveType + " to: " + newType);
    }
    */

    // Needler stuff. Probaly useless regardless.
    // public bool FAttemptFire(Vector3 msPos, Vector3 shotPoint)
    // {

    //     if(mState == STATE.READY){
    //         if(cPC.mCurEnergy < cPC._energyDrainPerNeedleFire){
    //             return false;
    //         }

    //         // Basically we cast around until we find an actor, and then lock on them.
    //         Actor actorToTarget;
    //         Actor[] actors = FindObjectsOfType<Actor>();
    //         if(actors.Length <= 1){
    //             actorToTarget = null;
    //         }else{
    //             float smallestDis = 10000f;
    //             int indClosest = -1;
    //             for(int i=0; i<actors.Length; i++){
    //                 if(actors[i].GetComponent<PC_Cont>()) continue;

    //                 float tempDis = Vector2.Distance(msPos, actors[i].transform.position);
    //                 if(tempDis < smallestDis){
    //                     smallestDis = tempDis;
    //                     indClosest = i;
    //                 }
    //             }

    //             actorToTarget = actors[indClosest];
    //         }

    //         Vector3 vDif = (msPos - shotPoint).normalized;
    //         PJ_PC_Needle n = Instantiate(PF_Needle, shotPoint, transform.rotation);
    //         n.mLifespan = _needleLifespan;
    //         n.mCreatedTimeStamp = Time.time;
    //         n.mTarget = actorToTarget;
    //         n.cRigid.velocity = vDif * _needleSpd;

    //         mFireTmStmp = Time.time;
    //         mState = STATE.UNREADY;
    //         return true;
    //     }
    //     return false;
    // }

}
