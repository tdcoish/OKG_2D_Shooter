using UnityEngine;
using UnityEngine.Tilemaps;


public class MAN_GridSetup : MonoBehaviour
{
    public Tilemap                  rTilemap;
    public ENV_TileRock             PF_TileRockObj;

    public void FRUN_Start(MAN_Helper helper, MAN_Pathing pather)
    {
        // Ugh. Actually the cells can have negative indices, which makes sense but makes this more complicated.
        rTilemap.CompressBounds();
        pather.FFigureOutWhichTilesAreNonPathable();
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(!pather.mAllTiles[x,y].mTraversable){
                    Vector2 pos = helper.FGetWorldPosOfTile(new Vector2Int(x,y));
                    Instantiate(PF_TileRockObj, pos, transform.rotation);
                }
            }
        }

        pather.FFindPathingTilesDiagonalFromCornersOfBlocks();
        // cPather.FDrawLinesBetweenValidConnections();
    }
}
