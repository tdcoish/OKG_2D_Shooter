using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathingTile
{
    public PathingTile(bool canPath = false){
        mCanPath = canPath;
        mConnections = new List<Vector2Int>();
        mPrevNodeOnPath = new Vector2Int();
        mScore = 10000f;
        mHeuristicDistance = 0f;
        mVisited = false;
    }
    public bool                     mCanPath = false;
    public bool                     mVisited = false;
    public List<Vector2Int>         mConnections;
    public Vector2Int               mPrevNodeOnPath;
    public float                    mScore;
    public float                    mHeuristicDistance;
}

public class Man_Combat : MonoBehaviour
{
    public Tilemap                  rTilemap;

    public MSC_SquareMarker         PF_Red1;
    public MSC_SquareMarker         PF_Green2;
    public MSC_SquareMarker         PF_Blue3;
    public MSC_SquareMarker         PF_Purple4;
    public MSC_SquareMarker         PF_Yellow5;

    public MSC_SquareMarker         rStartNode;
    public MSC_SquareMarker         rEndNode;

    public TEST_BasicEnemy          rEnemy;
    public bool                     mPathChar = false;
    public List<Vector2Int>         mPath;

    public PathingTile[,]           mPathingTiles;

    public int                      _timeTestRepeatNum = 1000;

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

        FMakeWalkableTilesFormConnections();

        // Figure out which tiles the start/end nodes correspond to.
    }


    Vector2Int FGetTileClosestToSpot(Vector2 posOfObj)
    {
        BoundsInt bounds = rTilemap.cellBounds;

        for(int x=bounds.x; x<(bounds.x + bounds.size.x); x++){
            for(int y=bounds.y; y<(bounds.y + bounds.size.y); y++){
                Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x, y, 0));

                if(Vector2.Distance(posOfObj, tileWorldPos) < 1f){
                    return new Vector2Int(x - bounds.x, y - bounds.y);
                }
            }
        }

        return new Vector2Int(-1,-1);
    }

   void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)){
            FShowWalkableTiles();
        }
        if(Input.GetKeyDown(KeyCode.J)){
            //FMakeWalkableTilesFormConnections();
        }
        if(Input.GetKeyDown(KeyCode.I)){
            FDrawConnections();
        }

        // replace start.
        if(Input.GetMouseButtonDown(0)){
            FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.PURPLE);
            FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.BLUE);
            Camera c = Camera.main;
            Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);
            rStartNode = Instantiate(PF_Purple4, msPos, transform.rotation);
            Vector2Int startNode = FGetTileClosestToSpot(rStartNode.transform.position);
            Vector2Int endNode = FGetTileClosestToSpot(rEndNode.transform.position);
            FDrawPath(FCalcPath(startNode, endNode));

            rEnemy.transform.position = msPos;
        }
        // replace finish.
        if(Input.GetMouseButtonDown(1)){
            FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.YELLOW);
            FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.BLUE);
            Camera c = Camera.main;
            Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);
            rEndNode = Instantiate(PF_Yellow5, msPos, transform.rotation);
            Vector2Int startNode = FGetTileClosestToSpot(rStartNode.transform.position);
            Vector2Int endNode = FGetTileClosestToSpot(rEndNode.transform.position);
            List<Vector2Int> path = FCalcPath(startNode, endNode);
            FDrawPath(path);

            // make the enemy move.
            mPath = new List<Vector2Int>(path);
            mPathChar = true;
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            DateTime stTm = DateTime.Now;
            Vector2Int startNode = FGetTileClosestToSpot(rStartNode.transform.position);
            Vector2Int endNode = FGetTileClosestToSpot(rEndNode.transform.position);
            for(int i=0; i<_timeTestRepeatNum; i++){
                FCalcPath(startNode, endNode);
            }
            DateTime endTm = DateTime.Now;
            Debug.Log("Time taken: " + (endTm - stTm));
        }

        if(mPathChar){
            RUN_MoveCharacter();
        }
    }

    void RUN_MoveCharacter()
    {
        if(mPath.Count == 0){
            Debug.Log("Done pathing");
            mPathChar = false;
            return;
        }

        Vector2 GetWorldPosOfTile(Vector2Int indice)
        {
            if(indice.x < 0 || indice.x > 16 || indice.y < 0 || indice.y > 16){
                Debug.Log("Path node not in field of play");
                return new Vector2();
            }
            BoundsInt bounds = rTilemap.cellBounds;
            Vector2 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(indice.x + bounds.x, indice.y + bounds.y, 0));
            tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
            return tileWorldPos;
        }

        Vector2 curDestNode = GetWorldPosOfTile(mPath[0]);
        Vector2 dif = curDestNode - (Vector2)rEnemy.transform.position;
        rEnemy.cRigid.velocity = dif.normalized * rEnemy._movSpd;

        float dis = Vector2.Distance(rEnemy.transform.position, curDestNode);
        if(dis < 0.1f){
            Debug.Log("Hit node, moving on");
            mPath.RemoveAt(0);
            if(mPath.Count == 0){
                Debug.Log("Hit end. Path followed.");
                mPathChar = false;
                rEnemy.cRigid.velocity = Vector2.zero;
            }
        }


    }

    public void FClearAllMarkers()
    {
        MSC_SquareMarker[] markers = FindObjectsOfType<MSC_SquareMarker>();
        foreach(MSC_SquareMarker m in markers){
            Destroy(m.gameObject);
        }
    }

    public void FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL level)
    {
        MSC_SquareMarker[] markers = FindObjectsOfType<MSC_SquareMarker>();
        foreach(MSC_SquareMarker m in markers){
            if(m._level == level) Destroy(m.gameObject);
        }
    }

    public void FShowWalkableTiles()
    {
        // FClearAllMarkers();
        FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.RED);
        FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.GREEN);

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

    public void FDrawPath(List<Vector2Int> path)
    {
        if(path == null){
            Debug.Log("Path is null, no drawing");
            return;
        }

        BoundsInt bounds = rTilemap.cellBounds;
        for(int i=0; i<path.Count; i++){
            Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(path[i].x + bounds.x, path[i].y + bounds.y, 0));
            tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
            Instantiate(PF_Blue3, tileWorldPos, transform.rotation);
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

                    Debug.DrawLine(tileWorldPos, destTileWorldPos, Color.cyan, 60f);
                }
            }
        }
    }

    public List<Vector2Int> FCalcPath(Vector2Int startNode, Vector2Int endNode)
    {
        if(!mPathingTiles[startNode.x,startNode.y].mCanPath){
            Debug.Log("Path start tile can't path");
            return null;
        }
        if(!mPathingTiles[endNode.x,endNode.y].mCanPath){
            Debug.Log("Path end tile can't path");
            return null;
        }

        // prep the nodes and add heuristic distance.
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // Use 10000f as a good "haven't been here" distance.
                mPathingTiles[x,y].mScore = 10000f;
                Vector2Int dif = new Vector2Int(Mathf.Abs(x - endNode.x), Mathf.Abs(y - endNode.y));
                float dis = Mathf.Sqrt(dif.x*dif.x + dif.y*dif.y);
                mPathingTiles[x,y].mHeuristicDistance = dis;
                mPathingTiles[x,y].mPrevNodeOnPath = new Vector2Int(-1,-1);     // intentionally using an illegal index.
                mPathingTiles[x,y].mVisited = false;
            }
        }

        // To start, set the starting node score to 0.
        // I actually never need Vector2.Distance stuff or real world position stuff. Using indexes we can figure out distances.
        mPathingTiles[startNode.x, startNode.y].mScore = 0;
        Vector2Int          activeInd = new Vector2Int();

        // Repeat until at destination node. 
        int                 iterations = 0;
        bool                foundPath = false;
        while(!foundPath || iterations < 256)
        {
            float               lowestScore = 10000000f;
            // Iterate through all the nodes that haven't been exhausted
            // pick the one with the lowest score + heuristic.
            for(int x=0; x<16; x++){
                for(int y=0; y<16; y++){
                    if(mPathingTiles[x,y].mVisited) continue;

                    float combinedScore = mPathingTiles[x,y].mHeuristicDistance + mPathingTiles[x,y].mScore;
                    if(combinedScore < lowestScore){
                        lowestScore = combinedScore;
                        activeInd = new Vector2Int(x,y);
                    }
                }
            }
            mPathingTiles[activeInd.x, activeInd.y].mVisited = true;
            // If we're on the end node just end here.
            if(activeInd == endNode){
                foundPath = true;
                List<Vector2Int> path = new List<Vector2Int>();
                path.Add(activeInd);
                bool gotToStart = false;
                Vector2Int workingInd = activeInd;
                while(!gotToStart){
                    path.Add(mPathingTiles[workingInd.x, workingInd.y].mPrevNodeOnPath);
                    workingInd = mPathingTiles[workingInd.x, workingInd.y].mPrevNodeOnPath;
                    if(workingInd == startNode) gotToStart = true;
                }
                path.Reverse();
                return path;
            }

            // Update its connections.
            for(int i=0; i<mPathingTiles[activeInd.x, activeInd.y].mConnections.Count; i++){
                Vector2Int destNodeIndice = mPathingTiles[activeInd.x, activeInd.y].mConnections[i];
                Vector2Int dif = new Vector2Int(Mathf.Abs(destNodeIndice.x - activeInd.x), Mathf.Abs(destNodeIndice.y - activeInd.y));
                float dis = Mathf.Sqrt(dif.x*dif.x + dif.y*dif.y);
                float totalScoreThusFar = dis + mPathingTiles[activeInd.x, activeInd.y].mScore;
                if(totalScoreThusFar < mPathingTiles[destNodeIndice.x, destNodeIndice.y].mScore){
                    mPathingTiles[destNodeIndice.x, destNodeIndice.y].mScore = totalScoreThusFar;
                    mPathingTiles[destNodeIndice.x, destNodeIndice.y].mPrevNodeOnPath = activeInd;
                }
            }

            iterations++;
            if(iterations >= 255){
                Debug.Log("Hit max iterations. No more");
            }
        }
        Debug.Log("Error making path");
        return null;

    }

}
