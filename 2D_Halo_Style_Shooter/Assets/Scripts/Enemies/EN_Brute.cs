/*************************************************************************************
Okay, how does the Brute act? Well, it mostly tries to get close to the player. 
When really close, maybe then it charges? I guess that's sort of like the Hunters, except 
they can move slowly when they are firing, and they are always trying to get closer. They 
also fire plasma not cannon fire, and they don't hit nearly as hard.


They probably shouldn't be shielded, because them moving towards the player forces the player
to deal with them without the popping out of cover method.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Health
{
    public float                        _max;
    public float                        mAmt;
}

public class EN_Brute : MonoBehaviour
{
    public enum STATE{LONG_RANGE, CHASE, LEAP, RECOVER}
    public STATE                            mState;

    public Health                           mHealth;
    [HideInInspector]
    public Rigidbody2D                      cRigid;
    public PC_Cont                          rPC;
    [HideInInspector]
    public EN_PRifle                        cRifle;

    public GameObject                       gShotPoint;

    public float                            spdLong;           // speed we can move when firing weapon/long range
    public float                            _spdChase;
    public float                            _spdLeap;
    public float                            _enterFireDis;           // entering fire state or chase state.
    public float                            _enterChaseDis;
    public float                            _enterLeapDis;
    public float                            _leapTime;
    public float                            mLeapTmStmp;
    public float                            _recoverTime;
    public float                            mRecoverTmStmp;

    public Image                            IMG_HealthBar;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRifle = GetComponent<EN_PRifle>();
        cRigid = GetComponent<Rigidbody2D>();

        mHealth.mAmt = mHealth._max;
    }

    // Let's just get him to fire at the player to start.
    void Update()
    {
        IMG_HealthBar.fillAmount = mHealth.mAmt / mHealth._max;

        switch(mState)
        {
            case STATE.LONG_RANGE: RUN_LongRange(); break;
            case STATE.CHASE: RUN_Chase(); break;
            case STATE.LEAP: RUN_Leap(); break;
            case STATE.RECOVER: RUN_Recover(); break;
        }
    }

    // Try to move towards the player, try to fire at the player.
    void RUN_LongRange()
    {
        Vector3 vDif = rPC.transform.position - transform.position;
        cRigid.velocity = vDif.normalized * spdLong;

        cRifle.FAttemptFire(rPC, gShotPoint.transform.position);
        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);

        // Enter chase mode.
        if(Vector3.Distance(rPC.transform.position, transform.position) < _enterChaseDis){
            Debug.Log("Enter chase state");
            mState = STATE.CHASE;
        }
    }

    // When chasing they can't fire, but they will do a leap when in close.
    void RUN_Chase()
    {
        Vector3 vDif = rPC.transform.position - transform.position;
        cRigid.velocity = vDif.normalized * _spdChase;

        if(Vector3.Distance(rPC.transform.position, transform.position) < _enterLeapDis){
            Debug.Log("Leaping");
            mState = STATE.LEAP;
            mLeapTmStmp = Time.time;
            cRigid.velocity = vDif.normalized * _spdLeap;
        }
        // If too far, give up and go back to long range firing.
        if(Vector3.Distance(rPC.transform.position, transform.position) > _enterFireDis){
            Debug.Log("Too far, enter fire distance");
            mState = STATE.LONG_RANGE;
        }
    }

    void RUN_Leap()
    {
        if(Time.time - mLeapTmStmp > _leapTime){
            Debug.Log("Leapt, recovering for : " + _recoverTime);
            mState = STATE.RECOVER;
            mRecoverTmStmp = Time.time;
        }
    }

    // Take x seconds to recover.
    void RUN_Recover()
    {
        cRigid.velocity = Vector3.zero;
        if(Time.time - mRecoverTmStmp > _recoverTime){
            Debug.Log("Recovered.");
            mState = STATE.LONG_RANGE;
        }
    }

    void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        float realDamAmt = amt;
        if(type == DAMAGE_TYPE.PLASMA){
            realDamAmt *= 0.5f;
        }else if (type == DAMAGE_TYPE.BULLET){
            realDamAmt *= 2.0f;
        }else{
            Debug.Log("No damage type specified");
        }
        mHealth.mAmt -= realDamAmt;
        if(mHealth.mAmt <= 0f){
            Debug.Log("Should die now");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>())
        {
            FTakeDamage(5f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(5f, DAMAGE_TYPE.PLASMA);
        }
    }
}
