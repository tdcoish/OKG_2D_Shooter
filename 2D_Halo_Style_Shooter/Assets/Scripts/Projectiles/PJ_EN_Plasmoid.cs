/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_EN_Plasmoid : MonoBehaviour
{
    public Rigidbody2D          cRigid;

    public float                _spd;
    float                       mTimeCreated;
    public float                _lifetime = 10f;

    // probably some increase to shields or whatever, decrease to health.
    public float                _damage;

    public GameObject           PF_Particles;

    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        mTimeCreated = Time.time;
    }
    void Update()
    {
        if(Time.time - mTimeCreated > _lifetime){
            Destroy(gameObject);
        }
    }

    public void FFirePlasmoid(Vector3 normalizedDir)
    {
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        cRigid.velocity = normalizedDir * _spd;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Instantiate(PF_Particles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
