/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_FGren : MonoBehaviour
{    
    public Rigidbody2D                          cRigid;

    public bool                                 _useBounce = false;
    public GrenadeData                          mGrenD;
    public float                                _firstHopTime;
    public float                                mFirstLandTmStmp;
    public float                                _secondHopTime;
    public float                                mSecondLandTmStmp;
    public EX_PC_FGren                          PF_Explosion;

    public bool                                 mProperlyInstantiated = false;
    
    public void FRunStart(Vector3 dest)
    {
        mProperlyInstantiated = true;
        mGrenD.vDest = new Vector2();
        mGrenD.vDest = dest; mGrenD.vDest.z = 0f;
    }

    void Update()
    {
        if(!mProperlyInstantiated) return;

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
        if(_useBounce){
            if(Vector3.Distance(transform.position, mGrenD.vDest) < 0.5f){
                mFirstLandTmStmp = Time.time;
                mGrenD.mState = GrenadeData.STATE.FIRST_HOP;
                cRigid.velocity *= 0.25f;
            }
        }else{
            if((Vector3.Distance(transform.position, mGrenD.vDest) < 0.1f)){
                FEnter_Landed();
            }
        }
            
    }

    void RUN_FirstHop()
    {
        Debug.Log("First ho");
        if(Time.time - mFirstLandTmStmp > _firstHopTime){
            mSecondLandTmStmp = Time.time;
            cRigid.velocity *= 0.25f;
            mGrenD.mState = GrenadeData.STATE.SECOND_HOP;
        }
    }

    void RUN_SecondHop()
    {
        Debug.Log("Second hop");
        if(Time.time - mSecondLandTmStmp > _secondHopTime){
            FEnter_Landed();
        }
    }

    // Can be called by other components.
    public void FEnter_Landed()
    {
        Debug.Log("Enter landed");
        mGrenD.mLandTmStmp = Time.time;
        cRigid.velocity = Vector3.zero;
        mGrenD.mState = GrenadeData.STATE.LANDED;
    }
    // It's gonna do two hops just like a Halo Grenade.
    void RUN_Landed()
    {
        Debug.Log("Landed.");
        if(Time.time - mGrenD.mLandTmStmp > mGrenD._explosionTime){
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

}
