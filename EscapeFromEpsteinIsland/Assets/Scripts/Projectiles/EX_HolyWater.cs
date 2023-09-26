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
    public float                            _dam;
    public DAMAGE_TYPE                      _DAM_TYPE;

    public GameObject                       PF_Particles;
    [HideInInspector]
    public GameObject                       rSpawnedParticles;

    void Start()
    {
        mCreatedTmStmp = Time.time;
        GameObject g = rSpawnedParticles = Instantiate(PF_Particles, transform.position, transform.rotation);
        Vector3 posFixingZ = g.transform.position; posFixingZ.z = -1f;
        g.transform.position = posFixingZ;
    }

    void Update()
    {
        if(Time.time - mCreatedTmStmp > _lifeSpan){
            Destroy(rSpawnedParticles);
            Destroy(gameObject);
        }
    }
}
