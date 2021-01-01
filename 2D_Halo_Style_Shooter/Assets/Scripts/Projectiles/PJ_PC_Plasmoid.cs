/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_Plasmoid : PJ_Base
{

    public GameObject                       PF_Particles;
    

    void OnTriggerEnter2D(Collider2D col)
    {
        Instantiate(PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
