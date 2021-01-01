/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public struct EnemyData
{
    public Health                           mHealth;
    public GameObject                       PF_Particles;
    public UI_EN                            gUI;
}

public class EN_Base : MonoBehaviour
{
    public EnemyData                    mEnD;

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("hit somethign");
        if(col.GetComponent<PJ_Base>())
        {
            PJ_Base p = col.GetComponent<PJ_Base>();
            // take damage. No shields.
            mEnD.mHealth.mAmt -= p.mProjD._damage;
            Debug.Log("Took: " + p.mProjD._damage + " damage");

            if(mEnD.mHealth.mAmt <= 0f){
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
        Instantiate(mEnD.PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
