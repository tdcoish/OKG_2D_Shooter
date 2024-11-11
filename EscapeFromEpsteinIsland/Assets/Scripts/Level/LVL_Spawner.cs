using UnityEngine;

public class LVL_Spawner : MonoBehaviour
{
    public Man_Combat           rOverseer;
    public Sprite               sGrey;
    public Sprite               sYellow;
    public Sprite               sBlue;
    public Sprite               sRed;
    public SpriteRenderer       sRenderer;
    public GameObject           PF_Particles;
    public bool                 mParticlesSpawnedForThisActor = false;

    // maybe just flash depending on the time left.
    public float                _yellowTime = 2f;
    public float                _redTime = 0.5f;

    public bool                 mActorInPipeline = false;
    public Actor                PF_ActorToSpawn;
    public float                mSpawnCommandTmStmp;
    public float                mSpawnTargetTime;         // set by the thing that tells us to spawn.

    public void Start()
    {
        rOverseer = FindObjectOfType<Man_Combat>();
    }

    public void F_SpawnActorInPipeline()
    {
        Actor a = Instantiate(PF_ActorToSpawn, transform.position, transform.rotation);
        rOverseer.FStartAndAddActor(a);
        mActorInPipeline = false;
    }

    public void F_StoreSpawnActorCommand(Actor type, float delayTime)
    {
        // If there is already an actor in the pipeline, spawn them immediately.
        // Actually, we need to store multiple actors in a queue, but that's more complicated.
        if(mActorInPipeline){
            F_SpawnActorInPipeline();        
        }

        mActorInPipeline = true;
        mSpawnCommandTmStmp = Time.time;
        mSpawnTargetTime = Time.time + delayTime;
        PF_ActorToSpawn = type;
        mParticlesSpawnedForThisActor = false;
    }

    void Update()
    {
        if(mActorInPipeline){
            if(Time.time > mSpawnTargetTime){
                F_SpawnActorInPipeline();
            }
        }

        // purely graphical.
        if(!mActorInPipeline){
            sRenderer.sprite = sGrey;
        }else{
            float timeLeft = mSpawnTargetTime - Time.time;
            if(!mParticlesSpawnedForThisActor){
                GameObject particles = Instantiate(PF_Particles, transform.position, transform.rotation);
                Destroy(particles.gameObject, timeLeft);
                mParticlesSpawnedForThisActor = true;
            }

            if(timeLeft < _redTime){
                sRenderer.sprite = sRed;
            }else if(timeLeft < _yellowTime){
                sRenderer.sprite = sYellow;
            }else{
                sRenderer.sprite = sBlue;
            }
        }

    }
}
