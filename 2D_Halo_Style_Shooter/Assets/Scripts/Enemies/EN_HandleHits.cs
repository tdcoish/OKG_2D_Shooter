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
        if(col.GetComponent<PJ_Base>()){
            PJ_Base p = col.GetComponent<PJ_Base>();
            if(p.mProjD.rOwner != null){
                if(p.mProjD.rOwner == gameObject){
                    return;
                }
            }

            // If an enemy grenade hit us, just make its velocity stop, and it explodes.
            if(col.GetComponent<PJ_Gren>()){
                Debug.Log("Hit by some grenade");
                col.GetComponent<PJ_Gren>().FHandleHitObj();
                return;
            }

            cHpShlds.FTakeDamage(p.mProjD._damage, p.mProjD._DAM_TYPE);
            p.FDeath();

            Debug.Log("hit proj");
        }

        if(col.GetComponent<EX_Gren>() != null){
            Debug.Log("Enemy Inside grenade explosion");
            EX_Gren p = col.GetComponent<EX_Gren>();
            cHpShlds.FTakeDamage(p._dam, p._DAM_TYPE);
        }

        // At the end of getting potentially hit by things, check if we're dead.
        if(cHpShlds.mHealth.mAmt <= 0f){
            Debug.Log("Dead");
            KillOurselves();
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
