/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct Shields
{
    public enum STATE{FULL, BROKEN, RECHARGING}
    public STATE                            mState;
    public float                            _max;
    public float                            mStrength;
    public float                            _brokenTime;
    public float                            mBrokeTmStmp;
    public float                            _rechSpd;
}

public class PC_Cont : MonoBehaviour
{

    private Rigidbody2D                     cRigid;
    // private PC_Gun                          cGun;
    // private PC_Grnd                         cGrnd;
    private PC_Gun                          cGun;
    private PC_PRifle                       cPRifle;
    private PC_Gren                         cGren;

    public GameObject                       gShotPoint;
    public GameObject                       PF_Particles;

    public float                            _spd;
    public float                            _spdFwdMult = 1f;
    public float                            _spdBckMult = 0.5f;
    public float                            _spdSideMult = 0.7f;

    public Shields                          mShields;
    public Health                           mHealth;
    
    public UI_PC                            rUI;

    public bool                             _invinsible = false;

    public bool                             mARifleActive = true;

    void Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        cGun = GetComponent<PC_Gun>();
        cPRifle = GetComponent<PC_PRifle>();
        cGren = GetComponent<PC_Gren>();

        rUI = FindObjectOfType<UI_PC>();
        if(rUI == null){
            Debug.Log("No PC User Interface Found");
        }

        mHealth.mAmt = mHealth._max;

        mShields.mStrength = 75f;
        mShields.mState = Shields.STATE.BROKEN;
        cGun.mState = PC_Gun.STATE.CAN_FIRE;
        cGun.mGunD.mIsActive = true;
        cPRifle.mGunD.mIsActive = false;
    }

    void Update()
    {
        cRigid.velocity = HandleInputForVel();
        RotateToMouse();

        // // simulate damaged shields.
        // if(Input.GetMouseButtonDown(1)){
        //     cShields.FTakeDamage(0.2f);
        // }

        if(Input.GetKeyDown(KeyCode.Tab)){
            mARifleActive = !mARifleActive;
            cGun.mGunD.mIsActive = mARifleActive;
            cPRifle.mGunD.mIsActive = !mARifleActive;
        }

        // It is now time to make a system where the weapons are all controlled by the player.
        if(Input.GetMouseButton(0)){
            if(mARifleActive){
                cGun.FAttemptFire(Camera.main.ScreenToWorldPoint(Input.mousePosition), gShotPoint.transform.position);
            }else{
                cPRifle.FAttemptFire(Camera.main.ScreenToWorldPoint(Input.mousePosition), gShotPoint.transform.position);
            }
        }
        // Try to throw grenade. - Need to interrupt reload.
        if(Input.GetMouseButton(1)){
            cGren.FTryToThrowGrenade(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if(cGun.mState == PC_Gun.STATE.RELOADING){
                cGun.FResetReload();
            }
        }
        if(Input.GetKeyDown(KeyCode.R)){
            cGun.FAttemptReload();
        }

        mShields = RUN_UpdateShieldsData(mShields);
        cGun.FRunGun();
        cPRifle.FRunGun();

        rUI.FillShieldAmount(mShields.mStrength, mShields._max);
        rUI.FillHealthAmount(mHealth.mAmt, mHealth._max);
        rUI.FSetWepActGraphics(mARifleActive);
        rUI.FSetARifleUI(cGun.mClipD.mAmt, cGun.mClipD._size, cGun.mState, cGun.mClipD.mReloadTmStmp, cGun.mClipD._reloadTime);
        rUI.FSetPRifleUI(cPRifle.mPlasmaD.mHeat, cPRifle.mPlasmaD._maxHeat, cPRifle.mState);
        // cGun.FRun();
        // cGrnd.FRun();
        // CheckDead();
        // rUI.FSetBarSize(_health/_maxHealth);
        // rUI.FSetAmmoBarSize(cGun._ammo, cGun._maxAmmo);
        // rUI.FSetShieldBarSize(cShields._val, cShields._maxVal);
        // rUI.FSetTimeText(Time.time);
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
        return vVel;
    }

    private void RotateToMouse(){
		Camera c = Camera.main;
		Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);

		Vector2 distance = msPos - (Vector2)transform.position;
		float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
		angle -= 90;
		transform.eulerAngles = new Vector3(0, 0, angle);
	}

    public Shields RUN_UpdateShieldsData(Shields copiedData)
    {
        switch(copiedData.mState)
        {
            case Shields.STATE.FULL: copiedData = RUN_UpdateShieldsFull(copiedData); break;
            case Shields.STATE.RECHARGING: copiedData = RUN_UpdateShieldsRecharging(copiedData); break;
            case Shields.STATE.BROKEN: copiedData = RUN_UpdateShieldsBroken(copiedData); break;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsFull(Shields copiedData)
    {
        if(copiedData.mStrength < copiedData._max){
            Debug.Log("Mistake: Shields in full state while not fully charged.");
            copiedData.mState = Shields.STATE.BROKEN;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsBroken(Shields copiedData)
    {
        if(Time.time - copiedData.mBrokeTmStmp > copiedData._brokenTime){
            copiedData.mState = Shields.STATE.RECHARGING;
        }
        return copiedData;
    }

    public Shields RUN_UpdateShieldsRecharging(Shields copiedData)
    {
        copiedData.mStrength += Time.deltaTime * copiedData._rechSpd;
        if(copiedData.mStrength >= copiedData._max){
            copiedData.mStrength = copiedData._max;
            copiedData.mState = Shields.STATE.FULL;
        }
        return copiedData;
    }

    // For now, just say that plasma damage does 2x to shields, 1/2 to health, and vice versa for human weapon.
    public void FTakeDamage(float amt, PROJ_TYPE type)
    {
        // No matter what, the shields reset the recharge. Man, "Broken" was a terrible name for this effect.
        mShields.mState = Shields.STATE.BROKEN;
        mShields.mBrokeTmStmp = Time.time;
        // do damage to shields first.
        float modifier = 1f;
        if(type ==  PROJ_TYPE.PLASMA){
            modifier = 2.0f;
        }
        if(type == PROJ_TYPE.BULLET){
            modifier = 0.5f;
        }
        // should be properly handling the spill over, but it's fine.
        float healthDam = (amt * modifier) - mShields.mStrength;
        mShields.mStrength -= amt * modifier;
        if(mShields.mStrength < 0f) mShields.mStrength = 0f;
        if(healthDam > 0f){     // shields could not fully contain the attack.
            healthDam /= modifier * modifier;
            mHealth.mAmt -= healthDam;
        }
        // for now, just have the same modifier amounts, but in reverse.
        Debug.Log("Health Dam: " + healthDam);

        if(mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("SN_MN_Main");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_Base>()){
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    Debug.Log("Player shot himself");
                    return;
                }
            }
            // Unfortunately not true for the needler, which needs to accumulate for a while, but maybe we spawn in a new obj that's like "NeedlerstickObj".
            p.FDeath();

            FTakeDamage(p.mProjD._damage, p.mProjD._TYPE);
        }
    }
}
