using UnityEngine;
using System.Collections.Generic;

public class MAN_Spawner : MonoBehaviour
{
    public Man_Combat               cMan;

    // Want to change the spawner to spawning enemies with more control.

    public bool                     mSpawnEnemies = true;
    public float                    _startingSpawnInterval = 3f;
    public float                    mCurSpawnInterval;
    public float                    mSpawnTmStmp;
    public float                    _spawnIntervalIncrease = 1.2f;
    public List<LVL_Spawnpoint>     rSpawnpoints;
    public int                      mCurSpawnActorIndice;
    public int                      mSpawnerIndice;
    public List<Actor>              _spawnOrder;

    public Pk_Powerup               PF_Powerup;
    public float                    _healthSpawnInterval = 10f;
    public float                    mHealthSpawnTmStmp;
    public int                      mHealthSpawnIndice = 0;
    public List<LVL_Spawnpoint>     rHealthSpawnpoints;

    public void FRUN_Start()
    {
        cMan = GetComponent<Man_Combat>();
        mCurSpawnInterval = _startingSpawnInterval;
        mSpawnTmStmp = Time.time - (mCurSpawnInterval/1.1f);
        mCurSpawnActorIndice = 0;
        System.Random rand = new System.Random();
        mSpawnerIndice = rand.Next(rSpawnpoints.Count);
        mHealthSpawnIndice = rand.Next(rHealthSpawnpoints.Count);
        mHealthSpawnTmStmp = Time.time - (_healthSpawnInterval - 1f);
    }

    public void FRUN_Update()
    {
        if(!mSpawnEnemies){
            return;
        }
        if(_spawnOrder.Count == 0){
            return;
        }
        if(rSpawnpoints.Count == 0){
            return;
        }

        void StartAndAddActor(Actor a){
            Debug.Log("Spawned something");
            cMan.rActors.Add(a);
            a.RUN_Start();
            a.rOverseer = cMan;
        }

        if(Time.time - mSpawnTmStmp > mCurSpawnInterval){
            Actor a = Instantiate(_spawnOrder[mCurSpawnActorIndice], rSpawnpoints[mSpawnerIndice].transform.position, transform.rotation);
            StartAndAddActor(a);
            mCurSpawnActorIndice++;
            if(mCurSpawnActorIndice >= _spawnOrder.Count){
                mCurSpawnActorIndice = 0;
                mCurSpawnInterval /= _spawnIntervalIncrease;
            }

            mSpawnTmStmp = Time.time;
            mSpawnerIndice++;
            Debug.Log("Spawner indice: " + mSpawnerIndice);
            if(mSpawnerIndice >= rSpawnpoints.Count){
                mSpawnerIndice = 0;
            }
        }

        if(Time.time - mHealthSpawnTmStmp > _healthSpawnInterval){
            Instantiate(PF_Powerup, rHealthSpawnpoints[mHealthSpawnIndice].transform.position, transform.rotation);
            mHealthSpawnTmStmp = Time.time;
            mHealthSpawnIndice++;
            if(mHealthSpawnIndice >= rHealthSpawnpoints.Count){
                mHealthSpawnIndice = 0;
            }
        }


    }



}
