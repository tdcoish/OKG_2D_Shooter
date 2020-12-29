/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_PC_Plasmoid : MonoBehaviour
{
    public float                            _lifeSpan;
    public float                            _spd;
    public Rigidbody2D                      cRigid;

    public GameObject                       PF_Particles;
    
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _lifeSpan);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Instantiate(PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
