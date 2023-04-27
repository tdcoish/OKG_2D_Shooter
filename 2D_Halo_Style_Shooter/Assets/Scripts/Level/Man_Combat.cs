using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class Man_Combat : MonoBehaviour
{
    public bool                     mQuitOnEnemiesDefeated = true;
    public Tilemap                  rTilemap;
    [HideInInspector]
    public MAN_Pathing              cPather;
    [HideInInspector]
    public MAN_Helper               cHelper;

    public ENV_TileRock             PF_TileRockObj;

    public PC_Cont                  rPC;
    public List<Actor>              rActors;
    public Camera                   rCam;
    public MS_Icon                  PF_MouseIcon;
    public MS_Trail                 PF_MouseTrail;
    public float                    _mouseTrailSpacing = 1f;
    public UI_HUD                   rHUD;           // background info, not weapon select stuff.

    // Start is called before the first frame update
    void Start()
    {
        cPather = GetComponent<MAN_Pathing>();
        cPather.FRUN_Start();
        cHelper = GetComponent<MAN_Helper>();
        cHelper.FRUN_Start();

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

        // Set ms as locked. Create mouse icon.
        // Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
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

    void Update()
    {
        // Let them quit.
        if(Input.GetKeyDown(KeyCode.Escape)){
            SceneManager.LoadScene("SN_MN_Main");
        }

        cPather.FRUN_Update();

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
        // Make sure the actors stay in bounds.
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
        }

        if(rActors.Count <= 1 && mQuitOnEnemiesDefeated){
            SceneManager.LoadScene("SN_MN_Main");
        }

        if(Input.GetKeyDown(KeyCode.K)){
            rPC.FHandleDamExternal(45f, DAMAGE_TYPE.SLASH);
        }

        // Have the camera follow the player, for now.
        Vector3 camPos = rPC.transform.position; camPos.z = -10f;
        rCam.transform.position = camPos;
        FDrawMouseIconAndTrail();

        
        if(rHUD != null){
            if(rPC != null){
                rHUD.FillPCHealthAndShields(rPC.cHpShlds.mHealth.mAmt, rPC.cHpShlds.mHealth._max, rPC.cHpShlds.mShields.mStrength, rPC.cHpShlds.mShields._max);
                rHUD.FillPCManaAmount(rPC.mCurEnergy, rPC._energyMax);
                rHUD.FillPCStaminaAmount(rPC.mCurStamina, rPC._staminaMax);
            }
        }
    }

    public void FDrawMouseIconAndTrail()
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
        int iterations = 1;
        bool pastCursorPos = false;
        while(!pastCursorPos){
            trailPos = (Vector2)rPC.transform.position + (_mouseTrailSpacing * iterations * vDir);
            if(Vector2.Distance(trailPos, rPC.transform.position) > Vector2.Distance(msPos, rPC.transform.position)){
                break;
            }
            Instantiate(PF_MouseTrail, trailPos, transform.rotation);
            iterations++;
        }
    }

    public void FHandlePlayerDied()
    {
        SceneManager.LoadScene("SN_MN_Main");
    }

}
