/*************************************************************************************
Has to manage his gun. Also move around. Also do cute behaviours.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public enum DAMAGE_TYPE{PLASMA, BULLET}

public class EN_Elite : MonoBehaviour
{
    public enum STATE{TRY_TO_FIRE, SLEEP}
    public STATE                        mState;

    public PC_Cont                      rPC;
    public EN_PRifle                    cRifle;

    public float                        _maxHealth;
    public float                        mHealth;
    public Shields                      mShields;

    public GameObject                   gShotPoint;
    public GameObject                   PF_Particles;

    public Image                        IMG_ShieldBar;
    public Image                        IMG_HealthBar;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRifle = GetComponent<EN_PRifle>();
        mHealth = _maxHealth;
        mState = STATE.TRY_TO_FIRE;
    }

    // For now he just fires his rifle.
    void Update()
    {
        cRifle.FAttemptFire(rPC, gShotPoint.transform.position);
        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);

        mShields = RUN_UpdateShieldsData(mShields);

        UI_UpdateShieldHealthBars(mHealth, _maxHealth, mShields.mStrength, mShields._max);
    }


    // For now, just say that plasma damage does 2x to shields, 1/2 to health, and vice versa for human weapon.
    public void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        // No matter what, the shields reset the recharge. Man, "Broken" was a terrible name for this effect.
        mShields.mState = Shields.STATE.BROKEN;
        mShields.mBrokeTmStmp = Time.time;
        // do damage to shields first.
        float modifier = 1f;
        if(type == DAMAGE_TYPE.PLASMA){
            modifier = 2.0f;
        }
        if(type == DAMAGE_TYPE.BULLET){
            modifier = 0.5f;
        }
        // should be properly handling the spill over, but it's fine.
        float healthDam = (amt * modifier) - mShields.mStrength;
        mShields.mStrength -= amt * modifier;
        if(mShields.mStrength < 0f) mShields.mStrength = 0f;
        if(healthDam > 0f){     // shields could not fully contain the attack.
            healthDam /= modifier * modifier;
            mHealth -= healthDam;
        }
        // for now, just have the same modifier amounts, but in reverse.
        Debug.Log("Health Dam: " + healthDam);

        if(mHealth <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void RUN_TryToFire()
    {

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

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>())
        {
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }
    }

    void UI_UpdateShieldHealthBars(float mHealth, float _maxHealth, float mShields, float _maxShields)
    {
        IMG_HealthBar.fillAmount = (mHealth / _maxHealth);
        IMG_ShieldBar.fillAmount = (mShields / _maxShields);
    }
}
