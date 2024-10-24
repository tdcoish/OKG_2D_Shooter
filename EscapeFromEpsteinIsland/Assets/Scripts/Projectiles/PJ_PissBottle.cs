using UnityEngine;

public class PJ_PissBottle : MonoBehaviour
{
    public Rigidbody2D                          cRigid;

    public float                                _spdInAir;
    public float                                mLandTmStmp;
    public Vector3                              vDest;
    public EX_Piss                              PF_Explosion;

    public bool                                 mProperlyInstantiated = false;
    
    public void FRunStart(Vector3 dest)
    {
        mProperlyInstantiated = true;
        vDest = new Vector2(); vDest = dest; vDest.z = 0f;
    }

    void Explode()
    {
        Instantiate(PF_Explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    void Update()
    {
        if(!mProperlyInstantiated) return;

        Vector3 vDif = vDest - transform.position;
        cRigid.velocity = vDif.normalized * _spdInAir;
        if((Vector3.Distance(transform.position, vDest) < 0.1f)){
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<ENV_TileRock>()){
            Explode();
        }
    }
}
