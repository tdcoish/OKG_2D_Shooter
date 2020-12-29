/*************************************************************************************
Has to manage his gun. Also move around. Also do cute behaviours.
*************************************************************************************/
using UnityEngine;

public class EN_Elite : MonoBehaviour
{
    public enum STATE{TRY_TO_FIRE, SLEEP}
    public STATE                        mState;

    public PC_Cont                      rPC;
    public EN_PRifle                    cRifle;

    public float                        _maxHealth;
    public float                        mHealth;

    public GameObject                   gShotPoint;

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
    }

    void RUN_TryToFire()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>())
        {
            Debug.Log("You shot me!");
            mHealth -= 50f;
            if(mHealth <= 0f){
                Destroy(gameObject);
            }
        }
    }
}
