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
    public int                              _numWormsToSpawnUponDeath;
    public bool                             mWormsSpawned = false;
    public float                            _wormSpawnDelay = 0.1f;
    public Man_Combat                       rOverseer;

    public GameObject                       PF_Particles;
    [HideInInspector]
    public GameObject                       rSpawnedParticles;

    // It's actually the explosion that spawns in the worms.
    public EN_FloodInfectionForm        PF_InfectionForm;

    public void F_Start(Man_Combat refOverseer, EN_BPBertha originBertha)
    {
        rOverseer = refOverseer;
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
        float percentToCenterFromEdge;
        if(disToPC < _damageRadius){
            percentToCenterFromEdge = (_damageRadius - disToPC) / _damageRadius;
            rOverseer.rPC.F_ReceiveBerthaExplodeDamage(percentToCenterFromEdge * _fullDamage);
        }

        // Now do all of them.
        float disToActor;
        // This line causes the entire editor to hang?
        for(int i=0; i<rOverseer.rActors.Count; i++){
            if(rOverseer.rActors[i] == originBertha) continue;
            if(rOverseer.rActors[i].GetComponent<PC_Cont>()) continue;
            disToActor = Vector2.Distance(rOverseer.rActors[i].transform.position, transform.position);
            if(disToActor > _damageRadius) continue;
            percentToCenterFromEdge = (_damageRadius - disToActor) / _damageRadius;
            EN_Base b = rOverseer.rActors[i].GetComponent<EN_Base>();
            if(b != null){
                b.FTakeDamage(percentToCenterFromEdge * _fullDamage, DAMAGE_TYPE.EXPLOSION);
            }
        }
    }

    public void Update()
    {
        if(rOverseer == null) return;
        if(!mWormsSpawned){
            if(Time.time - mCreatedTmStmp > _wormSpawnDelay){
                mWormsSpawned = true;
                for(int i=0; i<_numWormsToSpawnUponDeath; i++){
                    EN_FloodInfectionForm f = Instantiate(PF_InfectionForm, transform.position, transform.rotation);
                    rOverseer.rActors.Add(f);
                    f.rOverseer = rOverseer;
                    f.RUN_Start();
                }
            }
        }
        if(Time.time - mCreatedTmStmp > _lifespan){
            Destroy(rSpawnedParticles);
            Destroy(gameObject);
        }
    }
}
