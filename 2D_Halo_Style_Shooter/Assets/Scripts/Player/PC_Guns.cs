/****************************************************************************************************
Intermediate refactoring step. I want to shove all the four guns into this one thing.

Will probably have a mini gun class, or perhaps struct, that all of them are composed with. 
****************************************************************************************************/

using UnityEngine;

[System.Serializable]
public class DT_Gun
{
    public enum TYPE{PRIFLE, SHOTGUN, GRENADER, NEEDLER}
    public void F_BasicSetup()
    {
        mState = STATE.READY;
        mFireTmStmp = _fireInterval * -1f;
    }
    public enum STATE {READY, UNREADY}
    public STATE                    mState;
    public float                    _fireInterval = 1f;
    public float                    mFireTmStmp;
    public float                    _energyDrainPerFire = 10f;
    public TYPE                     mType;

    public void F_SelfUpdate()
    {
        if(mState == STATE.UNREADY){
            if(Time.time - mFireTmStmp > _fireInterval){
                mState = STATE.READY;
            }
        }
    }

    public bool F_CheckCanFire(float curEnergy, float energyNeeded)
    {
        if(mState != STATE.READY){
            return false;
        }
        if(curEnergy < energyNeeded){
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
    public float                            _needleTimeBeforeDetonation = 1f;
    public float                            _needleLifespan = 4f;
    public PJ_PC_Needle                     PJ_Needle;
    public DT_Gun                           mGrenader;
    public PJ_PC_Gren                       PJ_Grenade;

    public void F_Start()
    {
        cPC = GetComponent<PC_Cont>();
        mPRifle.F_BasicSetup();
        mShotgun.F_BasicSetup();
        mNeedler.F_BasicSetup();
        mGrenader.F_BasicSetup();
    }

    public void F_UpdateWeaponStates()
    {
        mPRifle.F_SelfUpdate();
        mShotgun.F_SelfUpdate();
        mNeedler.F_SelfUpdate();
        mGrenader.F_SelfUpdate();
    }

    public void F_CheckInputHandleFiring(Vector3 msPos, Vector3 shotPoint)
    {
        if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.Space)){
            // Fire shotgun.
            TryFiringWeapon(mShotgun);
        }
        if(Input.GetMouseButton(0) && !Input.GetKey(KeyCode.Space)){
            // Fire Prifle
            TryFiringWeapon(mPRifle);
        }
        if(Input.GetMouseButton(1) && Input.GetKey(KeyCode.Space)){
            // Fire needler
            TryFiringWeapon(mNeedler);
        }
        if(Input.GetMouseButton(1) && !Input.GetKey(KeyCode.Space)){
            // Fire grenade.
            TryFiringWeapon(mGrenader);
        }

        void TryFiringWeapon(DT_Gun gun)
        {
            if(gun.mType == DT_Gun.TYPE.NEEDLER){
                Debug.Log("Needler firing not really implemented yet.");
                return;
            }
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
            Vector3 vDif = (msPos - shotPoint).normalized;

            if(gun.F_CheckCanFire(cPC.mCurEnergy, gun._energyDrainPerFire)){
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
                    g.mLandingSpot = msPos;
                }

                gun.mFireTmStmp = Time.time;
                gun.mState = DT_Gun.STATE.UNREADY;
                cPC.mEnergyBroken = true;
                cPC.mCurEnergy -= gun._energyDrainPerFire;
                cPC.mLastEnergyUseTmStmp = Time.time;
            }
        }

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
