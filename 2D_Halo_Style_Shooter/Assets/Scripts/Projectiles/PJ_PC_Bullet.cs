/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_Bullet : PJ_Base
{
    
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, mProjD._lifespan);
    }
}
