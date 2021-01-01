/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_FGren : PJ_Base
{    
    public GrenadeData                          mGrenD;
    public float                                _firstHopTime;
    public float                                mFirstLandTmStmp;
    public float                                _secondHopTime;
    public float                                mSecondLandTmStmp;

    public EX_PC_FGren                        PF_Explosion;
    
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mGrenD.vDest = new Vector2();
    }

    void Update()
    {
        switch(mGrenD.mState){
            case GrenadeData.STATE.IN_AIR: RUN_InAir(); break;
            case GrenadeData.STATE.FIRST_HOP: RUN_FirstHop(); break;
            case GrenadeData.STATE.SECOND_HOP: RUN_SecondHop(); break;
            case GrenadeData.STATE.LANDED: RUN_Landed(); break;
        }
    }

    void RUN_InAir()
    {
        Vector3 vDif = mGrenD.vDest - transform.position;
        cRigid.velocity = vDif.normalized * mGrenD._spdInAir;

        // because we'll be too close or too far going frame by frame.
        if(Vector3.Distance(transform.position, mGrenD.vDest) < 0.5f){
            mFirstLandTmStmp = Time.time;
            mGrenD.mState = GrenadeData.STATE.FIRST_HOP;
            cRigid.velocity *= 0.25f;
        }
    }

    void RUN_FirstHop()
    {
        if(Time.time - mFirstLandTmStmp > _firstHopTime){
            mSecondLandTmStmp = Time.time;
            cRigid.velocity *= 0.25f;
            mGrenD.mState = GrenadeData.STATE.SECOND_HOP;
        }
    }

    void RUN_SecondHop()
    {
        if(Time.time - mSecondLandTmStmp > _secondHopTime){
            mGrenD.mLandTmStmp = Time.time;
            cRigid.velocity = Vector3.zero;
            mGrenD.mState = GrenadeData.STATE.LANDED;
        }
    }

    // It's gonna do two hops just like a Halo Grenade.
    void RUN_Landed()
    {
        if(Time.time - mGrenD.mLandTmStmp > mGrenD._explosionTime){
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

}
