/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class PJ_EN_Plasmoid : MonoBehaviour
{
    public Rigidbody2D          cRigid;

    public float                _spd;
    float                       mTimeCreated;
    public float                _lifetime = 10f;

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
}
