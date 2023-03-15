/*************************************************************************************
This was ultimately massively premature. Don't do this kind of early optimization again.
*************************************************************************************/
using UnityEngine;

public class EN_HandleHits : MonoBehaviour
{
    public A_HealthShields              cHpShlds;
    public EN_Misc                      cMisc;

    public bool                         mDisableAllExceptMelee = true;

    void Start()
    {
        cMisc = GetComponent<EN_Misc>();
        cHpShlds = GetComponent<A_HealthShields>();
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if(mDisableAllExceptMelee) return;

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
            FEnemySpecificHandleDamTaken();
            p.FDeath();
        }

        if(col.GetComponent<EX_Gren>() != null){
            Debug.Log("Enemy Inside grenade explosion");
            EX_Gren p = col.GetComponent<EX_Gren>();
            cHpShlds.FTakeDamage(p._dam, p._DAM_TYPE);
            FEnemySpecificHandleDamTaken();
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
        if(cMisc.PF_Particles != null) Instantiate(cMisc.PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void FHandleMeleeHit(float _dam, DAMAGE_TYPE _TYPE)
    {
        cHpShlds.FTakeDamage(_dam, _TYPE);
        // At the end of getting potentially hit by things, check if we're dead.
        if(cHpShlds.mHealth.mAmt <= 0f){
            Debug.Log("Dead");
            KillOurselves();
        }
    }

    public void FEnemySpecificHandleDamTaken()
    {
        switch(cMisc._TYPE)
        {
            case ENEMY_TYPE.E_SNIPER: GetComponent<EN_Sniper>().FHandleTookDamage(); break;
        }
    }

}
