/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_EN_Plasmoid : PJ_Base
{

    // probably some increase to shields or whatever, decrease to health.
    public float                _damage;

    public GameObject           PF_Particles;

    public void FFirePlasmoid(Vector3 normalizedDir)
    {
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        cRigid.velocity = normalizedDir * mProjD._spd;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Instantiate(PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
