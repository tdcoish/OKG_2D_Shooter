/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EN_HandleHits : MonoBehaviour
{
    public A_HealthShields              cHpShlds;
    public EN_Misc                      cMisc;

    void Start()
    {
        cMisc = GetComponent<EN_Misc>();
        cHpShlds = GetComponent<A_HealthShields>();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("hit somethign");
        if(col.GetComponent<PJ_Base>())
        {
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    Debug.Log("Hit ourselves, no damage");
                    return;
                }
            }
            // take damage. No shields.
            cHpShlds.FTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);

            p.FDeath();

            if(cHpShlds.mHealth.mAmt <= 0f){
                Debug.Log("Dead");
                KillOurselves();
            }
        }
    }

    public void KillOurselves()
    {
        Debug.Log("Kill myself");
        // need a reference to the level manager, audio system, etcetera.
        LVL_Man p = FindObjectOfType<LVL_Man>();
        if(p != null)
        {
            p.FHandleEnemyKilled(gameObject);
        }
        Instantiate(cMisc.PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }


}
