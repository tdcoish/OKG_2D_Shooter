/*************************************************************************************

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
    public enum STATE{TRY_TO_FIRE, CHASE}
    public STATE                            mState;

    public Health                           mHealth;
    [HideInInspector]
    public Rigidbody2D                      cRigid;
    public PC_Cont                          rPC;
    [HideInInspector]
    public EN_PRifle                        cRifle;

    public Image                            IMG_HealthBar;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        cRifle = GetComponent<EN_PRifle>();

        mHealth.mAmt = mHealth._max;
    }

    // Let's just get him to fire at the player to start.
    void Update()
    {
        cRifle.FAttemptFire(rPC, transform.position);
        cRifle.mData = cRifle.FRunUpdate(cRifle.mData);

        IMG_HealthBar.fillAmount = mHealth.mAmt / mHealth._max;
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
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }
    }
}
