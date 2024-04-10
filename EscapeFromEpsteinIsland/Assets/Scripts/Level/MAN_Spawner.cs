using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class MAN_Spawner : MonoBehaviour
{
    public Man_Combat               cMan;
    public MAN_PlayDetails          cPlayDetails;

    // Want to change the spawner to spawning enemies with more control.

    public bool                     mSpawnEnemies = true;
    public bool                     mForceWaveMode = false;
    public float                    _startingSpawnInterval = 3f;
    public float                    mCurSpawnInterval;
    public int                      mCurSpawnActorIndice;
    public int                      mCurWaveIndice = 0;
    public float                    _curWaveTimeLength = 40f;
    // Rate limiting, so we don't have 40 entities popping into the same spots.
    public int                      _maxWaveEntitiesSpawnedPerSecond = 4;
    public float                    mWaveTmStmp;
    public float                    mSpawnTmStmp;
    public float                    _spawnIntervalIncrease = 1.2f;
    public List<LVL_Spawnpoint>     rSpawnpoints;
    public int                      mSpawnerIndice;
    public List<Actor>              _spawnOrder;
    // This is so fragile. Needs to be a better way.
    public Dictionary<int, Actor> _typeDictionary;
    public Actor                    PF_NPC;
    public Actor                    PF_SchlomoSpellcaster;
    public Actor                    PF_Troon;
    public Actor                    PF_Hunter;
    public Actor                    PF_Grunt;
    public Actor                    PF_ZOGbot;
    public Actor                    PF_Beamer;
    public Actor                    PF_BPBertha;

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

        cMan = GetComponent<Man_Combat>();
        mCurSpawnInterval = _startingSpawnInterval;
        mSpawnTmStmp = Time.time - (mCurSpawnInterval/1.1f);
        mCurSpawnActorIndice = 0;
        mWaveTmStmp = Time.time - (_curWaveTimeLength * 0.95f);
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
            Vector3 pos = SlightRandomizeStartingPos(rSpawnpoints[mSpawnerIndice].transform.position);
            Actor a = Instantiate(type, pos, transform.rotation);
            StartAndAddActor(a);
            
            mSpawnTmStmp = Time.time;
            mSpawnerIndice++;
            if(mSpawnerIndice >= rSpawnpoints.Count){
                mSpawnerIndice = 0;
            }
        }

        void RunEndlessLogic()
        {
            if(Time.time - mSpawnTmStmp > mCurSpawnInterval){
                SpawnActor(_spawnOrder[mCurSpawnActorIndice]);

                mCurSpawnActorIndice++;
                if(mCurSpawnActorIndice >= _spawnOrder.Count){
                    mCurSpawnActorIndice = 0;
                    mCurSpawnInterval /= _spawnIntervalIncrease;
                }    
            }
        }
        
        void RunWaveLogic()
        {
            // When was the last wave spawned?
            // Eventually need a rate limiter, such as 4 entities spawned per second.
            if(Time. time - mWaveTmStmp > _curWaveTimeLength){
                // Spawn all the actors. 
                string path = Application.streamingAssetsPath+"/Waves/FirstWave.bin";
                if(!File.Exists(path)){
                    Debug.Log("ERROR! No wave data detected.");
                    return;
                }

                FileStream fStream = new FileStream(path, FileMode.Open);
                BinaryReader br = new BinaryReader(fStream);
                // Again, fragile.
                for(int i=0; i<_typeDictionary.Count; i++){
                    int numType = br.ReadInt32();
                    for(int j=0; j<numType; j++){
                        SpawnActor(_typeDictionary[i]);
                    }
                }

                br.Close();
                fStream.Close(); 

                mWaveTmStmp = Time.time;
            }
        }

        if(!cPlayDetails.mEndless || mForceWaveMode){
            RunWaveLogic();
        }
        else{
            RunEndlessLogic();
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
