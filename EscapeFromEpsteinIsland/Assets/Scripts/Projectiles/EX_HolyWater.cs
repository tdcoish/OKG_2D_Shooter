/***************************************************************************************************
I want the holy water to do both burst damage initially, as well as constant damage afterwards.
For now I'll just make it constant damage and change it later.

Actually implementing constant damage is a bit annoying.
***************************************************************************************************/

using UnityEngine;

public class EX_HolyWater : MonoBehaviour
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
            EN_Base[] a = FindObjectsOfType<EN_Base>();
            for(int i=0; i<a.Length; i++){
                if(Vector3.Distance(transform.position, a[i].transform.position) < _damRadius){
                    // Eventually needs to be done through combat manager.
                    a[i].FAcceptHolyWaterDamage(_damPerSecond * _damTick);
                }
            }

            mLastDamTmStmp = Time.time;
        }
        
        if(Time.time - mCreatedTmStmp > _lifeSpan){
            Destroy(rSpawnedParticles);
            Destroy(gameObject);
        }
    }
}
