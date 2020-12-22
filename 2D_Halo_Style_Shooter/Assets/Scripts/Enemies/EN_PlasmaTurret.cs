/*************************************************************************************
Fire patterns. Includes state.

So basically our state is: Gun is not overheating == mass firing. Or, Gun needs to cool down == don't shoot.

Another way we could do this is adding another interval between bunches of fire.
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

    public float                            _fireInterval = 0.5f;
    public float                            mLastFire;
    public float                            _cooldownTime = 2f;
    public float                            mStartCooldownTimeStamp;
    public float                            mWepHeat = 0f;
    public float                            _heatAddedPerShot = 10f;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null){
            Debug.Log("No player character in scene");
        }
        mLastFire = Time.time;
    }

    void Update()
    {
        switch(mState)
        {
            case STATE.FIRING: RUN_FiringState(); break;
            case STATE.COOLING_DOWN: RUN_CooldownState(); break;
        }
    }

    void RUN_FiringState()
    {
        if(Time.time - mLastFire > _fireInterval){
            mLastFire = Time.time;

            PJ_EN_Plasmoid rPlasmoid = Instantiate(PF_Plasmoid, transform.position, transform.rotation);
            Vector3 vDir = rPC.transform.position - transform.position;
            vDir = Vector3.Normalize(vDir);
            rPlasmoid.cRigid.velocity = vDir * rPlasmoid._spd;

            mWepHeat += _heatAddedPerShot;
        }

        if(mWepHeat >= 100f)
        {
            mState = STATE.COOLING_DOWN;
            mStartCooldownTimeStamp = Time.time;
            Debug.Log("Weapon Overheated, cooling down");
        }
    }

    void RUN_CooldownState()
    {
        if(Time.time - mStartCooldownTimeStamp > _cooldownTime)
        {
            mState = STATE.FIRING;
            Debug.Log("Cooldown Over");
            mWepHeat = 0f;
        }
    }

}
