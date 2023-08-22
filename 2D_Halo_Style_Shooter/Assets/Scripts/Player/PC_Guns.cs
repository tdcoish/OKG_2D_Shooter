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
****************************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DT_Gun
{
    public enum TYPE{NA, PRIFLE, SHOTGUN, GRENADER, NEEDLER, BEAM_RIFLE}
    public void F_BasicSetup()
    {
        mState = STATE.READY;
        mFireTmStmp = _fireInterval * -1f;
    }
    public enum STATE {READY, FIRING, UNREADY}
    public STATE                    mState;
    public TYPE                     mType;
    public float                    _fireInterval = 1f;
    public float                    mFireTmStmp;
    public float                    _maxHeating = 100f;
    public float                    mCurHeating;
    public float                    _heatingPerFire;
    public bool                     mOverheated = false;
    public float                    _overheatDoneAmt = 50f;
    public bool                     mActiveFlag = false;

    public void F_SelfUpdate()
    {
        if(mState == STATE.UNREADY){
            if(Time.time - mFireTmStmp > _fireInterval){
                mState = STATE.READY;
            }
        }
    }

    public bool F_CheckCanFire()
    {
        if(mState != STATE.READY){
            return false;
        }
        if(mOverheated){
            return false;
        }
        return true;
    }    
}

public class PC_Guns : MonoBehaviour
{
    PC_Cont                                 cPC;
    public DT_Gun                           mPRifle;
    public PJ_PC_Firebolt                   PJ_PRifle;
    public DT_Gun                           mShotgun;
    public int                              _pelletsFiredPerBlast = 5;
    public float                            _spread = 30f;
    public float                            _pelletInitialSpeed = 5f;
    public float                            _pelletLifetime = 1.5f;
    public PJ_PC_ShotgunPellet              PJ_Pellet;
    public DT_Gun                           mNeedler;
    public float                            _needleSpd = 1.5f;
    public float                            _needleTurnRate = 30f;
    public float                            _needleTimeDet = 1f;
    public float                            _needleLifespan = 4f;
    public PJ_PC_Needle                     PJ_Needle;
    public DT_Gun                           mGrenader;
    public PJ_PC_Gren                       PJ_Grenade;
    // Beam rifle needs to have a certain tick rate of damage dealt. Or maybe it happens every frame?
    public DT_Gun                           mBeamRifle;
    public LineRenderer                     cBeamRifleRender;
    public PJ_PC_BeamRifle                  PJ_BeamRifle;

    public List<DT_Gun>                     mGuns;

    public void F_CooldownWeaponsAndUpdateState(float amt)
    {
        void CooldownWeapon(DT_Gun gun, float amt)
        {
            gun.mCurHeating -= amt; 
            if(gun.mCurHeating < 0f) gun.mCurHeating = 0f;

            if(gun.mOverheated){
                if(gun.mCurHeating <= gun._overheatDoneAmt){
                    gun.mOverheated = false;
                }
            }   

            if(gun.mState == DT_Gun.STATE.UNREADY){
                if(Time.time - gun.mFireTmStmp > gun._fireInterval){
                    gun.mState = DT_Gun.STATE.READY;
                }
            }
        }
        CooldownWeapon(mPRifle, amt);
        CooldownWeapon(mGrenader, amt);
        CooldownWeapon(mBeamRifle, amt);
        CooldownWeapon(mShotgun, amt);
        CooldownWeapon(mNeedler, amt);
    }

    public void F_Start()
    {
        cPC = GetComponent<PC_Cont>();
        mGuns = new List<DT_Gun>();
        mGuns.Add(mPRifle); mGuns.Add(mShotgun); mGuns.Add(mGrenader); mGuns.Add(mNeedler); mGuns.Add(mBeamRifle);
        for(int i=0; i<mGuns.Count; i++){
            mGuns[i].F_BasicSetup();
        }
    }

    public void F_UpdateWeaponStates()
    {
        mPRifle.F_SelfUpdate();
        mShotgun.F_SelfUpdate();
        mNeedler.F_SelfUpdate();
        mGrenader.F_SelfUpdate();
        mBeamRifle.F_SelfUpdate();
    }

    public void F_CheckInputHandleFiring(Vector3 msPos, Vector3 shotPoint)
    {
        // New plan is that they fire the active weapon with LMB. RMB is probably grenades, maybe mines.
        if(Input.GetMouseButton(0)){
            for(int i=0; i<mGuns.Count; i++){
                if(mGuns[i].mActiveFlag){
                    TryFiringWeapon(mGuns[i]);
                    return;
                }
            }
            Debug.Log("Worrying, no active gun.");
        }

        void TryFiringWeapon(DT_Gun gun)
        {
            // ShootPellet only useful for the shotgun.
            void ShootPellet(Vector3 vDir)
            {
                PJ_PC_ShotgunPellet p = Instantiate(PJ_Pellet, shotPoint, transform.rotation);
                p.cRigid.velocity = vDir * _pelletInitialSpeed;
                p.mStartSpd = _pelletInitialSpeed;
                p.vDir = vDir.normalized;
                p.mCreatedTimeStamp = Time.time;
                p.mLifespan = _pelletLifetime;
            }

            msPos.z = 0f;
            Vector3 destination;
            if(!cPC.mHasActiveTarget || cPC.rCurTarget == null){
                destination = msPos;
            }else{
                destination = cPC.rCurTarget.transform.position;
            }
            Vector3 vDif = (destination - shotPoint).normalized;

            if(gun.F_CheckCanFire()){
                if(gun.mType == DT_Gun.TYPE.SHOTGUN){
                    float step = _spread / (float)(_pelletsFiredPerBlast-1);
                    float rightMost = _spread / 2f * -1f;
                    for(int i=0; i<_pelletsFiredPerBlast; i++){
                        ShootPellet(Quaternion.Euler(0f,0f, rightMost + (step * i)) * vDif);
                    }
                }else if(gun.mType == DT_Gun.TYPE.PRIFLE){
                    PJ_PC_Firebolt p = Instantiate(PJ_PRifle, shotPoint, transform.rotation);
                    p.cRigid.velocity = vDif * p._spd;
                }else if(gun.mType == DT_Gun.TYPE.GRENADER){
                    PJ_PC_Gren g = Instantiate(PJ_Grenade, shotPoint, transform.rotation);
                    g.cRigid.velocity = vDif * g._spd;
                    g.mState = PJ_PC_Gren.STATE.IN_AIR;
                    g.mLandingSpot = destination;
                }else if(gun.mType == DT_Gun.TYPE.BEAM_RIFLE){
                    Debug.Log("Shot beam rifle");
                    // Eventually have to raycast to see what we actually hit.
                    PJ_PC_BeamRifle b = Instantiate(PJ_BeamRifle, destination, transform.rotation);
                    b.mLifespan = mBeamRifle._fireInterval; b.mCreatedTimeStamp = Time.time;
                    b.rShooter = GetComponent<Actor>();
                }else if(gun.mType == DT_Gun.TYPE.NEEDLER){
                    PJ_PC_Needle n = Instantiate(PJ_Needle, shotPoint, transform.rotation);
                    Debug.Log("Needle created");
                    n._spd = _needleSpd; n._timeBeforeDetonation = _needleTimeDet;
                    n._turnRate = _needleTurnRate; n._lifespan = _needleLifespan;
                    n.mCreatedTimeStamp = Time.time;
                    n.GetComponent<Rigidbody2D>().velocity = vDif * n._spd;
                    if(cPC.rCurTarget == null){
                        n.rTarget = null;
                    }else{
                        n.rTarget = cPC.rCurTarget;
                    }
                }

                gun.mFireTmStmp = Time.time;
                gun.mState = DT_Gun.STATE.UNREADY;
                gun.mCurHeating += gun._heatingPerFire; 
                if(gun.mCurHeating >= gun._maxHeating) gun.mOverheated = true;
                cPC.mLastEnergyUseTmStmp = Time.time;
            }
        }

    }

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
