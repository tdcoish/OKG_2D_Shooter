using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class MAN_Spawner : MonoBehaviour
{
    public Man_Combat               cMan;
    public SO_PlayDetails           SO_PlayDetails;

    // Want to change the spawner to spawning enemies with more control.

    public bool                     mSpawnEnemies = true;
    public string                   mDefaultScenarioName = "NPC Overload";
    public bool                     mPlayerHasBeatenScenario = false;
    public Scenario                 mActiveScenario;
    public int                      mScenarioWaveIndex = 0;
    public float                    _startingSpawnInterval = 3f;
    public int                      mCurSpawnActorIndice;
    public int                      mCurWaveIndice = 0;
    public float                    _curWaveTimeLength = 40f;
    // Rate limiting, so we don't have 40 entities popping into the same spots.
    public int                      _maxWaveEntitiesSpawnedPerSecond = 4;
    public float                    mWaveTmStmp;
    public float                    mSpawnTmStmp;
    public List<LVL_Spawner>        rSpawners;
    public int                      mSpawnerIndice;
    public List<Actor>              _spawnOrder;
    public float                    _endlessTimeBetweenWaves = 10f;
    public int                      _endlessStartingWavePoints = 20;
    public int                      _endlessWavePointsIncrease = 5;
    public int                      mEndlessCurWavePoints;
    public float                    mEndlessSpawnTmStmp;
    public bool                     mEndlessHasStarted = false;
    public int                      _fastClearBonus = 500;
    public int                      _ultraFastClearBonus = 1500;
    // This is so fragile. Needs to be a better way.
    public Dictionary<int, Actor>   _typeDictionary;
    public Actor                    PF_NPC;
    public Actor                    PF_SchlomoSpellcaster;
    public Actor                    PF_Troon;
    public Actor                    PF_Hunter;
    public Actor                    PF_Grunt;
    public Actor                    PF_ZOGbot;
    public Actor                    PF_Beamer;
    public Actor                    PF_BPBertha;
    public Actor                    PF_Antifa;
    public Actor                    PF_Jequeerus;
    public Actor                    PF_Shaniqua;
    public Actor                    PF_GCaptain;
    public Actor                    PF_FtM;
    public Actor                    PF_MGunner;
    public Actor                    PF_Shielder;
    public Dictionary<int, string>  _typesIndexToString;

    public Pk_Powerup               PF_Powerup;
    public float                    _healthSpawnInterval = 10f;
    public float                    mHealthSpawnTmStmp;
    public int                      mHealthSpawnIndice = 0;
    public List<LVL_Spawnpoint>     rHealthSpawnpoints;

    public void FRUN_Start()
    {
        // Very ugly, but works for now.
        _typeDictionary = new Dictionary<int, Actor>();
        _typeDictionary.Add(0, PF_NPC);
        _typeDictionary.Add(1, PF_SchlomoSpellcaster);
        _typeDictionary.Add(2, PF_Troon);
        _typeDictionary.Add(3, PF_Hunter);
        _typeDictionary.Add(4, PF_Grunt);
        _typeDictionary.Add(5, PF_ZOGbot);
        _typeDictionary.Add(6, PF_Beamer);
        _typeDictionary.Add(7, PF_BPBertha);
        _typeDictionary.Add(8, PF_Antifa);
        _typeDictionary.Add(9, PF_Jequeerus);
        _typeDictionary.Add(10, PF_Shaniqua);
        _typeDictionary.Add(11, PF_GCaptain);
        _typeDictionary.Add(12, PF_FtM);
        _typeDictionary.Add(13, PF_MGunner);
        _typeDictionary.Add(14, PF_Shielder);

        _typesIndexToString = new Dictionary<int, string>();
        _typesIndexToString.Add(0, "NPC");
        _typesIndexToString.Add(1, "SchlomoSpellcaster");
        _typesIndexToString.Add(2, "Knight/Troon");
        _typesIndexToString.Add(3, "Hunter");
        _typesIndexToString.Add(4, "Grunt");
        _typesIndexToString.Add(5, "Elite/ZOGbot");
        _typesIndexToString.Add(6, "Beamer");
        _typesIndexToString.Add(7, "BodyPositiveBertha");
        _typesIndexToString.Add(8, "Antifa");
        _typesIndexToString.Add(9, "Jequeerus");
        _typesIndexToString.Add(10, "Shaniqua");
        _typesIndexToString.Add(11, "GruntCaptain");
        _typesIndexToString.Add(12, "FtM");
        _typesIndexToString.Add(13, "MGunner");
        _typesIndexToString.Add(14, "Shielder");

        cMan = GetComponent<Man_Combat>();
        mCurSpawnActorIndice = 0;
        mWaveTmStmp = Time.time - (_curWaveTimeLength * 0.95f);
        System.Random rand = new System.Random();
        // mSpawnerIndice = rand.Next(rSpawners.Count);
        mSpawnerIndice = 0;
        mHealthSpawnIndice = rand.Next(rHealthSpawnpoints.Count);
        mHealthSpawnTmStmp = Time.time - (_healthSpawnInterval - 1f);
        mEndlessCurWavePoints = _endlessStartingWavePoints;
        mEndlessSpawnTmStmp = Time.time - _endlessTimeBetweenWaves + 1f;

        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.CAMPAIGN){
            mActiveScenario = new Scenario();
            // Have to load in the scenario.
            string scenario = SO_PlayDetails.mCampaignLevel;
            Debug.Log("Level: " + scenario);
            if(scenario == ""){
                mActiveScenario.FLoadScenarioFromFile(mDefaultScenarioName);
            }else{
                mActiveScenario.FLoadScenarioFromFile(scenario);
            }
        }
    }

    public bool F_NoEnemiesLeftToSpawn()
    {
        for(int i=0; i<rSpawners.Count; i++){
            if(rSpawners[i].mSpawnQueue.Count > 0){
                return false;
            }
        }
        return true;
    }

    public void FRUN_Update()
    {
        if(!mSpawnEnemies){
            return;
        }
        if(rSpawners.Count == 0){
            return;
        }
        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.PRACTICE){
            return;
        }

        void SpawnActor(Actor type)
        {
            rSpawners[mSpawnerIndice].F_StoreSpawnActorCommand(type, 2f, true);            
            mSpawnTmStmp = Time.time;
            mSpawnerIndice++;
            if(mSpawnerIndice >= rSpawners.Count){
                mSpawnerIndice = 0;
            }
        }

        // No, this needs to be massively improved. Spawn new amount of enemies every 10 seconds, and 
        // have a certain score of enemies spawned. Next time we increase the amount of enemies spawned.
        void RunEndlessLogic()
        {
            if(_spawnOrder.Count == 0){
                Debug.Log("Forgot to populate actor spawn list.");
                return;
            }

            void SpawnNextWave(int numWaves = 1)
            {
                mEndlessCurWavePoints += (_endlessWavePointsIncrease * numWaves);

                int curWaveTotal = 0;
                while(curWaveTotal < mEndlessCurWavePoints){
                    int nextActorScore = _spawnOrder[mCurSpawnActorIndice].GetComponent<EN_Base>()._endlessScore;
                    if(curWaveTotal + nextActorScore < mEndlessCurWavePoints){
                        // Spawn entity, update actor spawn indice, continue.
                        SpawnActor(_spawnOrder[mCurSpawnActorIndice]);
                        curWaveTotal += nextActorScore;
                        mCurSpawnActorIndice++;
                        if(mCurSpawnActorIndice >= _spawnOrder.Count){
                            mCurSpawnActorIndice = 0;
                        }    
                    }else{
                        // Just spawn NPCs to fill out the roster.
                        for(int i=0; i<mEndlessCurWavePoints - curWaveTotal; i++){
                            SpawnActor(_spawnOrder[0]);
                        }
                        curWaveTotal = mEndlessCurWavePoints;
                    }
                }

                mEndlessSpawnTmStmp = Time.time;
            }

            if(Time.time - mEndlessSpawnTmStmp > _endlessTimeBetweenWaves){
                SpawnNextWave();
                mEndlessHasStarted = true;
            }

            // If there are no enemy actors left, and none in the spawn queue, spawn the next wave now.
            if(cMan.rActors.Count <= 1 && mEndlessHasStarted && F_NoEnemiesLeftToSpawn()){
                float spareTime = Time.time - mEndlessSpawnTmStmp;
                // make more complicated later.
                if((spareTime / _endlessTimeBetweenWaves < 0.5f)){
                    // add score bonus.
                    cMan.cScore.mScore += _ultraFastClearBonus;
                    SpawnNextWave(5);
                }else{
                    cMan.cScore.mScore += _fastClearBonus;
                    SpawnNextWave(3);
                }
            }
        }

        void RunScenarioLogic()
        {
            if(mScenarioWaveIndex >= mActiveScenario.mWaves.Count){
                // Debug.Log("Woah. Already spawned the last wave. Can't spawn another.");
                return;
            }
            // Basically, if it's time to spawn the new wave, do so. 
            // If we're on the last wave, when those enemies are killed, they player wins.
            if(Time.time - mWaveTmStmp > mActiveScenario.mWaves[mScenarioWaveIndex].mTimeBeforeStarting){
                
                // Good God.
                for(int i=0; i<mActiveScenario.mWaves[mScenarioWaveIndex].mNumEnemies.Count; i++){
                    int numOfThisEnemy = mActiveScenario.mWaves[mScenarioWaveIndex].mNumEnemies[_typesIndexToString[i]];
                    Debug.Log(numOfThisEnemy + " of: " + mActiveScenario.mWaves[mScenarioWaveIndex].mNumEnemies[_typesIndexToString[i]]);
                    for(int j=0; j<numOfThisEnemy; j++){
                        SpawnActor(_typeDictionary[i]);
                    }
                }
                mScenarioWaveIndex++;
                mWaveTmStmp = Time.time;
                if(mScenarioWaveIndex >= mActiveScenario.mWaves.Count){
                    Debug.Log("That was the last wave. Handle appropriately.");
                    Debug.Log("Maybe have FBI agents spawn in with increasing numbers or something.");
                }
            }
        }

        if(mSpawnEnemies){
            if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
                RunEndlessLogic();
            }else{
                RunScenarioLogic();
            }
        }

        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
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
}
