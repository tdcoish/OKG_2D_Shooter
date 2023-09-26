using UnityEngine;
using System.Collections.Generic;

public class MAN_Spawner : MonoBehaviour
{
    public Man_Combat               cMan;
    public EN_NPC                   PF_NPC;
    public EN_Knight                PF_Knight;
    public EN_Grunt                 PF_Grunt;
    public EN_Hunter                PF_Hunter;
    public EN_FloodInfectionForm    PF_InfectionForm;
    public EN_Beamer                PF_Beamer;
    public EN_Elite                 PF_Elite;

    public bool                     mSpawnEnemies = true;
    public float                    _trashSpawnInterval = 5f;
    public float                    mLastTrashSpawnTmStmp;
    public float                    _hunterSpawnInterval = 15f;
    public float                    mLastHunterSpawnTmStmp;
    public float                    _eliteSpawnInterval = 10f;
    public float                    mLastEliteSpawnTmStmp;
    public float                    _beamerSpawnInterval = 20f;
    public float                    mLastBeamerSpawnTmStmp;
    public float                    _spawnRateIncreasePerTenSec = 1.5f;
    public float                    mSpawnRateIncreaseTmStmp;
    public List<LVL_Spawnpoint>     rSpawnpoints;

    public void FRUN_Start()
    {
        mLastTrashSpawnTmStmp = Time.time - _trashSpawnInterval;
        mLastHunterSpawnTmStmp = Time.time - _hunterSpawnInterval;
        mLastEliteSpawnTmStmp = Time.time - _eliteSpawnInterval;
        mLastBeamerSpawnTmStmp = Time.time - _beamerSpawnInterval;
        cMan = GetComponent<Man_Combat>();
    }

    public void FRUN_Update()
    {
        if(!mSpawnEnemies){
            return;
        }

        if(Time.time - mSpawnRateIncreaseTmStmp > 10f){
            mSpawnRateIncreaseTmStmp = Time.time;
            _trashSpawnInterval /= _spawnRateIncreasePerTenSec;
            _hunterSpawnInterval /= _spawnRateIncreasePerTenSec;
            _eliteSpawnInterval /= _spawnRateIncreasePerTenSec;
            _beamerSpawnInterval /= _spawnRateIncreasePerTenSec;
        }

        void StartAndAddActor(Actor a){
            Debug.Log("Spawned something");
            cMan.rActors.Add(a);
            a.RUN_Start();
            a.rOverseer = cMan;
        }

        if(Time.time - mLastTrashSpawnTmStmp > _trashSpawnInterval){
            mLastTrashSpawnTmStmp = Time.time;
            for(int i=0; i<rSpawnpoints.Count; i++){
                Actor a = new Actor();
                if(i == 0){
                    a = Instantiate(PF_NPC, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                }
                if(i == 1){
                    a = Instantiate(PF_Knight, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                }
                if(i == 2){
                    a = Instantiate(PF_Grunt, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                }
                if(i == 3){
                    a = Instantiate(PF_InfectionForm, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                }
                StartAndAddActor(a);
            }
        }

        if(Time.time - mLastEliteSpawnTmStmp > _eliteSpawnInterval){
            mLastEliteSpawnTmStmp = Time.time;
            for(int i=0; i<rSpawnpoints.Count; i++){
                Actor a = Instantiate(PF_Elite, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                StartAndAddActor(a);
            }
        }

        if(Time.time - mLastHunterSpawnTmStmp > _hunterSpawnInterval){
            mLastHunterSpawnTmStmp = Time.time;
            for(int i=0; i<rSpawnpoints.Count; i++){
                Actor a = Instantiate(PF_Hunter, rSpawnpoints[i].transform.position, rSpawnpoints[i].transform.rotation);
                StartAndAddActor(a);
            }
        }
    }



}
