/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Cont : MonoBehaviour
{

    private Rigidbody2D                     cRigid;
    // private PC_Gun                          cGun;
    // private PC_Grnd                         cGrnd;
    private PC_Shields                      cShields;
    private PC_Gun                          cGun;

    public float                            _spd;
    public float                            _spdFwdMult = 1f;
    public float                            _spdBckMult = 0.5f;
    public float                            _spdSideMult = 0.7f;
    public float                            _maxHealth = 1000f;
    public float                            mHealth;
    
    public UI_PC                            rUI;

    public bool                             _invinsible = false;

    void Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        // cGun = GetComponent<PC_Gun>();   
        // cGrnd = GetComponent<PC_Grnd>();
        cShields = GetComponent<PC_Shields>();  
        cGun = GetComponent<PC_Gun>();

        rUI = FindObjectOfType<UI_PC>();
        if(rUI == null){
            Debug.Log("No PC User Interface Found");
        }

        mHealth = _maxHealth;

        cShields.mShieldStrength = 0.25f;
        cShields.mState = PC_Shields.STATE.BROKEN;
        cGun.mState = PC_Gun.STATE.CAN_FIRE;
    }

    void Update()
    {
        cRigid.velocity = HandleInputForVel();
        RotateToMouse();

        // simulate damaged shields.
        if(Input.GetMouseButtonDown(1)){
            cShields.FTakeDamage(0.2f);
        }
        if(Input.GetMouseButton(0)){
            cGun.FAttemptFire(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        if(Input.GetKeyDown(KeyCode.R)){
            cGun.FAttemptReload();
        }

        cShields.FRunShields();
        cGun.FRunGun();

        rUI.FillShieldAmount(cShields.mShieldStrength);
        rUI.FSetARifleUI(cGun.mClipAmt, cGun._clipSize, cGun.mState, cGun.mReloadTmStmp, cGun._reloadTime);

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
}
