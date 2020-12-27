/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_Plasmoid : MonoBehaviour
{
    public float                            _lifeSpan;
    public float                            _spd;
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
