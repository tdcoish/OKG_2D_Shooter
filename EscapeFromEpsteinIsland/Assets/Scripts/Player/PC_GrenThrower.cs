/**************************************************************************************************
New system. You get a certain number of charges of holy water, let's say 2. These recharge on their 
own every five seconds. Each of them recharges independently.
**************************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class PC_GrenThrower : MonoBehaviour
{
    public PC_Cont                          cPC;
    public PJ_HolyWater                     PF_HolyWater;

    // public bool                             _dropAsMines = false;
    // public float                            mLastThrowTmStmp;
    public float                            _cooldownRate = 5f;
    public int                              _maxCharges = 1;
    public float[]                          mThrowTmStmps;
    public float                            mLastThrowTmStmp;
    public float                            _minDelayBetweenThrows = 0.5f;
    
    public List<AudioClip>                  rMineClips;
    public AudioSource                      mMinePlayer;

    public void F_Start()
    {
        mThrowTmStmps = new float[_maxCharges];
        for(int i=0; i<mThrowTmStmps.Length; i++){
            mThrowTmStmps[i] = Time.time - _cooldownRate;
        }
    }

    // Let the player throw Holy Water with Q/E, and MMB.
    public void FRunGrenadeLogic()
    {
        if(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Mouse2)){
            if(Time.time - mLastThrowTmStmp > _minDelayBetweenThrows){
                for(int i=0; i<_maxCharges; i++){
                    if(Time.time - mThrowTmStmps[i] > _cooldownRate){
                        PJ_HolyWater g = Instantiate(PF_HolyWater, cPC.gShotPoint.transform.position, transform.rotation);
                        g.FRunStart(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        // mLastThrowTmStmp = Time.time;
                        mThrowTmStmps[i] = Time.time;
                        Debug.Log("Fired grenade");

                        System.Random rand = new System.Random();
                        int clipInd = rand.Next(rMineClips.Count);
                        mMinePlayer.clip = rMineClips[clipInd];
                        mMinePlayer.Play();

                        mLastThrowTmStmp = Time.time;
                        break;
                    }
                }
            }
        }
    }


}
