using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Man_Combat : MonoBehaviour
{
    public enum STATE{INTRO, PAUSE, NORMAL, PC_DIED, PLAYER_WON}
    public STATE                    mState = STATE.INTRO;

    public SO_PlayDetails           SO_PlayDetails;

    public bool                     mQuitOnEnemiesDefeated = true;
    [HideInInspector]
    public MAN_Pathing              cPather;
    [HideInInspector]
    public MAN_Helper               cHelper;
    [HideInInspector]
    public MAN_Score                cScore;
    [HideInInspector]
    public MAN_Spawner              cSpawner;
    [HideInInspector]
    public MAN_HUD                  cPCHUD;

    public bool                     mPlayerDied = false;
    public PC_Cont                  rPC;
    public List<Actor>              rActors;
    public List<ENV_SniperSpot>     rSniperSpots;
    public Camera                   rCam;
    public ENV_PracticeSpawn        rPracticeSpawnPoint;

    public UI_CombatIntro           screen_intro;
    public UI_CombatOver            screen_score;
    public UI_CombatPause           screen_pause;
    public Text                     TXT_Scorescreen;
    public Text                     TXT_ScorescreenNewHighest;

    public float                    _minActorSpacing = 1f;

    // Start is called before the first frame update
    public void Start()
    {
        // Program back in the intro blurb later.
        mState = STATE.NORMAL;

        cHelper = GetComponent<MAN_Helper>();
        cHelper.FRUN_Start();
        cPather = GetComponent<MAN_Pathing>();
        cPather.FRUN_Start();
        cScore = GetComponent<MAN_Score>();
        if(cScore != null) cScore.FRUN_Start();
        cSpawner = GetComponent<MAN_Spawner>();
        if(cSpawner!= null) cSpawner.FRUN_Start();
        cPCHUD = GetComponent<MAN_HUD>();
        cPCHUD.FRUN_Start();
        
        rPC = FindObjectOfType<PC_Cont>();
        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.PRACTICE){
            rPC._debugInvinsible = true;
            Instantiate(SO_PlayDetails.PF_Enemy, rPracticeSpawnPoint.transform.position, transform.rotation);
        }

        rActors = FindObjectsOfType<Actor>().ToList();
        foreach(Actor a in rActors){
            a.RUN_Start();
            a.rOverseer = this;
        }
        rSniperSpots = FindObjectsOfType<ENV_SniperSpot>().ToList();

    }

    public void FRegisterDeadEnemy(Actor killedOne)
    {
        if(killedOne == null) return;
        Debug.Log(killedOne + " is telling me that it died.");
        for(int i=0; i<rActors.Count; i++){
            if(killedOne == rActors[i]){
                if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
                    EN_Base b = (EN_Base)killedOne;
                    if(b != null){
                        cScore.mScore += b._arcadeKillScore;
                    }
                }
                Debug.Log("Removing: " + rActors[i]);
                Destroy(rActors[i].gameObject);
                rActors.RemoveAt(i);
                break;
            }
        }
    }

    public void FRUN_Intro()
    {
        
    }
    public void FRUN_Pause()
    {

    }
    public void FRUN_Won()
    {

    }

    public void ENTER_PC_Dead()
    {
        mState = STATE.PC_DIED;
        screen_score.gameObject.SetActive(true);
        Cursor.visible = true;

        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
            cScore.mHighScores.Add(cScore.mScore);
            // If they got a new high score, we tell them that.
            TXT_Scorescreen.text = "Score: " + cScore.mScore;
            if(cScore.FCheckIfScoreIsNewHighest(cScore.mScore)){
                TXT_ScorescreenNewHighest.text = "A new high score!";
            }else{
                TXT_ScorescreenNewHighest.text = "";
            }
        }
    }
    public void FRUN_PC_Dead()
    {
        // We actually don't do anything here for now.
        cHelper.FDeleteProjectilesOutsideArenaBoundaries();
    }
    public void BTN_HitQuit()
    {
        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
            cScore.FSaveScoresToFile();
        }
        SceneManager.LoadScene("SN_MN_Main");
    }
    public void BTN_HitPlay()
    {
        screen_intro.gameObject.SetActive(false);
        screen_score.gameObject.SetActive(false);
        if(SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
            cScore.mTimeStartTmStmp = Time.time;
        }

        mState = STATE.NORMAL;
        Cursor.visible = false;
    }
    public void BTN_PauseContinue()
    {
        Time.timeScale = 1;
        mState = STATE.NORMAL;
        screen_pause.gameObject.SetActive(false);
    }
    public void BTN_PauseQuit()
    {
        SceneManager.LoadScene("SN_MN_Main");
    }

    public void FRUN_Normal()
    {
        if(cScore != null) cScore.FRUN_Update();
        cPather.FRUN_Update();
        if(cSpawner != null) cSpawner.FRUN_Update();

        cHelper.FDeleteProjectilesOutsideArenaBoundaries();

        // Obviously have to handle when the hunters are killed.
        for(int i=0; i<rActors.Count; i++){
            if(rActors[i] == null){
                Debug.Log("Had to remove: " + rActors[i] + " failed to register death earlier.");
                rActors.RemoveAt(i);
                i--;
            }
        }

        if(rActors.Count > 0) {
            for(int i=0; i<rActors.Count; i++){
                // If there is no player, and we're dealing with an enemy, don't run their update.
                if(rPC == null && rActors[i].GetComponent<EN_Base>()){
                    continue;
                }
                rActors[i].RUN_Update();
            }
        }
        // Make sure the actors stay in bounds, as well as outside of any obstacles.
        if(rActors.Count > 0){
            // Y increases with the array. 
            Vector2 botLeft = cHelper.FGetWorldPosOfTile(new Vector2Int(0,0));
            Vector2 topRight = cHelper.FGetWorldPosOfTile(new Vector2Int(15,15));
            for(int i=0; i<rActors.Count; i++){
                Vector2 pos = rActors[i].transform.position;
                if(pos.y > topRight.y) pos.y = topRight.y;
                if(pos.y < botLeft.y) pos.y = botLeft.y;
                if(pos.x < botLeft.x) pos.x = botLeft.x;
                if(pos.x > topRight.x) pos.x = topRight.x;

                rActors[i].transform.position = pos;
            }

            // Hold on a second. All we need to do is move the actor until they are further left, right
            // top, or beneath the tile. Whichever they are overlapping is the way we move.
            for(int i=0; i<rActors.Count; i++){
                // Find out if actor is on invalid tile
                // If so, find the nearest valid tile.
                // Then subtract that position from the invalid tile position
                // Push the actor in that direction until they're free.

                Vector2Int tileActorIsOn = cHelper.FGetTileClosestToSpot(rActors[i].transform.position);
                if(!cPather.mAllTiles[tileActorIsOn.x, tileActorIsOn.y].mTraversable){
                    Vector2 invalidTileCenterPos = cHelper.FGetWorldPosOfTile(tileActorIsOn);
                    Vector2Int closestValidTile = cHelper.FGetTileClosestToSpot(rActors[i].transform.position, true);
                    // Debug.Log("Invalid tile: " + tileActorIsOn + ", closest valid tile: " + closestValidTile);
                    
                    Vector2 validTileCenterPos = cHelper.FGetWorldPosOfTile(closestValidTile);
                    Vector2 vDif = validTileCenterPos - invalidTileCenterPos;
                    Vector2 vDisplacement = vDif / 2f;

                    Vector3 newPos = rActors[i].transform.position;
                    if(vDisplacement.x < 0.1f && vDisplacement.x > -0.1f){
                        newPos.y = invalidTileCenterPos.y; 
                        newPos.y += vDisplacement.y;
                    }else if(vDisplacement.y < 0.1f && vDisplacement.y > -0.1f){
                        newPos.x = invalidTileCenterPos.x;
                        newPos.x += vDisplacement.x;
                    }
                    rActors[i].transform.position = newPos; 
                }
                    
            }

            // Push the non-pc actors away from each other if they get too close.
            for(int i=1; i<rActors.Count; i++){
                if(rActors[i].GetComponent<PC_Cont>() || rActors[i-1].GetComponent<PC_Cont>()) continue;

                if(Vector2.Distance(rActors[i].transform.position, rActors[i-1].transform.position) < _minActorSpacing){
                    if(rActors[i].transform.position == rActors[i-1].transform.position){
                        Vector3 hackNewPos = rActors[i].transform.position; 
                        hackNewPos.x+=0.1f; hackNewPos.y+=0.1f;
                        rActors[i].transform.position = hackNewPos;
                        continue;
                    }
                    Vector2 center = (rActors[i].transform.position + rActors[i-1].transform.position)/2f;
                    Vector2 vDir = (Vector2)(rActors[i].transform.position - rActors[i-1].transform.position).normalized;
                    Vector3 newPos = center + vDir*_minActorSpacing/2f;
                    rActors[i].transform.position = newPos;
                    newPos = center - vDir*_minActorSpacing/2f;
                    rActors[i-1].transform.position = newPos;
                }
            }
        }

        if(rActors.Count <= 1 && mQuitOnEnemiesDefeated && SO_PlayDetails.mMode == SO_PlayDetails.MODE.ARCADE){
            SceneManager.LoadScene("SN_MN_Main");
        }else if(rActors.Count <=1 && SO_PlayDetails.mMode == SO_PlayDetails.MODE.CAMPAIGN){
            // Tell them that they won.
            ENTER_PLAYER_WON();
            return;
        }

        if(Input.GetKeyDown(KeyCode.K)){
            rPC.FHandleDamExternal(45f, DAMAGE_TYPE.SLASH);
        }

        // Have the camera follow the player, for now.
        if(rPC != null){
            Vector3 camPos = rPC.transform.position; camPos.z = -10f;
            rCam.transform.position = camPos;
            cPCHUD.FDrawMouseIconAndTrailAndActiveTarget(rCam, rPC);
            foreach(ENV_SniperSpot s in rSniperSpots){
                s.F_CheckCanSeePlayer(rPC);
            }
        }

        cPCHUD.F_Update(rPC);

        // Check if the player died after updating all the actors.
        if(mPlayerDied){
            // spawn in dead player representation.
            ENTER_PC_Dead();
        }
    }

    public void ENTER_PLAYER_WON()
    {
        mState = STATE.PLAYER_WON;
        screen_score.gameObject.SetActive(true);
        Cursor.visible = true;
        TXT_Scorescreen.text = "You beat: " + cSpawner.mActiveScenario.mName + " scenario!";
    }

    public void Update()
    {
        // Let them quit.
        if(Input.GetKeyDown(KeyCode.M)){
            if(mState == STATE.PAUSE){
                BTN_PauseContinue();
            }else{
                // make pause screen active.
                Time.timeScale = 0;
                screen_pause.gameObject.SetActive(true);
                mState = STATE.PAUSE;
            }
        }

        switch(mState)
        {
            case STATE.INTRO: FRUN_Intro(); break;
            case STATE.PAUSE: FRUN_Pause(); break;
            case STATE.NORMAL: FRUN_Normal(); break;
            case STATE.PC_DIED: FRUN_PC_Dead(); break;
            case STATE.PLAYER_WON: FRUN_Won(); break;
        }
    }

    public void FHandlePlayerDied()
    {
        mPlayerDied = true;
    }
}
