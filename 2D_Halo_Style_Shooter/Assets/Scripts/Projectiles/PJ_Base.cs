/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public enum PROJ_TYPE{PLASMA, BULLET, OTHER}

[System.Serializable]
public enum DAMAGE_TYPE{NO_DAMAGE, PLASMA, BULLET, GRENADE, MELEE, EXPLOSION, SNIPER, SLASH, BOOMERANG}

[System.Serializable]
public struct ProjectileData
{
    public float                        _spd;
    // public float                        _turnRate;
    public float                        _lifespan;
    public float                        _damage;
    public PROJ_TYPE                    _TYPE;
    public DAMAGE_TYPE                  _DAM_TYPE;

    // Currently just used to ignore something shooting itself.
    public GameObject                   rOwner;             // CHECK IF NULL ALWAYS!!!!!!!

    public GameObject                   PF_Particles;
}

public class PJ_Base : MonoBehaviour
{
    public ProjectileData               mProjD;
    public Rigidbody2D                  cRigid;

    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, mProjD._lifespan);
    }

    public void FShootAt(Vector3 vDest, Vector3 vStartPos, GameObject owner)
    {
        Vector3 vDir = Vector3.Normalize(vDest - vStartPos);
        cRigid.velocity = vDir * mProjD._spd;
        mProjD.rOwner = owner;      // The thing that shot us, eg. the player, this elite, that turret, etc.
    }

    public void FDeath()
    {
        if(mProjD.PF_Particles == null){
            Debug.Log(gameObject + " is missing death particles");
        }else{
            Instantiate(mProjD.PF_Particles, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    // This could cause some problems. Can be overwritten though.
    public void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<ENV_Rock>() || col.GetComponent<ENV_Wall>()){
            Debug.Log("Hit env component.");
            FDeath();
        }
    }
}
