/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct GrenadeData
{
    public enum STATE{IN_AIR, LANDED, FIRST_HOP, SECOND_HOP, READY_TO_EXPLODE}
    public STATE                        mState;
    public Vector3                      vDest;
    public float                        _spdInAir;
    // I guess technically it needs to be thrown, hit an area, then change state to a countdown to explosion.
    public float                        _explosionTime;
    public float                        mLandTmStmp;
}

public class PJ_EN_PGrenade : MonoBehaviour
{
    public GrenadeData                  mGrenD;
    public Rigidbody2D                  cRigid;

    public EX_PGrenade                  PF_Explosion;

    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mGrenD.vDest = new Vector2();
        mGrenD.mState = GrenadeData.STATE.IN_AIR;
    }

    // basically we're thrown to a spot, land, then explode.
    void Update()
    {
        switch(mGrenD.mState){
            case GrenadeData.STATE.IN_AIR: RUN_InAir(); break;
            case GrenadeData.STATE.LANDED: RUN_Landed(); break;
        }
    }

    void RUN_InAir()
    {
        // really annoying this.
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        
        Vector3 vDif = mGrenD.vDest - transform.position;
        cRigid.velocity = vDif.normalized * mGrenD._spdInAir;
        // Debug.Log("Grenade vel: " + cRigid.velocity);

        // because we'll be too close or too far going frame by frame.
        if(Vector3.Distance(transform.position, mGrenD.vDest) < 0.5f){
            FEnter_Landed();
        }else{
            // Debug.Log("Distance: " + Vector3.Distance(transform.position, mGrenD.vDest));
        }
    }

    // The PC can call this if we hit him.
    public void FEnter_Landed()
    {
        cRigid.velocity = Vector3.zero;
        mGrenD.mLandTmStmp = Time.time;
        mGrenD.mState = GrenadeData.STATE.LANDED;
    }
    void RUN_Landed()
    {
        if(Time.time - mGrenD.mLandTmStmp > mGrenD._explosionTime){
            // explode.
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
