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
    public Camera                   rCam;
    public MS_Icon                  PF_MouseIcon;
    public MS_Trail                 PF_MouseTrail;
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

            // has a weird bug where it doesn't handle the seams very well.
            for(int i=0; i<rActors.Count; i++){
                // Find out if actor is on invalid tile
                // Find direction from center of invalid tile
                // Change that direction to either pure up/down/left/right
                // Find the size of the tiles
                // Shift the character the distance of the tile size in the up/down/left/right direction.
                // Check that actor is not on another invalid tile.
                // If so, keep pushing them out further and further.

                Vector2Int tileActorIsOn = cHelper.FGetTileClosestToSpot(rActors[i].transform.position);
                if(!cPather.mPathingTiles[tileActorIsOn.x, tileActorIsOn.y].mCanPath){
                    // Now we need to find the nearest valid tile that can path.
                    Debug.Log("Actor: " + rActors[i] + " is on an invalid tile.");
                    // There should always be one, provided that the actor is not spawned or teleported into the stones.
                    Vector2 invalidTileCenterPos = cHelper.FGetWorldPosOfTile(tileActorIsOn);
                    Vector2 vDir = (Vector2)(rActors[i].transform.position - (Vector3)invalidTileCenterPos);
                    vDir = vDir.normalized;
                    Vector2 vCardinalDir = Vector2.up;
                    float largestDot = Vector3.Dot(Vector2.up, vDir);
                    float tempDot = Vector3.Dot(Vector2.down, vDir);
                    if(tempDot > largestDot){
                        vCardinalDir = Vector2.down;  
                        largestDot = tempDot;
                        Debug.Log("Move down");
                    } 
                    tempDot = Vector3.Dot(Vector2.right, vDir);
                    if(tempDot > largestDot){
                        vCardinalDir = Vector2.right;
                        largestDot = tempDot;
                        Debug.Log("Move right");
                    }
                    tempDot = Vector3.Dot(Vector2.left, vDir);
                    if(tempDot > largestDot){
                        vCardinalDir = Vector2.left;
                        largestDot = tempDot;
                        Debug.Log("Move left");
                    }

                    float tileSize = Vector3.Distance(cHelper.FGetWorldPosOfTile(new Vector2Int(0,0)), cHelper.FGetWorldPosOfTile(new Vector2Int(0,1)));
                    Vector3 newPos = rActors[i].transform.position;
                    bool actorHasEscapedWall = false;
                    int iterations = 1;
                    while(!actorHasEscapedWall)
                    {
                        if(vCardinalDir.x < 0.1f && vCardinalDir.x > -0.1f){
                            newPos.y = invalidTileCenterPos.y + (vCardinalDir.y * tileSize/10f * (float)iterations);
                        }else if (vCardinalDir.y < 0.1f && vCardinalDir.y > -0.1f){
                            newPos.x = invalidTileCenterPos.x + (vCardinalDir.x * tileSize/10f * (float)iterations);
                        }
                        rActors[i].transform.position = newPos;

                        tileActorIsOn = cHelper.FGetTileClosestToSpot(rActors[i].transform.position);
                        if(cPather.mPathingTiles[tileActorIsOn.x, tileActorIsOn.y].mCanPath){
                            actorHasEscapedWall = true;
                        }
                        iterations++;

                        if(iterations > 100){
                            Debug.Log("ending it here.");
                            actorHasEscapedWall = true;
                        }
                    }


                    rActors[i].transform.position = newPos;
                    
                    tileActorIsOn = cHelper.FGetTileClosestToSpot(rActors[i].transform.position);
                    if(!cPather.mPathingTiles[tileActorIsOn.x, tileActorIsOn.y].mCanPath){

                    }

                    // Vector2 dest = cHelper.FGetWorldPosOfTile(closestPathableTile);
                    // // Now we keep moving them a tiny bit in that direction until they get kicked out.
                    // bool kickedOutOfWall = false;
                    // Vector3 vDir = (rActors[i].transform.position - (Vector3)dest).normalized;
                    // while(!kickedOutOfWall){
                    //     rActors[i].transform.position = rActors[i].transform.position + vDir*0.1f;
                    //     tileActorIsOn = cHelper.FGetTileClosestToSpot(rActors[i].transform.position);
                    //     if(cPather.mPathingTiles[tileActorIsOn.x, tileActorIsOn.y].mCanPath){
                    //         kickedOutOfWall = true;
                    //     }
                    // }
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
