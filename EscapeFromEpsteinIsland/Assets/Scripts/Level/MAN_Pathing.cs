using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

/****************************************************************************************************************************************
Total aside, but cover works best with weapons that can be charged, and weapons that need reloading.
Scope creep is real, and this game should be as simple as possible, but I think the best 2D Halo style 
game has reloading weapons, weapon overheating, and weapon charging, albeit with drastically reduced shield
start recharge times. 
Also, I need flusher enemies. Could steal Flood ability to jump around the map, but something like a fast
moving Brute, that can't easily be stunned, and charges the player also works. Also, make the cover smaller
I want the player weaving in and out of cover, or having partial cover only from one direction. The smaller
the cover, the better.
Also, add in something like the carbine rifle, with an enemy that has a pause before every firing burst, 
as long as we also add in tons of small cover areas for the player to weave in between. 

https://www.youtube.com/watch?v=mxbVfa8HJg8

https://www.youtube.com/watch?v=dQ2k3cS_pZg

Yet more ideas for weapons. Burst damage low DPS, high DPS low burst, poison weapon that does slow, but 
consistent damage and 1 hit KOs the enemy (after a long while), AoE burst, AoE sustain, bounce around 
walls attack (like flak cannon, or frag grenade), line piercing attack (sniper, slide attack), longer
and shorter attacks (like close range flamethrower made from axe body spray and lighter), controlled
AoE burst (shock rifle alt fire UT), life leech, automatic turret, homing around walls (so you can spray
it in cover), combine burst (needler), clip based vs overheat based, Li Ming orb, charge up weapon, 
increasing rate of fire to encourage staying out, 

Enemy that fires gas mortar that lingers in the area. Forces player out of the same cover.
Enemy that fires laser line that lingers for a few seconds. Perhaps multiple lines.

Oooooh boy, am I simultaneously looking forward to, and not looking forward to updating this.

Let's start just by marking the tiles that are corner to blocked areas. 
Done.
Now, we need to change the way the pathing system paths.

The solution is very simple. Every traversable tile contains a list of all the pathing tiles that it can
see. We use this to complete the circuit when pathfinding. 
When pathing, we figure out what square we're starting on. This square adds itself to the pathing tiles that
it can see. Similarly, the end time that we finish on adds itself to all the pathing tiles that it can
see. 
We then crank through the A* algorithm, finding the shortest path.
When done, we remove any traversable tiles from the mConnections in the pathing tile.
****************************************************************************************************************************************/

public class NodeAndDistance
{
    public NodeAndDistance(Vector2Int nodeIndice, float distanceFromUs)
    {
        mNode = nodeIndice;
        mDis = distanceFromUs;
    }
    public Vector2Int               mNode;
    public float                    mDis;
}

public class PathingTile
{
    public PathingTile(bool traversable = false){
        mTraversable = traversable;
        mPathing = false;
        mConnections = new List<NodeAndDistance>();
        mPrevNodeOnPath = new Vector2Int();
        mScore = 10000f;
        mHeuristicDistance = 0f;
        mVisited = false;
    }
    public bool                     mTraversable = false;
    public bool                     mPathing = false;
    public bool                     mVisited = false;
    public List<NodeAndDistance>    mConnections;           // Connections only to pathing tiles
    public Vector2Int               mPrevNodeOnPath;
    public float                    mScore;
    public float                    mHeuristicDistance;
    public bool                     mCanSeePlayer = false;
}

public class MAN_Pathing : MonoBehaviour
{
    [HideInInspector]
    public Man_Combat               cCombat;
    [HideInInspector]
    public MAN_Helper               cHelper;

    public Tilemap                  rTilemap;
    public ENV_TileRock             PF_TileRockObj;

    public MSC_SquareMarker         rStartNode;
    public MSC_SquareMarker         rEndNode;

    public TEST_BasicEnemy          rEnemy;
    public bool                     mPathChar = false;
    public List<Vector2Int>         mPath;

    public PathingTile[,]           mAllTiles;
    public List<Vector2Int>         mPathNodeTiles;

    public int                      _timeTestRepeatNum = 1000;

    public bool                     _debugPathingAllowed = false;
    public bool                     _markTilesCanSeePlayer = true;

    public void FRUN_Start()
    {
        cCombat = GetComponent<Man_Combat>();
        cHelper = GetComponent<MAN_Helper>();
        
        rTilemap.CompressBounds();
        FFigureOutWhichTilesAreNonPathableAndPlacePillarObjects();

        FFindPathingTilesDiagonalFromCornersOfBlocks();
        // FDrawLinesBetweenValidConnections();
    }

    public void FFigureOutWhichTilesAreNonPathableAndPlacePillarObjects()
    {
        BoundsInt bounds = rTilemap.cellBounds;
        Debug.Log(bounds);
        mAllTiles = new PathingTile[bounds.size.x, bounds.size.y];
        for(int x=0;x<bounds.size.x; x++){
            for(int y=0; y<bounds.size.y; y++){
                mAllTiles[x,y] = new PathingTile();
            }
        }
        mAllTiles[0,0].mTraversable = false;

        for(int x=bounds.x; x<(bounds.x + bounds.size.x); x++){
            for(int y=bounds.y; y<(bounds.y + bounds.size.y); y++){
                for(int z=bounds.z; z<(bounds.z + bounds.size.z); z++){
                    TileBase tile = rTilemap.GetTile(new Vector3Int(x,y,z));
                    if(tile){
                        if(tile.ToString().Contains("Castle")){
                            mAllTiles[x - bounds.x, y - bounds.y].mTraversable = false;
                        }else{
                            mAllTiles[x - bounds.x, y - bounds.y].mTraversable = true;
                        }
                    }else{
                        Debug.Log("Nulld");
                    }

                    // Debug.Log("Three vals: " + new Vector3(x,y,z));
                }
            }
        }

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(!mAllTiles[x,y].mTraversable){
                    Vector2 pos = cHelper.FGetWorldPosOfTile(new Vector2Int(x,y));
                    Instantiate(PF_TileRockObj, pos, transform.rotation);
                }
            }
        }
    }

    public void FShowWalkableTiles()
    {
        // FClearAllMarkers();
        cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.RED);
        cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.GREEN);

        BoundsInt bounds = rTilemap.cellBounds;

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(mAllTiles[x,y].mTraversable){
                    // on the right tile.
                    Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                    tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
                    Instantiate(cHelper.PF_Green2, tileWorldPos, transform.rotation);
                }else{
                    Vector3 tileWorldPos = rTilemap.CellToWorld(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                    tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
                    Instantiate(cHelper.PF_Red1, tileWorldPos, transform.rotation);
                }
            }
        }
    }

    // A tile can have multiple corner path spots if it is a block of one.
    public void FFindPathingTilesDiagonalFromCornersOfBlocks()
    {
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                // If a tile is stone. 
                if(!mAllTiles[x,y].mTraversable){

                    void CheckAndSetIfTileValidCorner(Vector2Int startIndice, int xInc, int yInc)
                    {
                        Vector2Int goalIndice = new Vector2Int(startIndice.x+xInc, startIndice.y+yInc);
                        // If our goal adjacent indice isn't even valid, then obviously they can't path with it.
                        if(goalIndice.x < 0 || goalIndice.x >= 16 || goalIndice.y < 0 || goalIndice.y >= 16){
                            return;
                        }

                        // If we see more impassable terrain in the diagonal area, then we're not that corner.
                        if(!mAllTiles[goalIndice.x, goalIndice.y].mTraversable){
                            return;
                        }

                        // If we've gotten this far, we have a valid pathing tile, but still need to make sure
                        // we are the correct corner.
                        if(!mAllTiles[startIndice.x, goalIndice.y].mTraversable){
                            return;
                        }
                        if(!mAllTiles[goalIndice.x, startIndice.y].mTraversable){
                            return;
                        }
                        // Instantiate(cHelper.PF_Blue3, cHelper.FGetWorldPosOfTile(goalIndice), transform.rotation);
                        mAllTiles[goalIndice.x, goalIndice.y].mPathing = true;
                    }

                    CheckAndSetIfTileValidCorner(new Vector2Int(x,y), 1, 1);
                    CheckAndSetIfTileValidCorner(new Vector2Int(x,y), 1, -1);
                    CheckAndSetIfTileValidCorner(new Vector2Int(x,y), -1, 1);
                    CheckAndSetIfTileValidCorner(new Vector2Int(x,y), -1, -1);

                }
            }
        }

        mPathNodeTiles = new List<Vector2Int>();
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(mAllTiles[x,y].mPathing){
                    mPathNodeTiles.Add(new Vector2Int(x,y));
                }
            }
        }

        // Now we want to go through them all again, and connect the tiles that can see each
        // other.
        for(int i=0; i<mPathNodeTiles.Count; i++){
            for(int j=i+1; j<mPathNodeTiles.Count; j++){
                // Raycast to check if there is a wall blocking sight
                LayerMask mask = LayerMask.GetMask("ENV_Obj");
                Vector2 ourPos = cHelper.FGetWorldPosOfTile(mPathNodeTiles[i]);
                Vector2 otherPos = cHelper.FGetWorldPosOfTile(mPathNodeTiles[j]);
                Vector2 vDir = (otherPos - ourPos).normalized;
                float dis = Vector2.Distance(ourPos, otherPos);
                RaycastHit2D hit = Physics2D.Raycast(ourPos, vDir, dis, mask);
                if(hit.collider == null){
                    NodeAndDistance pNode = new NodeAndDistance(mPathNodeTiles[j], dis);
                    mAllTiles[mPathNodeTiles[i].x, mPathNodeTiles[i].y].mConnections.Add(pNode);
                    NodeAndDistance pNodeReverse = new NodeAndDistance(mPathNodeTiles[i], dis);
                    mAllTiles[mPathNodeTiles[j].x, mPathNodeTiles[j].y].mConnections.Add(pNodeReverse);
                }else{
                    Debug.Log("Hit world geometry");
                }
            }
        }

        // Now figure out which pathing tiles the merely traversable tiles can see.
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(mAllTiles[x,y].mTraversable && !mAllTiles[x,y].mPathing){
                    for(int i=0; i<mPathNodeTiles.Count; i++){
                        // Raycast to check if there is a wall blocking sight of the pathing tile.
                        LayerMask mask = LayerMask.GetMask("ENV_Obj");
                        Vector2 ourPos = cHelper.FGetWorldPosOfTile(new Vector2Int(x,y));
                        Vector2 otherPos = cHelper.FGetWorldPosOfTile(mPathNodeTiles[i]);
                        Vector2 vDir = (otherPos - ourPos).normalized;
                        float dis = Vector2.Distance(ourPos, otherPos);
                        RaycastHit2D hit = Physics2D.Raycast(ourPos, vDir, dis, mask);
                        if(hit.collider == null){
                            // I think I also need to store the distance.
                            NodeAndDistance n = new NodeAndDistance(mPathNodeTiles[i], dis);
                            mAllTiles[x,y].mConnections.Add(n);
                        }
                    }
                }
            }
        }
    }

    public void FDrawLinesBetweenValidConnections()
    {
        BoundsInt bounds = rTilemap.cellBounds;
        int connections = 0;

        for(int i=0; i<mPathNodeTiles.Count; i++){
            Vector2Int ind = mPathNodeTiles[i];
            Vector3 tileWorldPos = cHelper.FGetWorldPosOfTile(ind);
            for(int j = 0; j<mAllTiles[ind.x,ind.y].mConnections.Count; j++){
                
                Vector2Int destInd = mAllTiles[ind.x,ind.y].mConnections[j].mNode;
                Vector3 destTileWorldPos = cHelper.FGetWorldPosOfTile(destInd);
                Debug.DrawLine(tileWorldPos, destTileWorldPos, Color.cyan, 60f);
                connections++;
            }
        }

        // Debug.Log("Total Pathable Nodes: " + mPathNodeTiles.Count);
        // Debug.Log("Total connections: " + connections);
    }

    /********************************************************************************************
    New logic involves only using the pathing tiles. 

    Therefore, we can't start knowing which node to start on. We need to instead take our world 
    coordinates, although converting those to a startnode and the destination to an endnode is fine, 
    it's just that they won't be part of any path. 
    ********************************************************************************************/
    public List<Vector2Int> FCalcPath(Vector2Int startNode, Vector2Int endNode)
    {
        if(startNode == endNode){
            Debug.Log("Already on the correct node.");
            return null;
        }

        if(!mAllTiles[startNode.x,startNode.y].mTraversable){
            Debug.Log("Path START tile not traversable.");
            return null;
        }
        if(!mAllTiles[endNode.x,endNode.y].mTraversable){
            Debug.Log("Path END tile not traversable.");
            return null;
        }

        void AddTraversableTileToPathingNetwork(Vector2Int node)
        {
            if(mAllTiles[node.x, node.y].mPathing == false){
                for(int i=0; i<mAllTiles[node.x, node.y].mConnections.Count; i++){
                    Vector2Int pathTileVisible = mAllTiles[node.x, node.y].mConnections[i].mNode;
                    float dis = cHelper.FGetDistanceBetweenTiles(node, pathTileVisible);
                    mAllTiles[pathTileVisible.x, pathTileVisible.y].mConnections.Add(new NodeAndDistance(node, dis));
                }
                mPathNodeTiles.Add(node);
            }
        }
        AddTraversableTileToPathingNetwork(startNode);
        AddTraversableTileToPathingNetwork(endNode);

        // Call this before returning from this function.
        void RemoveAllTraversableTilesFromPathingNetwork()
        {
            for(int i=0; i<mPathNodeTiles.Count; i++){
                Vector2Int tile2 = mPathNodeTiles[i];
                for(int j=0; j<mAllTiles[tile2.x, tile2.y].mConnections.Count; j++){
                    Vector2Int tile = mAllTiles[tile2.x, tile2.y].mConnections[j].mNode;
                    if(!mAllTiles[tile.x, tile.y].mPathing){
                        mAllTiles[tile2.x, tile2.y].mConnections.RemoveAt(j);
                        j--;
                    }
                }
            }
            for(int i=0; i<mPathNodeTiles.Count; i++){
                if(!mAllTiles[mPathNodeTiles[i].x, mPathNodeTiles[i].y].mPathing){
                    mPathNodeTiles.RemoveAt(i);
                    i--;
                }
            }
        }

        // prep the nodes and add heuristic distance.
        for(int i=0; i<mPathNodeTiles.Count; i++){
            Vector2Int tile = mPathNodeTiles[i];
            // Use 10000f as a good "haven't been here" distance.
            mAllTiles[tile.x, tile.y].mScore = 10000f;
            float disToEndNode = cHelper.FGetDistanceBetweenTiles(tile, endNode);
            mAllTiles[tile.x, tile.y].mHeuristicDistance = disToEndNode;
            // intentionally using an illegal index.
            mAllTiles[tile.x,tile.y].mPrevNodeOnPath = new Vector2Int(-1,-1);     
            mAllTiles[tile.x,tile.y].mVisited = false;
        }
        // // prep the nodes and add heuristic distance.
        // for(int x=0; x<16; x++){
        //     for(int y=0; y<16; y++){
        //         // Use 10000f as a good "haven't been here" distance.
        //         mAllTiles[x,y].mScore = 10000f;
        //         Vector2Int dif = new Vector2Int(Mathf.Abs(x - endNode.x), Mathf.Abs(y - endNode.y));
        //         float dis = Mathf.Sqrt(dif.x*dif.x + dif.y*dif.y);
        //         mAllTiles[x,y].mHeuristicDistance = dis;
        //         mAllTiles[x,y].mPrevNodeOnPath = new Vector2Int(-1,-1);     // intentionally using an illegal index.
        //         mAllTiles[x,y].mVisited = false;
        //     }
        // }


        // To start, set the starting node score to 0.
        mAllTiles[startNode.x, startNode.y].mScore = 0;
        Vector2Int          activeInd = new Vector2Int();

        // Repeat until at destination node. 
        int                 iterations = 0;
        bool                foundPath = false;
        while(!foundPath || iterations < 256)
        {
            float               lowestScore = 10000000f;
            // Iterate through all the pathing nodes that haven't been exhausted
            // pick the one with the lowest score + heuristic.
            for(int i=0; i<mPathNodeTiles.Count; i++){
                Vector2Int pTile = mPathNodeTiles[i];
                if(mAllTiles[pTile.x, pTile.y].mVisited) continue;

                float combinedScore = mAllTiles[pTile.x, pTile.y].mHeuristicDistance + mAllTiles[pTile.x, pTile.y].mScore;
                if(combinedScore < lowestScore){
                    lowestScore = combinedScore;
                    activeInd = pTile;
                }
            }

            mAllTiles[activeInd.x, activeInd.y].mVisited = true;
            // If we're on the end node just end here.
            if(activeInd == endNode){
                foundPath = true;
                List<Vector2Int> path = new List<Vector2Int>();
                path.Add(activeInd);
                bool gotToStart = false;
                Vector2Int workingInd = activeInd;
                while(!gotToStart){
                    Debug.Log("Prev Node on path: " + mAllTiles[workingInd.x, workingInd.y].mPrevNodeOnPath);
                    path.Add(mAllTiles[workingInd.x, workingInd.y].mPrevNodeOnPath);
                    workingInd = mAllTiles[workingInd.x, workingInd.y].mPrevNodeOnPath;
                    if(workingInd == startNode) gotToStart = true;
                }
                path.Reverse();
                RemoveAllTraversableTilesFromPathingNetwork();
                return path;
            }

            // Update the scores of the pathing node's connections
            for(int i=0; i<mAllTiles[activeInd.x, activeInd.y].mConnections.Count; i++){
                Vector2Int destNodeIndice = mAllTiles[activeInd.x, activeInd.y].mConnections[i].mNode;
                float dis = mAllTiles[activeInd.x, activeInd.y].mConnections[i].mDis;
                float totalScoreThusFar = dis + mAllTiles[activeInd.x, activeInd.y].mScore;
                if(totalScoreThusFar < mAllTiles[destNodeIndice.x, destNodeIndice.y].mScore){
                    mAllTiles[destNodeIndice.x, destNodeIndice.y].mScore = totalScoreThusFar;
                    mAllTiles[destNodeIndice.x, destNodeIndice.y].mPrevNodeOnPath = activeInd;
                }
            }

            iterations++;
            if(iterations >= 255){
                Debug.Log("Hit max iterations. No more");
            }
        }
        Debug.Log("Error making path");
        RemoveAllTraversableTilesFromPathingNetwork();
        return null;

        // // To start, set the starting node score to 0.
        // // I actually never need Vector2.Distance stuff or real world position stuff. Using indexes we can figure out distances.
        // mAllTiles[startNode.x, startNode.y].mScore = 0;
        // Vector2Int          activeInd = new Vector2Int();

        // // Repeat until at destination node. 
        // int                 iterations = 0;
        // bool                foundPath = false;
        // while(!foundPath || iterations < 256)
        // {
        //     float               lowestScore = 10000000f;
        //     // Iterate through all the nodes that haven't been exhausted
        //     // pick the one with the lowest score + heuristic.
        //     for(int x=0; x<16; x++){
        //         for(int y=0; y<16; y++){
        //             if(mAllTiles[x,y].mVisited) continue;

        //             float combinedScore = mAllTiles[x,y].mHeuristicDistance + mAllTiles[x,y].mScore;
        //             if(combinedScore < lowestScore){
        //                 lowestScore = combinedScore;
        //                 activeInd = new Vector2Int(x,y);
        //             }
        //         }
        //     }
        //     mAllTiles[activeInd.x, activeInd.y].mVisited = true;
        //     // If we're on the end node just end here.
        //     if(activeInd == endNode){
        //         foundPath = true;
        //         List<Vector2Int> path = new List<Vector2Int>();
        //         path.Add(activeInd);
        //         bool gotToStart = false;
        //         Vector2Int workingInd = activeInd;
        //         while(!gotToStart){
        //             path.Add(mAllTiles[workingInd.x, workingInd.y].mPrevNodeOnPath);
        //             workingInd = mAllTiles[workingInd.x, workingInd.y].mPrevNodeOnPath;
        //             if(workingInd == startNode) gotToStart = true;
        //         }
        //         path.Reverse();
        //         return path;
        //     }

        //     // Update its connections.
        //     for(int i=0; i<mAllTiles[activeInd.x, activeInd.y].mConnections.Count; i++){
        //         Vector2Int destNodeIndice = mAllTiles[activeInd.x, activeInd.y].mConnections[i];
        //         Vector2Int dif = new Vector2Int(Mathf.Abs(destNodeIndice.x - activeInd.x), Mathf.Abs(destNodeIndice.y - activeInd.y));
        //         float dis = Mathf.Sqrt(dif.x*dif.x + dif.y*dif.y);
        //         float totalScoreThusFar = dis + mAllTiles[activeInd.x, activeInd.y].mScore;
        //         if(totalScoreThusFar < mAllTiles[destNodeIndice.x, destNodeIndice.y].mScore){
        //             mAllTiles[destNodeIndice.x, destNodeIndice.y].mScore = totalScoreThusFar;
        //             mAllTiles[destNodeIndice.x, destNodeIndice.y].mPrevNodeOnPath = activeInd;
        //         }
        //     }

        //     iterations++;
        //     if(iterations >= 255){
        //         Debug.Log("Hit max iterations. No more");
        //     }
        // }
        // Debug.Log("Error making path");
        // return null;

    }

    public float FDistance(Vector2Int startTile, Vector2Int endTile)
    {
        Vector2Int dif = new Vector2Int(Mathf.Abs(startTile.x - endTile.x), Mathf.Abs(startTile.y - endTile.y));
        float dis = Mathf.Sqrt(dif.x*dif.x + dif.y*dif.y);   
        return dis;
    }
    
    void RUN_MoveCharacter()
    {
        if(mPath.Count == 0){
            Debug.Log("Done pathing");
            mPathChar = false;
            return;
        }

        Vector2 curDestNode = cHelper.FGetWorldPosOfTile(mPath[0]);
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

    public void FRUN_Update()
    {
        if(_debugPathingAllowed){

            if(Input.GetKeyDown(KeyCode.K)){
                FShowWalkableTiles();
            }
            if(Input.GetKeyDown(KeyCode.J)){
                //FMakeWalkableTilesFormConnections();
            }
            if(Input.GetKeyDown(KeyCode.I)){
                // FDrawConnections();
            }
            if(Input.GetKeyDown(KeyCode.U)){
                TimeEveryPossiblePath();
            }

            // // replace start.
            // if(Input.GetMouseButtonDown(0)){
            //     cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.PURPLE);
            //     cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.BLUE);
            //     Camera c = Camera.main;
            //     Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);
            //     rStartNode = Instantiate(cHelper.PF_Purple4, msPos, transform.rotation);
            //     Vector2Int startNode = cHelper.FGetTileClosestToSpot(rStartNode.transform.position);
            //     Vector2Int endNode = cHelper.FGetTileClosestToSpot(rEndNode.transform.position);
            //     FDrawPath(FCalcPath(startNode, endNode));

            //     rEnemy.transform.position = msPos;
            // }
            // // replace finish.
            // if(Input.GetMouseButtonDown(1)){
            //     cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.YELLOW);
            //     cHelper.FClearMarkersOfLevel(MSC_SquareMarker.MARKER_LEVEL.BLUE);
            //     Camera c = Camera.main;
            //     Vector2 msPos = c.ScreenToWorldPoint(Input.mousePosition);
            //     rEndNode = Instantiate(cHelper.PF_Yellow5, msPos, transform.rotation);
            //     Vector2Int startNode = cHelper.FGetTileClosestToSpot(rStartNode.transform.position);
            //     Vector2Int endNode = cHelper.FGetTileClosestToSpot(rEndNode.transform.position);
            //     List<Vector2Int> path = FCalcPath(startNode, endNode);
            //     FDrawPath(path);

            //     // make the enemy move.
            //     mPath = new List<Vector2Int>(path);
            //     mPathChar = true;
            // }

            if(Input.GetKeyDown(KeyCode.Space)){
                DateTime stTm = DateTime.Now;
                Vector2Int startNode = cHelper.FGetTileClosestToSpot(rStartNode.transform.position);
                Vector2Int endNode = cHelper.FGetTileClosestToSpot(rEndNode.transform.position);
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

    }

    public void TimeEveryPossiblePath()
    {
        System.TimeSpan[,] times = new System.TimeSpan[16,16];

        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                if(!mAllTiles[x,y].mTraversable) continue;

                DateTime stTm = DateTime.Now;
                for(int x1=0; x1<16; x1++){
                    for(int y1=0; y1<16; y1++){
                        if(!mAllTiles[x1,y1].mTraversable) continue;
                        Vector2Int startNode = cHelper.FGetTileClosestToSpot(rStartNode.transform.position);
                        Vector2Int endNode = cHelper.FGetTileClosestToSpot(rEndNode.transform.position);
                        FCalcPath(startNode, endNode);
                    }
                }
                DateTime endTm = DateTime.Now;
                Debug.Log("Time taken: " + (endTm - stTm));
                times[x,y] = endTm - stTm;
            }
        }

        System.TimeSpan totalTime = new System.TimeSpan();
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                totalTime += times[x,y];
            }
        }
        Debug.Log("Total Time: " + totalTime);
    }


    public Vector2Int FFindClosestValidTile(Vector2 pos)
    {
        Vector2Int tile = cHelper.FGetTileClosestToSpot(pos);
        if(!FIsTileIndiceValid(tile)){
            Debug.Log("Starting tile invalid: " + tile + " from pos: " + pos);
            return new Vector2Int(-100,-100);
        }
        if(mAllTiles[tile.x,tile.y].mTraversable){
            return tile;
        }else{
            Vector2Int testTile = new Vector2Int();
            int iterations = 1;
            while(iterations < 10){
                testTile = tile;
                testTile.x -= iterations;
                if(FIsTileIndiceValid(testTile)){
                    if(mAllTiles[testTile.x,testTile.y].mTraversable){
                        return testTile;
                    }
                }
                testTile.x = tile.x+iterations;
                if(FIsTileIndiceValid(testTile)){
                    if(mAllTiles[testTile.x,testTile.y].mTraversable){
                        return testTile;
                    }
                }
                testTile = tile; testTile.y -= iterations;
                if(FIsTileIndiceValid(testTile)){
                    if(mAllTiles[testTile.x,testTile.y].mTraversable){
                        return testTile;
                    }
                }
                testTile.y = tile.y + iterations;
                if(FIsTileIndiceValid(testTile)){
                    if(mAllTiles[testTile.x,testTile.y].mTraversable){
                        return testTile;
                    }
                }
                iterations++;
            }

        }
        Debug.Log("Kept iterating, never found valid tile with starting point: " + tile);
        return new Vector2Int(-1000, -1000);
    }

    bool FIsTileIndiceValid(Vector2Int indice)
    {
        if(indice.x < 0) return false;
        if(indice.x >= 16) return false;
        if(indice.y < 0) return false;
        if(indice.y >= 16) return false;

        return true;
    }

    public bool FIsTileNextToAnyUnpathableTiles(Vector2Int tile)
    {
        if(!FIsTileIndiceValid(tile)){
            Debug.Log("Invalid tile");
            return false;
        }

        int edgesTouching = 0;
        int connectionsItShouldHave = 8;
        if(tile.x == 0 || tile.x == 15){
            edgesTouching++;
        }
        if(tile.y == 0 || tile.y == 15){
            edgesTouching++;
        }
        if(edgesTouching == 1){
            connectionsItShouldHave = 5;
        }
        if(edgesTouching == 2){
            connectionsItShouldHave = 3;
        }

        // We've already made the connections. Just check if they have connections in all valid directions, and that's that.
        if(mAllTiles[tile.x,tile.y].mConnections.Count == connectionsItShouldHave){
            return false;
        }
        
        return true;
    }

    // Returns square surrounding tile. Not ideally circular, but good enough for now.
    public List<Vector2Int> FGetSurroundingTiles(Vector2Int tile, int depth = 1, bool demandPathableTilesOnly = false)
    {
        if(!FIsTileIndiceValid(tile)){
            Debug.Log("Invalid start tile");
            return null;
        }

        List<Vector2Int> surroundingTiles = new List<Vector2Int>();

        for(int x=tile.x-depth; x<tile.x+depth; x++){
            for(int y=tile.y-depth; y<tile.y+depth; y++){
                Vector2Int indice = new Vector2Int(x,y);
                if(FIsTileIndiceValid(indice)){
                    if(demandPathableTilesOnly){
                        if(mAllTiles[x,y].mTraversable){
                            surroundingTiles.Add(new Vector2Int(x,y));
                        }
                    }else{
                        surroundingTiles.Add(new Vector2Int(x,y));
                    }
                }
            }
        }

        return surroundingTiles;
    }

    public List<Vector2Int> FGetSurroundingTilesByRadius(Vector2Int tile, float radius, bool demandPathableTilesOnly = false)
    {
        if(!FIsTileIndiceValid(tile)){
            Debug.Log("Invalid start tile");
            return null;
        }

        List<Vector2Int> surroundingTiles = new List<Vector2Int>();
        for(int x=0; x<16; x++){
            for(int y=0; y<16; y++){
                int disX = Mathf.Abs(x - tile.x); disX *= disX;
                int disY = Mathf.Abs(y - tile.y); disY *= disY;
                float dis = Mathf.Sqrt((float)disX + (float)disY);

                if(dis <= radius){
                    if(!demandPathableTilesOnly){
                        surroundingTiles.Add(new Vector2Int(x,y));
                    }else{
                        if(mAllTiles[x,y].mTraversable){
                            surroundingTiles.Add(new Vector2Int(x,y));
                        }
                    }
                }
            }
        }

        return surroundingTiles;
    }


}
