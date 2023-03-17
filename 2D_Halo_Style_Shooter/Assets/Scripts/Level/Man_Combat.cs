using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathingTile
{
    public PathingTile(bool canPath = false){
        mCanPath = canPath;
        mConnections = new List<Vector2Int>();
    }
    public bool                     mCanPath = false;
    public List<Vector2Int>         mConnections;
}

public class Man_Combat : MonoBehaviour
{
    public Tilemap                  rTilemap;

    public MSC_SquareMarker         PF_Red1;
    public MSC_SquareMarker         PF_Green2;
    public MSC_SquareMarker         PF_Blue3;
    public MSC_SquareMarker         PF_Purple4;
    public MSC_SquareMarker         PF_Yellow5;


    public PathingTile[,]           mPathingTiles;

    // Start is called before the first frame update
    void Start()
    {
        // Ugh. Actually the cells can have negative indices, which makes sense but makes this more complicated.
        rTilemap.CompressBounds();

        BoundsInt bounds = rTilemap.cellBounds;
        Debug.Log(bounds);
        mPathingTiles = new PathingTile[bounds.size.x, bounds.size.y];
        for(int x=0;x<bounds.size.x; x++){
            for(int y=0; y<bounds.size.y; y++){
                mPathingTiles[x,y] = new PathingTile();
            }
        }
        mPathingTiles[0,0].mCanPath = false;

        TileBase[] allTiles = rTilemap.GetTilesBlock(rTilemap.cellBounds);

        for(int x=bounds.x; x<(bounds.x + bounds.size.x); x++){
            for(int y=bounds.y; y<(bounds.y + bounds.size.y); y++){
                for(int z=bounds.z; z<(bounds.z + bounds.size.z); z++){
                    TileBase tile = rTilemap.GetTile(new Vector3Int(x,y,z));
                    if(tile){
                        if(tile.ToString().Contains("Castle")){
                            mPathingTiles[x - bounds.x, y - bounds.y].mCanPath = false;
                        }else{
                            mPathingTiles[x - bounds.x, y - bounds.y].mCanPath = true;
                        }
                    }else{
                        Debug.Log("Nulld");
                    }

                    // Debug.Log("Three vals: " + new Vector3(x,y,z));
                }
            }
        }

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            FShowWalkableTiles();
        }
        if(Input.GetKeyDown(KeyCode.J)){
            FMakeWalkableTilesFormConnections();
        }
        if(Input.GetKeyDown(KeyCode.I)){
            FDrawConnections();
        }
    }

    public void FShowWalkableTiles()
    {
        BoundsInt bounds = rTilemap.cellBounds;

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(mPathingTiles[x,y].mCanPath){
                    // on the right tile.
                    Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                    tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
                    Instantiate(PF_Green2, tileWorldPos, transform.rotation);
                }else{
                    Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                    tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
                    Instantiate(PF_Red1, tileWorldPos, transform.rotation);
                }
            }
        }
    }

    // Now we make all the walkable tiles connect to each other.
    public void FMakeWalkableTilesFormConnections()
    {
        bool InBounds(Vector2Int loc, int xMin = 0, int xMax = 15, int yMin = 0, int yMax = 15){
            if(loc.x < xMin) return false;
            if(loc.x > xMax) return false;
            if(loc.y < yMin) return false;
            if(loc.y > yMax) return false;
            return true;
        }

        void CheckAddConnection(Vector2Int origNode, Vector2Int destNode)
        {
            if(InBounds(destNode)){
                if(mPathingTiles[destNode.x, destNode.y].mCanPath){
                    if(mPathingTiles[destNode.x, destNode.y].mCanPath){
                        mPathingTiles[origNode.x,origNode.y].mConnections.Add(destNode);
                    }
                }
            }
        }

        void CheckAddDiagonalConnection(Vector2Int origNode, Vector2Int skip)
        {
            Vector2Int dest = origNode + skip;
            if(!InBounds(dest)) return;
            if(!mPathingTiles[dest.x, dest.y].mCanPath) return;

            if(mPathingTiles[origNode.x, origNode.y].mConnections.Contains(new Vector2Int(origNode.x + skip.x, origNode.y))){
                if(mPathingTiles[origNode.x, origNode.y].mConnections.Contains(new Vector2Int(origNode.x, origNode.y + skip.y))){
                    mPathingTiles[origNode.x, origNode.y].mConnections.Add(dest);
                }
            }
        }

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(!mPathingTiles[x,y].mCanPath) continue;
                
                CheckAddConnection(new Vector2Int(x,y), new Vector2Int(x-1,y));
                CheckAddConnection(new Vector2Int(x,y), new Vector2Int(x+1,y));
                CheckAddConnection(new Vector2Int(x,y), new Vector2Int(x,y-1));
                CheckAddConnection(new Vector2Int(x,y), new Vector2Int(x,y+1));

                // Now we have to check the diagonals.
                CheckAddDiagonalConnection(new Vector2Int(x,y), new Vector2Int(1,1));
                CheckAddDiagonalConnection(new Vector2Int(x,y), new Vector2Int(1,-1));
                CheckAddDiagonalConnection(new Vector2Int(x,y), new Vector2Int(-1,1));
                CheckAddDiagonalConnection(new Vector2Int(x,y), new Vector2Int(-1,-1));

                Debug.Log("Connections: " + mPathingTiles[x,y].mConnections.Count);
            }
        }
    }

    public void FDrawConnections()
    {
        BoundsInt bounds = rTilemap.cellBounds;

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                for(int j = 0; j<mPathingTiles[x,y].mConnections.Count; j++){
                    Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                    tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
                    Vector2Int destTile = mPathingTiles[x,y].mConnections[j];
                    Vector3 destTileWorldPos = rTilemap.CellToWorld(new Vector3Int(destTile.x + bounds.x, destTile.y + bounds.y, 0));
                    destTileWorldPos.x += 0.5f; destTileWorldPos.y += 0.5f;

                    // Debug.DrawLine(tileWorldPos, destTileWorldPos, Color.cyan, 60f);
                }
            }
        }
    }

}
