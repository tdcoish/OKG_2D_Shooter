/*************************************************************************************
Kind of a bad name for a basic concept. We need to be managing the weapon systems, mostly
because the various systems can interrupt the clip weapons reloading.

Either gun active at one time. + Can't fire for a sec after throwing grenade + same for melee
+ interrupt reload for clip if gren or melee or backpack.
*************************************************************************************/
using UnityEngine;


public class PC_AtkCtrl : MonoBehaviour
{
    public enum PC_WEAPONS{A_RIFLE, P_RIFLE}
    public PC_WEAPONS                       mActWep;
    // Handle player switched weapons.
    public enum STATE{REC_FROM_GREN_OR_MELEE, ABLE_TO_FIRE}
    public STATE                            mState;
    public bool                             mARifleActive = false;

    public PC_Gun                           cGun;
    public PC_PRifle                        cPRifle;
    public PC_Grenades                      cGrenThrower;
    public PC_Melee                         cMelee;

    public float                            mRecoverTime;
    public float                            mRecTmStmp;

    void Start()
    {
        cGun = GetComponent<PC_Gun>();
        cPRifle = GetComponent<PC_PRifle>();
        cGrenThrower = GetComponent<PC_Grenades>();
        cMelee = GetComponent<PC_Melee>();
    }

    public void FHandleMelee(float time)
    {

    }

    // After they throw a grenade or melee, we have recover time.
    public void FEnterRecover(float time)
    {

    }

    public void FCheckAndHandleWeaponSwitched()
    {
        if(Input.GetKeyDown(KeyCode.Tab)){
            if(mActWep == PC_WEAPONS.A_RIFLE){
                mActWep = PC_WEAPONS.P_RIFLE;
            }else if(mActWep == PC_WEAPONS.P_RIFLE){
                mActWep = PC_WEAPONS.A_RIFLE;
            }
        }
        SetActiveWeapon();
    }

    void SetActiveWeapon()
    {
        if(mActWep == PC_WEAPONS.A_RIFLE){
            cGun.mGunD.mIsActive = true;
            cPRifle.mGunD.mIsActive = false;
        }else{
            cGun.mGunD.mIsActive = false;
            cPRifle.mGunD.mIsActive = true;
        }
    }
}
