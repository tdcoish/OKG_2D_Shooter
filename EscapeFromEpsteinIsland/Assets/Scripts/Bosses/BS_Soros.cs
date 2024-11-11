/***************************************************************************************************
Okay, what does the boss need to do? Swing back and forth and shoot projectiles. Give them multiple
little movement patterns.

The boss is going to have stages. After each stage, the spiral spell is unleashed, and a new pattern
is brought forward, similar to other SCHMUP bosses. Possibly he clears all the projectiles and spells
on the stage when he does this.
***************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BS_Soros : EN_Base
{
    public uint                     kNormal = 1<<2;
    public uint                     kStageRecovery = 1<<3;

    public float                    _stageRecoveryTime = 3f;
    public float                    mStageChangeTmStmp;
    public int                      mStageInd = 0;
    public int                      _numStages = 4;

    public float                    _largeXWavesPerSecond = 0.5f;
    public float                    mStartX = 0f; 
    public float                    mLargeXDis = 4f;
    public float                    _smallXWavesPerSecond = 1.7f;
    public float                    mSmallXDis = 0.5f;
    public float                    _largeYWavesPerSecond = 0.75f;
    public float                    mLargeStartY = 2f;
    public float                    mLargeYDis = 1f;

    public float                    _fireInterval = 0.1f;
    public float                    mFireTmStmp;
    public int                      _salvoNum = 5;
    public int                      mSalvoInd;
    public float                    _salvoInterval = 0.5f;
    public PJ_EN_Plasmoid           PF_Plasmoid;    

    public float                    _spellOneFireInterval = 4f;
    public float                    mSpellOneTmStmp;
    public SPL_StarDavid2           PF_StarDavidSpellOne;
    public float                    _spellTwoFireInterval = 6f;
    public float                    mSpellTwoTmStmp;
    public SPL_StarDavid4           PF_StarDavidSpellTwo;
    
    public SPL_StarDavid3           PF_StarDavidSpellThree;

    public SPL_StarDavid4           rStar1;
    public SPL_StarDavid4           rStar2;

    public SPL_StarDavid5           rStar3;

    public float                    _healthPerStage = 500;
    public float                    mNextStageHealthMark;

    public List<LVL_Spawner>        rSpawners;
    public int                      mSpawnInd;
    public EN_Hunter                PF_Golem;
    public float                    _golemSpawnInterval = 5f;
    public float                    mGolemSpawnTmStmp;

    // Used for the second stage spell
    public GameObject               rCenterStage;
    public float                    _randomRangeSpell = 3f;

    // UI stuff.
    public Image                        IMG_HealthBar;

    public override void F_CharSpecStart()
    {
        _healthPerStage = cHpShlds.mHealth._max / (_numStages * 2);
        mNextStageHealthMark = cHpShlds.mHealth._max - _healthPerStage;
        mGolemSpawnTmStmp = Time.time - _golemSpawnInterval + 2f;
    }

    // Just move him side to side for now.
    public override void F_CharSpecUpdate()
    {
        if(kState == kNormal){
            F_RunNormal();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }else if(kState == kStageRecovery){
            F_RunStageRecovery();
        }

        IMG_HealthBar.fillAmount = (cHpShlds.mHealth.mAmt / cHpShlds.mHealth._max);
    }

    public void F_RunStageRecovery()
    {
        rStar1.gameObject.SetActive(false);
        rStar2.gameObject.SetActive(false);
        rStar3.gameObject.SetActive(false);
        if(Time.time - mStageChangeTmStmp > _stageRecoveryTime){
            kState = kNormal;
            if(mStageInd == 1){
                mSpellOneTmStmp = Time.time - 1f;
            }
            if(mStageInd == 2){
                rStar1.mFireTmStmp = Time.time + 2f;
                rStar2.mFireTmStmp = Time.time + 2f;
            }
            if(mStageInd == 3){
                rStar3.mSalvoTmStmp = Time.time + rStar3._salvoRate;
            }

        }
    }

    public void F_RunNormal()
    {        
        // Handle taking enough damage to change stages.
        if(cHpShlds.mHealth.mAmt < mNextStageHealthMark){
            Instantiate(PF_StarDavidSpellThree, transform.position, transform.rotation);
            mNextStageHealthMark = cHpShlds.mHealth.mAmt - _healthPerStage;
            mStageChangeTmStmp = Time.time;
            mStageInd++;
            if(mStageInd >= _numStages) mStageInd = 0;
            kState = kStageRecovery;

            return;
        }

        void SpawningGolems()
        {
            if(Time.time - mGolemSpawnTmStmp > _golemSpawnInterval){
                rSpawners[mSpawnInd].F_StoreSpawnActorCommand(PF_Golem, 2f);

                // EN_Hunter h = Instantiate(PF_Golem, rSpawners[mSpawnInd].transform.position, transform.rotation);
                // rOverseer.FStartAndAddActor(h);
                mSpawnInd++; if(mSpawnInd >= rSpawners.Count) mSpawnInd = 0;
                mGolemSpawnTmStmp = Time.time;
            }
        }
        SpawningGolems();

        void Movement(){
            // Crazy how I messed up the math, yet the result is good.
            float fudgeFactor = 6f * _largeXWavesPerSecond;
            float newLargeX = (Mathf.Sin(Time.time * fudgeFactor) * mLargeXDis) + mStartX;
            fudgeFactor = 6f * _smallXWavesPerSecond;
            float newSmallX = (Mathf.Sin(Time.time * fudgeFactor) * mSmallXDis) + mStartX;
            fudgeFactor = 6f * _largeYWavesPerSecond;
            float newY = (Mathf.Sin(Time.time * fudgeFactor) * mLargeYDis) + mLargeStartY;
            Vector2 newPos = transform.position;
            newPos.x = newLargeX + newSmallX;
            newPos.y = newY;
            transform.position = newPos;
        }
        Movement();

        void FiringMainProjectiles()
        {
            // Fire or don't fire projectile at player.
            if(mSalvoInd < _salvoNum){
                if(Time.time - mFireTmStmp > _fireInterval){
                    // Fire new projectile.
                    PJ_EN_Plasmoid rPlasmoid = Instantiate(PF_Plasmoid, gShotPoint.transform.position, transform.rotation);
                    Vector3 vDir = rOverseer.rPC.transform.position - gShotPoint.transform.position;
                    vDir = Vector3.Normalize(vDir);
                    rPlasmoid.cRigid.velocity = vDir * rPlasmoid.mProjD._spd;
                    rPlasmoid.mProjD.rOwner = gameObject;
                    rPlasmoid.transform.up = vDir;

                    mFireTmStmp = Time.time;
                    mSalvoInd++;
                }
            }else{
                if(Time.time - mFireTmStmp > _salvoInterval){
                    mSalvoInd = 0;
                }
            }
        }

        // Now, what else? Spawning in some spells, projectiles that shoot other projectiles, etcetera.
        void FiringSpells()
        {
            if(Time.time - _spellOneFireInterval > mSpellOneTmStmp){
                Vector2 GenerateRandomSpellPos()
                {
                    float yRand = Random.Range(-_randomRangeSpell, _randomRangeSpell);
                    float xRand = Random.Range(-_randomRangeSpell, _randomRangeSpell);
                    Vector2 targetPos = rCenterStage.transform.position;
                    targetPos.x += xRand; targetPos.y += yRand;
                    return targetPos;
                }
                SPL_StarDavid2[] alreadyCastSpells = FindObjectsOfType<SPL_StarDavid2>();
                int its = 0;
                while(its < 100){
                    bool spawnFound = true;
                    Vector2 tPos = GenerateRandomSpellPos();
                    for(int i=0; i<alreadyCastSpells.Length; i++){
                        if(Vector2.Distance(tPos, alreadyCastSpells[i].transform.position) < 1f){
                            spawnFound = false;
                        }
                    }
                    if(spawnFound){
                        Instantiate(PF_StarDavidSpellOne, tPos, transform.rotation);
                        its=100;
                    }
                    its++;
                }
                mSpellOneTmStmp = Time.time;
            }
            if(Time.time - _spellTwoFireInterval > mSpellTwoTmStmp){
                // Instantiate(PF_StarDavidSpellTwo, rOverseer.rPC.transform.position, transform.rotation);
                mSpellTwoTmStmp = Time.time;
            }
        }


        if(mStageInd == 0){
            FiringMainProjectiles();
        }else if(mStageInd == 1){
            FiringSpells();
        }else if(mStageInd == 2){
            rStar1.gameObject.SetActive(true);
            rStar2.gameObject.SetActive(true);
        }else if(mStageInd == 3){
            rStar3.gameObject.SetActive(true);
        }

    }

    public override void EXIT_PoiseBreak()
    {
        kState = kNormal;
    }

    public void F_Death()
    {
        IMG_HealthBar.fillAmount = 0f;
        rStar1.gameObject.SetActive(false);
        rStar2.gameObject.SetActive(false);
        rStar3.gameObject.SetActive(false);

        rOverseer.FRegisterDeadEnemy(this);
    }

}
