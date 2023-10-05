using UnityEngine;

public class EX_BPBertha : MonoBehaviour
{
    public SpriteRenderer                   rSprite;
    public float                            _lifespan;
    public float                            mCreatedTmStmp;
    public float                            _fadeoutTime = 0.5f;
    public float                            mFadeoutTmStmp;
    public float                            _damageRadius = 2f;
    public float                            _fullDamage = 100f;

    public GameObject                       PF_Particles;
    [HideInInspector]
    public GameObject                       rSpawnedParticles;

    public void F_Start(Man_Combat rOverseer)
    {
        mCreatedTmStmp = Time.time;
        rSpawnedParticles = Instantiate(PF_Particles, transform.position, transform.rotation);
        Vector3 posFixingZ = rSpawnedParticles.transform.position; posFixingZ.z = -1f;
        rSpawnedParticles.transform.position = posFixingZ;

        // Now that we are created, we do damage to everything in the area.
        // Or at least just the PC for now.
        if(rOverseer.rPC == null){
            return;
        }

        float disToPC = Vector2.Distance(transform.position, rOverseer.rPC.transform.position);
        if(disToPC > _damageRadius) return;

        float percentToCenterFromEdge = (_damageRadius - disToPC) / _damageRadius;
        rOverseer.rPC.F_ReceiveBerthaExplodeDamage(percentToCenterFromEdge * _fullDamage);

    }

    public void Update()
    {
        if(Time.time - mCreatedTmStmp > _lifespan){
            Destroy(rSpawnedParticles);
            Destroy(gameObject);
        }
    }
}
