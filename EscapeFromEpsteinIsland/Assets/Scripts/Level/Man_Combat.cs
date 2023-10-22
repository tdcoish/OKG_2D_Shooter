using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Man_Combat : MonoBehaviour
{
    public enum STATE{INTRO, NORMAL, PC_DIED}
    public STATE                    mState = STATE.INTRO;
    public bool                     mSkipTutorialBlurb = true;

    public bool                     mQuitOnEnemiesDefeated = true;
    public Tilemap                  rTilemap;
    [HideInInspector]
    public MAN_Pathing              cPather;
    [HideInInspector]
    public MAN_Helper               cHelper;
    [HideInInspector]
    public MAN_Score                cScore;
    [HideInInspector]
    public MAN_Spawner              cSpawner;

    public ENV_TileRock             PF_TileRockObj;

    public bool                     mPlayerDied = false;
    public PC_Cont                  rPC;
    public List<Actor>              rActors;
    public List<ENV_SniperSpot>     rSniperSpots;
    public Camera                   rCam;
    public MS_Icon                  PF_MouseIcon;
    public MS_Trail                 PF_MouseTrail;
    public TH_Icon                  PF_TrueHeadingIcon;
    public TH_Trail                 PF_TrueHeadingTrail;
    // Want to make the number of trailing icons different. 
    public int                      _mouseTrailNumbers = 5;
    public float                    _minTrailSpacing = 0.25f;
    public UI_HUD                   rHUD;           // background info, not weapon select stuff.
    public GameObject               UI_ActiveTarget;
    public UI_StuckHUD              rStuckHUD;

    public GameObject               screen_intro;
    public GameObject               screen_score;
    public Text                     TXT_Scorescreen;
    public Text                     TXT_ScorescreenNewHighest;

    public float                    _minActorSpacing = 1f;

    // Start is called before the first frame update
    void Start()
    {
        cPather = GetComponent<MAN_Pathing>();
        cPather.FRUN_Start();
        cHelper = GetComponent<MAN_Helper>();
        cHelper.FRUN_Start();
        cScore = GetComponent<MAN_Score>();
        cScore.FRUN_Start();
        cSpawner = GetComponent<MAN_Spawner>();
        cSpawner.FRUN_Start();

        // Ugh. Actually the cells can have negative indices, which makes sense but makes this more complicated.
        rTilemap.CompressBounds();
        cPather.FSetUpPathingTilesAndConnections();

        // Figure out which tiles the start/end nodes correspond to.
        FPlaceTileRockGameObjectsOnRockTiles();
        
        rPC = FindObjectOfType<PC_Cont>();
        rActors = FindObjectsOfType<Actor>().ToList();
        foreach(Actor a in rActors){
            a.RUN_Start();
            a.rOverseer = this;
        }
        rSniperSpots = FindObjectsOfType<ENV_SniperSpot>().ToList();

        rHUD = FindObjectOfType<UI_HUD>();
        if(rHUD == null){
            Debug.Log("No HUD found");
        }
        rStuckHUD = FindObjectOfType<UI_StuckHUD>();
        if(rStuckHUD == null){
            Debug.Log("No stuck HUD found");
        }

        if(mSkipTutorialBlurb){
            mState = STATE.NORMAL;
        }else{
            // Set ms as locked. Create mouse icon.
            // Cursor.lockState = CursorLockMode.Confined;
            screen_intro.SetActive(true);
            mState = STATE.INTRO;
        }

    }

    // Have to figure out which areas are rocks, and spawn in appropriate gameobjects with collision boxes.
    public void FPlaceTileRockGameObjectsOnRockTiles()
    {
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(!cPather.mPathingTiles[x,y].mCanPath){
                    Vector2 pos = cHelper.FGetWorldPosOfTile(new Vector2Int(x,y));
                    Instantiate(PF_TileRockObj, pos, transform.rotation);
                }
            }
        }

    }

    public void FRegisterDeadEnemy(Actor killedOne)
    {
        if(killedOne == null) return;
        Debug.Log(killedOne + " is telling me that it died.");
        for(int i=0; i<rActors.Count; i++){
            if(killedOne == rActors[i]){
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

    public void ENTER_PC_Dead()
    {
        mState = STATE.PC_DIED;
        screen_score.SetActive(true);
        TXT_Scorescreen.text = "Score: " + cScore.mScore;
        Cursor.visible = true;
        cScore.mHighScores.Add(cScore.mScore);

        // If they got a new high score, we tell them that.
        if(cScore.FCheckIfScoreIsNewHighest(cScore.mScore)){
            TXT_ScorescreenNewHighest.text = "A new high score!";
        }else{
            TXT_ScorescreenNewHighest.text = "";
        }
    }
    public void FRUN_PC_Dead()
    {
        // We actually don't do anything here for now.
    }
    public void BTN_HitQuit()
    {
        cScore.FSaveScoresToFile();
        SceneManager.LoadScene("SN_MN_Main");
    }
    public void BTN_HitPlay()
    {
        screen_intro.SetActive(false);
        screen_score.SetActive(false);
        cScore.mTimeStartTmStmp = Time.time;

        mState = STATE.NORMAL;
        Cursor.visible = false;
    }

    public void FRUN_Normal()
    {
        cScore.FRUN_Update();
        cPather.FRUN_Update();
        cSpawner.FRUN_Update();

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
                if(!cPather.mPathingTiles[tileActorIsOn.x, tileActorIsOn.y].mCanPath){
                    Vector2 invalidTileCenterPos = cHelper.FGetWorldPosOfTile(tileActorIsOn);
                    Vector2Int closestValidTile = cHelper.FGetTileClosestToSpot(rActors[i].transform.position, true);
                    Debug.Log("Invalid tile: " + tileActorIsOn + ", closest valid tile: " + closestValidTile);
                    
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

        if(rActors.Count <= 1 && mQuitOnEnemiesDefeated){
            SceneManager.LoadScene("SN_MN_Main");
        }

        if(Input.GetKeyDown(KeyCode.K)){
            rPC.FHandleDamExternal(45f, DAMAGE_TYPE.SLASH);
        }

        // Have the camera follow the player, for now.
        if(rPC != null){
            Vector3 camPos = rPC.transform.position; camPos.z = -10f;
            rCam.transform.position = camPos;
            FDrawMouseIconAndTrailAndActiveTarget();
            foreach(ENV_SniperSpot s in rSniperSpots){
                s.F_CheckCanSeePlayer(rPC);
            }
        }

        if(rHUD != null){
            if(rPC != null){
                rHUD.FillPCHealthAndShields(rPC.cHpShlds.mHealth.mAmt, rPC.cHpShlds.mHealth._max, rPC.cHpShlds.mShields.mStrength, rPC.cHpShlds.mShields._max);
                rHUD.FillPCStaminaAmount(rPC.mCurStamina, rPC._staminaMax);
                rHUD.FillWeaponOverheatAmounts(rPC);
            }
        }
        if(rStuckHUD != null){
            if(rPC != null){
                rStuckHUD.transform.position = rPC.transform.position;
                rStuckHUD.FillBars(rPC);
            }
        }

        // Check if the player died after updating all the actors.
        if(mPlayerDied){
            // spawn in dead player representation.
            ENTER_PC_Dead();
        }
    }

    void Update()
    {
        // Let them quit.
        if(Input.GetKeyDown(KeyCode.M)){
            SceneManager.LoadScene("SN_MN_Main");
        }

        switch(mState)
        {
            case STATE.INTRO: FRUN_Intro(); break;
            case STATE.NORMAL: FRUN_Normal(); break;
            case STATE.PC_DIED: FRUN_PC_Dead(); break;
        }
    }

    // Don't draw the mouse when we're switching targets.
    public void FDrawMouseIconAndTrailAndActiveTarget()
    {
        MS_Icon[] icons = FindObjectsOfType<MS_Icon>();
        for(int i=0; i<icons.Length; i++){
            Destroy(icons[i].gameObject);
        }
        MS_Trail[] trails = FindObjectsOfType<MS_Trail>();
        for(int i=0; i<trails.Length; i++){
            Destroy(trails[i].gameObject);
        }
        TH_Icon[] th_icons = FindObjectsOfType<TH_Icon>();
        for(int i=0; i<th_icons.Length; i++){
            Destroy(th_icons[i].gameObject);
        }
        TH_Trail[] th_trails = FindObjectsOfType<TH_Trail>();
        for(int i=0; i<th_trails.Length; i++){
            Destroy(th_trails[i].gameObject);
        }

        Vector2 msPos = rCam.ScreenToWorldPoint(Input.mousePosition);
        Instantiate(PF_MouseIcon, msPos, transform.rotation);
        Vector2 vDir = (msPos - (Vector2)rPC.transform.position).normalized;
        Vector2 trailPos = rPC.transform.position;
        float dis = Vector2.Distance(msPos, rPC.transform.position);
        float spacing = dis / _mouseTrailNumbers;
        for(int i=0; i<_mouseTrailNumbers; i++){
            trailPos = (Vector2)rPC.transform.position + (spacing * i * vDir);
            Instantiate(PF_MouseTrail, trailPos, transform.rotation);
        }

        // Now draw the true heading spot.
        PC_Heading h = rPC.GetComponent<PC_Heading>();
        Instantiate(PF_TrueHeadingIcon, h.mCurHeadingSpot, transform.rotation);
        // Draw those trails.
        vDir = (h.mCurHeadingSpot - (Vector2)rPC.transform.position).normalized;
        Vector2 th_trailPos = rPC.transform.position;
        dis = Vector2.Distance(h.mCurHeadingSpot, rPC.transform.position);
        spacing = dis / _mouseTrailNumbers;
        for(int i=0; i<_mouseTrailNumbers; i++){
            th_trailPos = (Vector2)rPC.transform.position + (spacing * i * vDir);
            Instantiate(PF_MouseTrail, th_trailPos, transform.rotation);
        }

        if(!rPC.mHasActiveTarget){
            UI_ActiveTarget.gameObject.SetActive(false);           
        }else{
            UI_ActiveTarget.gameObject.SetActive(true);
            UI_ActiveTarget.transform.position = rPC.rCurTarget.transform.position;
        }

    }

    public void FHandlePlayerDied()
    {
        mPlayerDied = true;
    }
}
