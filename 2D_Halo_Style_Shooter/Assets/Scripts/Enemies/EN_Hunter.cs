/*************************************************************************************
Wew. Gonna need a whole lot of stats here.
*************************************************************************************/
using UnityEngine;

public class EN_Hunter : MonoBehaviour
{
    enum STATE{LONG_RANGE, CLOSE_RANGE, LEAPING, RECOVER_FROM_LEAP}
    private STATE                   mState;

    private Rigidbody2D             cRigid;

    public float                    _disEnterLeapRange = 5f;
    public float                    _disEnterCloseRange = 10f;
    public float                    _disEnterLongRange = 15f;
    public float                    _chaseSpd = 3f;
    public float                    _leapSpd = 6f;
    public float                    _leapTime = 3f;
    private float                   mLeapTmStmp;
    public float                    _recoverTime = 1f;
    private float                   mRecoverTmStmp;

    public PC_Cont                  rPC;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch(mState){
            case STATE.CLOSE_RANGE: RUN_CloseRange(); break;
            case STATE.LONG_RANGE: RUN_LongRange(); break;
            case STATE.LEAPING: RUN_Leap(); break;
            case STATE.RECOVER_FROM_LEAP: RUN_RecoverFromLeap(); break;
        }
    }

    void RUN_LongRange(){
        // Shuffle around randomly? 
        cRigid.velocity = Vector2.zero;

        if(Vector3.Distance(rPC.transform.position, transform.position) < _disEnterCloseRange){
            Debug.Log("Enter Close Range");
            mState = STATE.CLOSE_RANGE;
        }
    }
    void RUN_CloseRange(){
        // this is more interesting. We chase after the player, then we charge at them.
        float disToPly = Vector3.Distance(rPC.transform.position, transform.position);

        if(disToPly > _disEnterLongRange){
            Debug.Log("Leave Close Range, going Long");
            mState = STATE.LONG_RANGE;
            return;
        }

        if(disToPly < _disEnterLeapRange){
            Debug.Log("Entering Leap");
            mState = STATE.LEAPING;
            mLeapTmStmp = Time.time;
            return;
        }

        // just run after the player.
        Vector3 vDir = rPC.transform.position - transform.position;
        cRigid.velocity = vDir.normalized * _chaseSpd;
    }
    void RUN_Leap()
    {
        // just assume for now we're going in the right direction.
        cRigid.velocity = cRigid.velocity.normalized * _leapSpd;

        if(Time.time - mLeapTmStmp > _leapTime){
            Debug.Log("Done leaping, recovering");
            mState = STATE.RECOVER_FROM_LEAP;
            mRecoverTmStmp = Time.time;
            return;
        }
    }
    void RUN_RecoverFromLeap()
    {
        cRigid.velocity = Vector3.zero;
        
        if(Time.time - mRecoverTmStmp > _recoverTime){
            Debug.Log("Done recovering, enter long range");
            mState = STATE.LONG_RANGE;
        }
    }
}
