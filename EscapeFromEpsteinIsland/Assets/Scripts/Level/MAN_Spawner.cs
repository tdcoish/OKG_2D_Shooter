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
    public List<LVL_Spawnpoint>     rSpawnpoints;
    public int                      mSpawnerIndice;
    public List<Actor>              _spawnOrder;
    public float                    _endlessTimeBetweenWaves = 10f;
    public int                      _endlessStartingWavePoints = 20;
    public int                      _endlessWavePointsIncrease = 5;
    public int                      mEndlessCurWavePoints;
    public float                    mEndlessSpawnTmStmp;
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

        cMan = GetComponent<Man_Combat>();
        mCurSpawnActorIndice = 0;
        mWaveTmStmp = Time.time - (_curWaveTimeLength * 0.95f);
        System.Random rand = new System.Random();
        mSpawnerIndice = rand.Next(rSpawnpoints.Count);
        mHealthSpawnIndice = rand.Next(rHealthSpawnpoints.Count);
        mHealthSpawnTmStmp = Time.time - (_healthSpawnInterval - 1f);
        mEndlessCurWavePoints = _endlessStartingWavePoints;
        mEndlessSpawnTmStmp = Time.time - _endlessTimeBetweenWaves + 1f;

        if(!SO_PlayDetails.mRunEndless){
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

    public void FRUN_Update()
    {
        if(!mSpawnEnemies){
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

        // Slightly randomized starting positions might not be ideal.
        // Instead, manually placing them away from existing actors makes more sense.
        Vector3 SlightRandomizeStartingPos(Vector3 origPos)
        {
            float randomGap = 2f;
            System.Random rand = new System.Random();
            double randomFloat = rand.NextDouble() * randomGap;
            randomFloat -= randomGap/2f;
            origPos.x += (float)randomFloat;
            randomFloat = rand.NextDouble() * randomGap;
            randomFloat -= randomGap/2f;
            origPos.y += (float)randomFloat;
            return origPos;
        }

        void SpawnActor(Actor type)
        {
            // don't spawn if too close to player.
            void FindNextSpawnerFarEnoughFromPlayer()
            {
                int maxIterations = rSpawnpoints.Count;
                int curIterations = 0;
                bool foundAppropriateSpawn = false;
                while(!foundAppropriateSpawn && curIterations < maxIterations){
                    curIterations++;
                    float disToPlayer = Vector2.Distance(cMan.rPC.transform.position, rSpawnpoints[mSpawnerIndice].transform.position);
                    if(disToPlayer < 2f){
                        mSpawnerIndice++;
                        if(mSpawnerIndice >= rSpawnpoints.Count){
                            mSpawnerIndice = 0;
                        }
                    }else{
                        foundAppropriateSpawn = true;
                    }
                }
            }

            FindNextSpawnerFarEnoughFromPlayer();
            Vector3 pos = SlightRandomizeStartingPos(rSpawnpoints[mSpawnerIndice].transform.position);
            Actor a = Instantiate(type, pos, transform.rotation);
            StartAndAddActor(a);
            
            mSpawnTmStmp = Time.time;
            mSpawnerIndice++;
            if(mSpawnerIndice >= rSpawnpoints.Count){
                mSpawnerIndice = 0;
            }
        }

        // No, this needs to be massively improved. Spawn new amount of enemies every 10 seconds, and 
        // have a certain score of enemies spawned. Next time we increase the amount of enemies spawned.
        void RunEndlessLogic()
        {
            Debug.Log("Running endless logic");
            if(_spawnOrder.Count == 0){
                Debug.Log("Forgot to populate actor spawn list.");
                return;
            }

            if(Time.time - mEndlessSpawnTmStmp > _endlessTimeBetweenWaves){
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

                mEndlessCurWavePoints += _endlessWavePointsIncrease;
                mEndlessSpawnTmStmp = Time.time;
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
            if(SO_PlayDetails.mRunEndless){
                RunEndlessLogic();
            }else{
                RunScenarioLogic();
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
