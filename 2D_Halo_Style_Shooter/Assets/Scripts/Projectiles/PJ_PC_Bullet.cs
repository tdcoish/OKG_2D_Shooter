/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_Bullet : MonoBehaviour
{
    public float                            _lifeSpan = 5f;
    public float                            _spd = 20f;
    public Rigidbody2D                      cRigid;
    
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _lifeSpan);
    }

    void Update()
    {
        
    }
}
