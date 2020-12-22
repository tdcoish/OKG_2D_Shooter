/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PC_Cont : MonoBehaviour
{

    private Rigidbody2D                     cRigid;
    // private PC_Gun                          cGun;
    // private PC_Grnd                         cGrnd;
    private PC_Shields                      cShields;

    public float                            _spd;
    public float                            _maxHealth = 1000f;
    public float                            _health;
    
    public UI_PC                            rUI;

    public bool                             _invinsible = false;

    void Start()
    {
        cRigid = GetComponent<Rigidbody2D>(); 
        // cGun = GetComponent<PC_Gun>();   
        // cGrnd = GetComponent<PC_Grnd>();
        cShields = GetComponent<PC_Shields>();  

        rUI = FindObjectOfType<UI_PC>();
        if(rUI == null){
            Debug.Log("No PC User Interface Found");
        }

        _health = _maxHealth;
    }

    void Update()
    {
        cRigid.velocity = HandleInputForVel();
        RotateToMouse();

        // cGun.FRun();
        // cGrnd.FRun();
        // CheckDead();
        // rUI.FSetBarSize(_health/_maxHealth);
        // rUI.FSetAmmoBarSize(cGun._ammo, cGun._maxAmmo);
        // rUI.FSetShieldBarSize(cShields._val, cShields._maxVal);
        // rUI.FSetTimeText(Time.time);
    }

    private Vector3 HandleInputForVel()
    {
        Vector2 vVel = new Vector2();
        if(Input.GetKey(KeyCode.A)){
            vVel.x -= _spd;
        }
        if(Input.GetKey(KeyCode.D)){
            vVel.x += _spd;
        }
        if(Input.GetKey(KeyCode.W)){
            vVel.y += _spd;
        }
        if(Input.GetKey(KeyCode.S)){
            vVel.y -= _spd;
        }
        
        vVel = Vector3.Normalize(vVel) * _spd;
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
