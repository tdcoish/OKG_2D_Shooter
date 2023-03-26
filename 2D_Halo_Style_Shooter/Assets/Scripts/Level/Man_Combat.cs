using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Man_Combat : MonoBehaviour
{
    public Tilemap                  rTilemap;
    [HideInInspector]
    public MAN_Pathing              cPather;
    [HideInInspector]
    public MAN_Helper               cHelper;

    public ENV_TileRock             PF_TileRockObj;

    public PC_Cont                  rPC;
    public List<EN_Hunter>          rHunters;
    public List<EN_Knight>          rKnights;

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
        rHunters = FindObjectsOfType<EN_Hunter>().ToList();
        rKnights = FindObjectsOfType<EN_Knight>().ToList();
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

    void Update()
    {
        cPather.FRUN_Update();

        // Obviously have to handle when the hunters are killed.
        for(int i=0; i<rHunters.Count; i++){
            if(rHunters[i] == null){
                rHunters.RemoveAt(i);
                i--;
            }
        }

        if(rHunters.Count > 0) {
            foreach(EN_Hunter h in rHunters){
                h.FRUN_Update(cPather);
            }
        }

        if(rKnights.Count > 0){
            foreach(EN_Knight k in rKnights){
                k.FUpdate();
            }
        }
    }

}
