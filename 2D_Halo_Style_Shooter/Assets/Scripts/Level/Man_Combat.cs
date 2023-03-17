using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Man_Combat : MonoBehaviour
{
    public Tilemap                  rTilemap;

    // Start is called before the first frame update
    void Start()
    {
        // Ugh. Actually the cells can have negative indices, which makes sense but makes this more complicated.



        rTilemap.CompressBounds();
        BoundsInt bounds = rTilemap.cellBounds;
        Debug.Log(bounds);
        TileBase[] allTiles = rTilemap.GetTilesBlock(rTilemap.cellBounds);

        for(int x=bounds.x; x<(bounds.x + bounds.size.x); x++){
            for(int y=bounds.y; y<(bounds.y + bounds.size.y); y++){
                for(int z=bounds.z; z<(bounds.z + bounds.size.z); z++){
                    TileBase tile = rTilemap.GetTile(new Vector3Int(x,y,z));
                    if(tile){
                        Debug.Log(tile.name + " at: " + x + "," + y);
                    }else{
                        Debug.Log("Nulld");
                    }
                }
            }
        }

        // t.GetSprite(0,0);

    }
}
