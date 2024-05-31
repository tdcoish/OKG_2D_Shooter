/***************************************************************************************************
The counterpoint to the holy water. Thrown by antifas.
***************************************************************************************************/
using UnityEngine;

public class EX_Piss : MonoBehaviour
{
    public float                            mCreatedTmStmp;
    public float                            _lifeSpan;
    public float                            _damPerSecond;
    public DAMAGE_TYPE                      _DAM_TYPE;
    public float                            _damRadius = 1.5f;
    public float                            _damTick = 0.1f;
    public float                            mLastDamTmStmp;

    public GameObject                       PF_Particles;
    [HideInInspector]
    public GameObject                       rSpawnedParticles;

    void Start()
    {
        mCreatedTmStmp = Time.time;
        mLastDamTmStmp = Time.time - _damTick;
        GameObject g = rSpawnedParticles = Instantiate(PF_Particles, transform.position, transform.rotation);
        Vector3 posFixingZ = g.transform.position; posFixingZ.z = -1f;
        g.transform.position = posFixingZ;
    }

    void Update()
    {
        if(Time.time - mLastDamTmStmp > _damTick){
            PC_Cont rPC = FindObjectOfType<PC_Cont>();
            if(Vector3.Distance(transform.position, rPC.transform.position) < _damRadius){
                rPC.FHandleDamExternal(_damPerSecond * _damTick, DAMAGE_TYPE.PISS);
            }

            mLastDamTmStmp = Time.time;
        }
        
        if(Time.time - mCreatedTmStmp > _lifeSpan){
            Destroy(rSpawnedParticles);
            Destroy(gameObject);
        }
    }
}
