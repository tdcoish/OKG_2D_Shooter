/*************************************************************************************

*************************************************************************************/
using UnityEngine;

[System.Serializable]
public enum PROJ_TYPE{PLASMA, BULLET}

[System.Serializable]
public struct ProjectileData
{
    public float                        _spd;
    // public float                        _turnRate;
    public float                        _lifespan;
    public float                        _damage;
    public PROJ_TYPE                    _TYPE;
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

}
