/*************************************************************************************
Wew. Gonna need a whole lot of stats here.
*************************************************************************************/
using UnityEngine;

public class EN_Hunter : MonoBehaviour
{
    public enum STATE{LONG_RANGE, CLOSE_RANGE, LEAPING, RECOVER_FROM_LEAP}
    public STATE                    mState;
    
    // for now sort of shuffle around at long range.
    public float                    _shuffleDirectionTime = 1.5f;
    public float                    mShuffleTmStmp;
    public float                    _shuffleSpd = 0.5f;
    public Vector2                  mShuffleDir;

    // Adding their firing as part of their character.
    public float                    _shotChargeTime = 3f;
    public float                    mChargeTmStmp;
    public PJ_EN_HunterBlast        PF_HunterBlast;

    public PC_Cont                  rPC;
    public float                        _maxHealth;
    public float                        mHealth;

    private Rigidbody2D             cRigid;

    public float                    _disEnterLeapRange = 5f;
    public float                    _disEnterCloseRange = 10f;
    public float                    _disEnterLongRange = 15f;
    public float                    _chaseSpd = 3f;
    public float                    _leapSpd = 6f;
    public float                    _leapTime = 3f;
    public float                    _leapDmg = 80f;
    private float                   mLeapTmStmp;
    public float                    _recoverTime = 1f;
    private float                   mRecoverTmStmp;

    public UI_EN                    gUI;


    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRigid = GetComponent<Rigidbody2D>();
        mHealth = _maxHealth;
    }

    void Update()
    {
        switch(mState){
            case STATE.CLOSE_RANGE: RUN_CloseRange(); break;
            case STATE.LONG_RANGE: RUN_LongRange(); break;
            case STATE.LEAPING: RUN_Leap(); break;
            case STATE.RECOVER_FROM_LEAP: RUN_RecoverFromLeap(); break;
        }

        gUI.FUpdateShieldHealthBars(mHealth, _maxHealth);

    }

    void RUN_LongRange(){
        // Pick a new direction every now and then to shuffle towards
        if(Time.time - mShuffleTmStmp > _shuffleDirectionTime){
            mShuffleDir = Random.insideUnitCircle.normalized;
            mShuffleTmStmp = Time.time;
        }
        cRigid.velocity = mShuffleDir * _shuffleSpd;

        // Also firing the projectile. Would need shot charge time to be set to Time.time upon entering shot charge state.
        if(Time.time - mChargeTmStmp > _shotChargeTime){
            PJ_EN_HunterBlast rHunterBlast = Instantiate(PF_HunterBlast, transform.position, transform.rotation);
            rHunterBlast.mDestination = rPC.transform.position;
            Vector3 vDir = rPC.transform.position - transform.position;
            vDir = Vector3.Normalize(vDir);
            rHunterBlast.cRigid.velocity = vDir * rHunterBlast.mProjD._spd;

            mChargeTmStmp = Time.time;
        }

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

    // deal with getting hit.
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Hit: " + col.gameObject);
        if(col.GetComponent<PJ_Base>()){
            PJ_Base proj = col.GetComponent<PJ_Base>();
            FTakeDamage(proj.mProjD._damage);
        }

        if(col.GetComponent<EN_Hunter>()){
            EN_Hunter h = col.GetComponent<EN_Hunter>();
            if(h.mState == EN_Hunter.STATE.LEAPING){
                FTakeDamage(_leapDmg);
                // cRigid.velocity = 
            }
        }
        if(col.GetComponent<EX_HBlast>()){
            EX_HBlast b = col.GetComponent<EX_HBlast>();
            FTakeDamage(b._damage);
        }
    }

    // For now, just say that plasma damage does 2x to shields, 1/2 to health, and vice versa for human weapon.
    public void FTakeDamage(float amt)
    {
        mHealth -= amt;
        if(mHealth < 0f) mHealth = 0f;
        // for now, just have the same modifier amounts, but in reverse.
        Debug.Log("Health Dam: " + amt);

        if(mHealth <= 0f){
            //Instantiate(PF_Particles, transform.position, transform.rotation);
            Debug.Log("Guess hunter died");
            Destroy(gameObject);
        }
    }

}
