/*************************************************************************************
Fire patterns. Includes state.

So basically our state is: Gun is not overheating == mass firing. Or, Gun needs to cool down == don't shoot.

Another way we could do this is adding another interval between bunches of fire.

This shit shows why OOP is such a fucking stupid idea. I want to write a clear function like:

public GunData ShootGun()
Which does all the little bullshit about Instantiating the variables, etcetera, but in order 
to do this, I need to first create a WP_Base class which uses methods to sneakily change the state 
of the data in hidden ways. Or I need to create a WP_Methods class which won't have side effects, but 
which is this annying little absurdity which works around OOP being a failed idea.
*************************************************************************************/
using UnityEngine;

public class EN_PlasmaTurret : MonoBehaviour
{
    private enum STATE{
        FIRING,
        COOLING_DOWN
    }
    private STATE                           mState = STATE.FIRING;

    private PC_Cont                         rPC;

    public PJ_EN_Plasmoid                   PF_Plasmoid;

    public GunData                          mGunD;
    public PlasmaGunData                    mPlasmaD;

    public Health                           mHealth;

    public UI_EN                            gUI;

    public GameObject                       PF_Particles;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null){
            Debug.Log("No player character in scene");
        }
        mGunD.mLastFireTmStmp = Time.time;
    }

    void Update()
    {
        switch(mState)
        {
            case STATE.FIRING: RUN_FiringState(); break;
            case STATE.COOLING_DOWN: RUN_CooldownState(); break;
        }

        gUI.FUpdateShieldHealthBars(mHealth.mAmt, mHealth._max);

        if(mHealth.mAmt <= 0f){
            KillOurselves();
        }
    }

    void RUN_FiringState()
    {
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval){
            mGunD.mLastFireTmStmp = Time.time;

            PJ_EN_Plasmoid rPlasmoid = Instantiate(PF_Plasmoid, transform.position, transform.rotation);
            Vector3 vDir = rPC.transform.position - transform.position;
            vDir = Vector3.Normalize(vDir);
            rPlasmoid.cRigid.velocity = vDir * rPlasmoid.mProjD._spd;
            rPlasmoid.mProjD.rOwner = gameObject;

            mPlasmaD.mHeat += mPlasmaD._heatPerShot;
        }

        if(mPlasmaD.mHeat >= 100f)
        {
            mState = STATE.COOLING_DOWN;
            mPlasmaD.mOverheatTmStmp = Time.time;
            Debug.Log("Weapon Overheated, cooling down");
        }
    }

    void RUN_CooldownState()
    {
        mPlasmaD.mHeat -= mPlasmaD._cooldownRate * Time.deltaTime;

        if(Time.time - mPlasmaD.mOverheatTmStmp > mPlasmaD._cooldownTime)
        {
            mState = STATE.FIRING;
            Debug.Log("Cooldown Over");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("hit somethign");
        if(col.GetComponent<PJ_Base>())
        {
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    Debug.Log("Hit ourselves.");
                    return;
                }
            }
            // take damage. No shields.
            mHealth.mAmt -= p.mProjD._damage;
            Debug.Log("Took: " + p.mProjD._damage + " damage");

            if(mHealth.mAmt <= 0f){
                Debug.Log("Dead");
                KillOurselves();
            }

            p.FDeath();
        }
    }

    void KillOurselves()
    {
        Debug.Log("Kill myself");
        // need a reference to the level manager, audio system, etcetera.
        LVL_Man p = FindObjectOfType<LVL_Man>();
        if(p != null)
        {
            p.FHandleEnemyKilled(gameObject);
        }
        Instantiate(PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }

}
