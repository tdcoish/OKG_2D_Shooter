/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Cont : Actor
{
    // Currently used mostly for animation. Subject to change.
    public enum STATE {IDLE, RUNNING, WINDUP, SLASHING, BATTACK_RECOVERY}
    public STATE                            mState;

    private Rigidbody2D                     cRigid;
    // private PC_Gun                          cGun;
    // private PC_Grnd                         cGrnd;
    // private PC_Gun                          cGun;
    // private PC_PRifle                       cPRifle;
    private PC_Grenades                     cGren;
    private A_HealthShields                 cHpShlds;
    private PC_Melee                        cMelee;
    private PC_FireboltSpell                cFireSpell;
    private PC_AnimDebug                    cAnim;

    public GameObject                       gShotPoint;
    public GameObject                       PF_Particles;

    public float                            _spd;
    public float                            _spdFwdMult = 1f;
    public float                            _spdBckMult = 0.5f;
    public float                            _spdSideMult = 0.7f;
    public bool                             mMoving = false;
    
    public UI_PC                            rUI;

    public bool                             _debugInvinsible = false;
    public float                            _tempInvinsibleTime = 0.1f;
    public bool                             mTempInvinsible = false;
    public float                            mTempInvinsibleTmStmp;

    public bool                             mFlyingAfterDamage = false;
    public float                            _flyingTime = 0.5f; // should change depending on what hit us.
    public float                            mFlyingTimeStmp;

    public bool                             mMeleeMode = true;

    public DIRECTION                        mHeading;
    public MAN_Helper                       rHelper;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        cGren = GetComponent<PC_Grenades>();
        cHpShlds = GetComponent<A_HealthShields>();
        cMelee = GetComponent<PC_Melee>();
        cAnim = GetComponent<PC_AnimDebug>();
        cFireSpell = GetComponent<PC_FireboltSpell>();
        cAnim.RUN_Start();

        rUI = FindObjectOfType<UI_PC>();
        if(rUI == null){
            Debug.Log("No PC User Interface Found");
        }
        rHelper = FindObjectOfType<MAN_Helper>();
        if(rHelper == null){
            Debug.Log("No helper in scene");
        }

        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = 75f;
        cHpShlds.mShields.mState = Shields.STATE.BROKEN;

        mState = STATE.IDLE;
    }

    public override void RUN_Update()
    {
        switch(mState)
        {
            case STATE.IDLE: RUN_IdleAndMoving(); break;
            case STATE.RUNNING: RUN_IdleAndMoving(); break;
            case STATE.WINDUP: RUN_WindupAndSlashingAndBAtkRec(); break;
            case STATE.SLASHING: RUN_WindupAndSlashingAndBAtkRec(); break;
            case STATE.BATTACK_RECOVERY: RUN_WindupAndSlashingAndBAtkRec(); break;
        }

        if(mTempInvinsible){
            if(Time.time - mTempInvinsibleTmStmp > _tempInvinsibleTime){
                mTempInvinsible = false;
            }
        }
        if(mFlyingAfterDamage){
            if(Time.time - mFlyingTimeStmp > _flyingTime){
                mFlyingAfterDamage = false;
            }
        }

        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);

        if(rUI != null){
            rUI.FillShieldAmount(cHpShlds.mShields.mStrength, cHpShlds.mShields._max);
            rUI.FillHealthAmount(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max);
        }
        cAnim.FRUN_Animation();

        // They should switch modes immediately, but not necessarily switch states, such as if they are hitstunned.
        if(Input.GetKeyDown(KeyCode.Tab)){
            mMeleeMode = !mMeleeMode;
        }
    }

    public void RUN_IdleAndMoving()
    {
        cFireSpell.FRunFireSpellUpdate();

        //RotateToMouse();
        cRigid.velocity = HandleInputForVel();

        if(cRigid.velocity != Vector2.zero){
            mState = STATE.RUNNING;
        }else{
            mState = STATE.IDLE;
        }
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vDir = msPos - (Vector2)transform.position;
        mHeading = rHelper.FGetCardinalDirection(vDir.normalized);

        if(Input.GetMouseButton(0)){
            if(mMeleeMode){
                ENTER_WindupForSlash();
            }else{
                cFireSpell.FAttemptFire(msPos, gShotPoint.transform.position);
            }
        }

    }

    public void ENTER_WindupForSlash()
    {
        Vector2 msPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cMelee.FStartMelee(msPos);
        Vector2 vDir = msPos - (Vector2)transform.position;
        mHeading = rHelper.FGetCardinalDirection(vDir.normalized);
        mState = STATE.WINDUP;
        Debug.Log("Started slash windup");
    }
    public void RUN_WindupAndSlashingAndBAtkRec()
    {
        cMelee.FRUN_Melee();
        if(cMelee.mState == PC_Melee.STATE.S_Windup) mState = STATE.WINDUP;
        if(cMelee.mState == PC_Melee.STATE.S_Slashing) mState = STATE.SLASHING;
        if(cMelee.mState == PC_Melee.STATE.S_Recover) mState = STATE.BATTACK_RECOVERY;

        // Figure that shit out.
        if(cMelee.mState == PC_Melee.STATE.S_NOT_Meleeing){
            Debug.Log("not slashing anymore");
            mState = STATE.IDLE;
        }
    }

    // a dot of 0.7 corresponds with 45* in that direction.
    private float GetDotMult(Vector2 vDirToMs, Vector2 vInputDir)
    {
        float dot = Vector2.Dot(vDirToMs.normalized, vInputDir);
        if(dot >= 0.7f){
            return 1f;
        }else if(dot <= -0.7f){
            return _spdBckMult;
        }else{
            return _spdSideMult;
        }
    }
    // Movement is now different depending on where the player is looking.
    private Vector3 HandleInputForVel()
    {
        Camera c = Camera.main;
		Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);

		Vector2 vDir = msPos - (Vector2)transform.position;
		float angle = Mathf.Atan2(vDir.y, vDir.x) * Mathf.Rad2Deg;
		angle -= 90;

        // want the normalized speed halfway in between the x and y max speeds.
        Vector2 vVel = new Vector2();
        float mult = 1f;
        if(Input.GetKey(KeyCode.A)){
            mult = GetDotMult(vDir, -Vector2.right);
            vVel.x -= _spd * mult;
        }
        if(Input.GetKey(KeyCode.D)){
            mult = GetDotMult(vDir, Vector2.right);
            vVel.x += _spd * mult;
        }
        if(Input.GetKey(KeyCode.W)){
            mult = GetDotMult(vDir, Vector2.up);
            vVel.y += _spd * mult;
        }
        if(Input.GetKey(KeyCode.S)){
            mult = GetDotMult(vDir, -Vector2.up);
            vVel.y -= _spd * mult;
        }

        float totalMult = Mathf.Abs(vVel.x/_spd) + Mathf.Abs(vVel.y/_spd);
        if(vVel.x != 0f && vVel.y != 0f){
            totalMult /= 2f;
        }
        vVel = Vector3.Normalize(vVel) * _spd * totalMult;
        if(vVel.magnitude != 0){
            mMoving = true;
        }else{
            mMoving = false;
        }
        return vVel;
    }

    private void RotateToMouse(){
		Camera c = Camera.main;
		Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);

		Vector2 distance = msPos - (Vector2)transform.position;
        // We don't actually rotate the player anymore.
		// float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
		// angle -= 90;
		// transform.eulerAngles = new Vector3(0, 0, angle);

	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_Base>()){
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    return;
                }
            }

            // If an enemy grenade hit us, just make its velocity stop, and it explodes.
            if(col.GetComponent<PJ_EN_PGrenade>()){
                col.GetComponent<PJ_EN_PGrenade>().FEnter_Landed();
                return;
            }
            // Note, will have to change a bit for the needler.
            if(!_debugInvinsible && !mTempInvinsible && p.mProjD._DAM_TYPE != DAMAGE_TYPE.NO_DAMAGE){
                mTempInvinsible = true;
                mTempInvinsibleTmStmp = Time.time;
                cHpShlds.FTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);
            }
            p.FDeath();
        }

        if(col.GetComponent<EN_KnightHitbox>()){
            cHpShlds.FTakeDamage(50f, DAMAGE_TYPE.SLASH);
            Debug.Log("Got slashed by the knight");
        }
        if(col.GetComponent<PJ_Boomerang>()){
            cHpShlds.FTakeDamage(50f, DAMAGE_TYPE.BOOMERANG);
            Destroy(col.gameObject);
            Debug.Log("Hit by boomerang");
        }

        // For explosions, we might also want to be pushed away from the center.
        if(col.GetComponent<EX_Gren>()){
            EX_Gren p = col.GetComponent<EX_Gren>();
            if(!_debugInvinsible && !mTempInvinsible){
                mTempInvinsible = true;
                mTempInvinsibleTmStmp = Time.time;
                cHpShlds.FTakeDamage(p._dam, p._DAM_TYPE);
            }
        }
        if(col.GetComponent<EX_HBlast>()){
            EX_HBlast b = col.GetComponent<EX_HBlast>();
            if(!_debugInvinsible && !mTempInvinsible){
                mTempInvinsible = true;
                mTempInvinsibleTmStmp = Time.time;
                cHpShlds.FTakeDamage(b._damage, DAMAGE_TYPE.EXPLOSION);
                Debug.Log("Damage should be: " + b._damage);
            }
        }

        // If the hunter collided with us.
        if(col.GetComponent<EN_Hunter>()){
            EN_Hunter h = col.GetComponent<EN_Hunter>();
            if(h.mState == EN_Hunter.STATE.LEAPING){
                Debug.Log("Hit by charging hunter");
                if(!_debugInvinsible && !mTempInvinsible){
                    mTempInvinsible = true;
                    mTempInvinsibleTmStmp = Time.time;
                    cHpShlds.FTakeDamage(h._leapDmg, DAMAGE_TYPE.MELEE);
                    mFlyingAfterDamage = true;
                    mFlyingTimeStmp = Time.time;
                    Debug.Log("sent flying");
                    cRigid.velocity = h.GetComponent<Rigidbody2D>().velocity * 2f;
                }
            }
        }

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("SN_MN_Main");
        }
    }

    public void OnTriggerStay2D(Collider2D col)
    {
        // Debug.Log("Inside: " + col.gameObject);
    }

    public void FHandleDamExternal(float amt, DAMAGE_TYPE _TYPE)
    {
        if(_debugInvinsible)
        {
            return;
        }
        cHpShlds.FTakeDamage(amt, _TYPE);
    }
}
