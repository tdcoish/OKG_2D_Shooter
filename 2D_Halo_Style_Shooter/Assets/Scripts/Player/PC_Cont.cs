/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Cont : MonoBehaviour
{

    private Rigidbody2D                     cRigid;
    // private PC_Gun                          cGun;
    // private PC_Grnd                         cGrnd;
    private PC_Gun                          cGun;
    private PC_PRifle                       cPRifle;
    private PC_Gren                         cGren;
    private A_HealthShields                 cHpShlds;

    public GameObject                       gShotPoint;
    public GameObject                       PF_Particles;

    public float                            _spd;
    public float                            _spdFwdMult = 1f;
    public float                            _spdBckMult = 0.5f;
    public float                            _spdSideMult = 0.7f;
    
    public UI_PC                            rUI;

    public bool                             _invinsible = false;

    public bool                             mARifleActive = true;

    void Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        cGun = GetComponent<PC_Gun>();
        cPRifle = GetComponent<PC_PRifle>();
        cGren = GetComponent<PC_Gren>();
        cHpShlds = GetComponent<A_HealthShields>();

        rUI = FindObjectOfType<UI_PC>();
        if(rUI == null){
            Debug.Log("No PC User Interface Found");
        }

        cHpShlds.mHealth.mAmt = cHpShlds.mHealth._max;
        cHpShlds.mShields.mStrength = 75f;
        cHpShlds.mShields.mState = Shields.STATE.BROKEN;

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

        cHpShlds.mShields = cHpShlds.FRUN_UpdateShieldsData(cHpShlds.mShields);
        cGun.FRunGun();
        cPRifle.FRunGun();

        rUI.FillShieldAmount(cHpShlds.mShields.mStrength, cHpShlds.mShields._max);
        rUI.FillHealthAmount(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max);
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

            // If an enemy grenade hit us, just make its velocity stop, and it explodes.
            if(col.GetComponent<PJ_EN_PGrenade>()){
                col.GetComponent<PJ_EN_PGrenade>().FEnter_Landed();
                return;
            }
            // Note, will have to change a bit for the needler.
            p.FDeath();
            if(!_invinsible){
                cHpShlds.FTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);
            }
        }

        if(col.GetComponent<EX_Gren>()){
            EX_Gren p = col.GetComponent<EX_Gren>();
            Debug.Log("Inside grenade explosion");
            if(!_invinsible){
                cHpShlds.FTakeDamage(p._dam, p._DAM_TYPE);
            }
        }

        if(cHpShlds.mHealth.mAmt <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("SN_MN_Main");
        }
    }
}
