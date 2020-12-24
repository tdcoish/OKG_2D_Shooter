/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_EN_PGrenade : MonoBehaviour
{
    private Rigidbody2D         cRigid;

    enum STATE{IN_AIR, LANDED}
    STATE                       mState;

    public Vector3              vDest;

    public float                _spdInAir = 8f;
    
    // I guess technically it needs to be thrown, hit an area, then change state to a countdown to explosion.
    public float                        _explosionTime = 2f;
    private float                       mLandTmStmp;

    public EX_PGrenade                  PF_Explosion;

    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        vDest = new Vector2();
        mState = STATE.IN_AIR;
    }

    // basically we're thrown to a spot, land, then explode.
    void Update()
    {
        switch(mState){
            case STATE.IN_AIR: RUN_InAir(); break;
            case STATE.LANDED: RUN_Landed(); break;
        }
    }

    void RUN_InAir()
    {
        Vector3 vDif = vDest - transform.position;
        cRigid.velocity = vDif.normalized * _spdInAir;

        // because we'll be too close or too far going frame by frame.
        if(Vector3.Distance(transform.position, vDest) < 0.5f){
            Debug.Log("Grenade Landed");
            cRigid.velocity = Vector3.zero;
            mLandTmStmp = Time.time;
            mState = STATE.LANDED;
        }else{
            Debug.Log("Distance: " + Vector3.Distance(transform.position, vDest));
        }
    }
    void RUN_Landed()
    {
        if(Time.time - mLandTmStmp > _explosionTime){
            // explode.
            Debug.Log("Exploded Grenade");
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
