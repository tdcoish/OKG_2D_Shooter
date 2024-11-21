using UnityEngine;
using System.Collections.Generic;

public class LVL_Spawner : MonoBehaviour
{
    public class SpawnData
    {
        public Actor                PF_Actor;
        public float                mCommandTmStmp;
        public float                mTargetTime;
        public bool                 mParticlesSpawnedYet;
        public SpawnData(Actor a, float commandTime, float targetTime)
        {
            PF_Actor = a; mCommandTmStmp = commandTime; mTargetTime = targetTime; mParticlesSpawnedYet = false;
        }
    }

    public Man_Combat           rOverseer;
    public Sprite               sGrey;
    public Sprite               sYellow;
    public Sprite               sBlue;
    public Sprite               sRed;
    public SpriteRenderer       sRenderer;
    public GameObject           PF_Particles;

    // maybe just flash depending on the time left.
    public float                _yellowTime = 2f;
    public float                _redTime = 0.5f;

    public List<SpawnData>      mSpawnQueue;

    public void Start()
    {
        rOverseer = FindObjectOfType<Man_Combat>();
        mSpawnQueue = new List<SpawnData>();
    }

    public void F_SpawnActorInPipeline()
    {
        Actor a = Instantiate(mSpawnQueue[0].PF_Actor, transform.position, transform.rotation);
        if(a == null) Debug.Log("Actor null");
        if(rOverseer == null) Debug.Log("Overseer null");
        rOverseer.FStartAndAddActor(a);
        mSpawnQueue.RemoveAt(0);
    }

    public void F_StoreSpawnActorCommand(Actor type, float delayTime)
    {
        SpawnData d = new SpawnData(type, Time.time, Time.time + delayTime);
        if(mSpawnQueue.Count > 0){
            SpawnData prev = mSpawnQueue[mSpawnQueue.Count-1];
            d.mCommandTmStmp += prev.mTargetTime - Time.time;
            d.mTargetTime = d.mCommandTmStmp + delayTime;
        }
        mSpawnQueue.Add(d);
    }

    void Update()
    {
        if(mSpawnQueue.Count != 0){
            if(Time.time > mSpawnQueue[0].mTargetTime){
                F_SpawnActorInPipeline();
            }
        }

        // purely graphical.
        if(mSpawnQueue.Count == 0){
            sRenderer.sprite = sGrey;
        }else{
            float timeLeft = mSpawnQueue[0].mTargetTime - Time.time;
            if(!mSpawnQueue[0].mParticlesSpawnedYet){
                GameObject particles = Instantiate(PF_Particles, transform.position, transform.rotation);
                Destroy(particles.gameObject, timeLeft);
                mSpawnQueue[0].mParticlesSpawnedYet = true;
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
