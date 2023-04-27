using UnityEngine;

public class PJ_PC_Gren : MonoBehaviour
{
    public enum STATE{IN_AIR, LANDED}
    public STATE                    mState;

    public float                    _spd = 5f;
    public float                    _explosionTime = 2f;
    float                           mLandTmStmp;
    public Vector2                  mLandingSpot;
    public Rigidbody2D              cRigid;
    public EX_PGrenade              PF_Explosion;

    void Update()
    {
        switch(mState)
        {
            case STATE.IN_AIR: Run_InAir(); break;
            case STATE.LANDED: Run_Landed(); break;
        }
    }

    void Run_InAir()
    {
        if(Vector2.Distance(transform.position, mLandingSpot) < 0.2f){
            mState = STATE.LANDED;
            mLandTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
        }
    }

    void Run_Landed()
    {
        if(Time.time - mLandTmStmp > _explosionTime){
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
    
    
}
