using UnityEngine;
using System.Collections.Generic;

public enum DIRECTION{NONE, DOWN, DOWNRIGHT, DOWNLEFT, RIGHT, LEFT, UPRIGHT, UPLEFT, UP}

class DotFloatText{
    public DotFloatText(float dot, DIRECTION dir){mDot = dot; mDir = dir;}
    public float mDot;
    public DIRECTION mDir;
}

public class MAN_Helper : MonoBehaviour
{
    [HideInInspector]
    public Man_Combat               cCombat;

    public MSC_SquareMarker         PF_Red1;
    public MSC_SquareMarker         PF_Green2;
    public MSC_SquareMarker         PF_Blue3;
    public MSC_SquareMarker         PF_Purple4;
    public MSC_SquareMarker         PF_Yellow5;

    public void FRUN_Start()
    {
        cCombat = GetComponent<Man_Combat>();
    }

    public Vector2 FGetWorldPosOfTile(Vector2Int indice)
    {
        if(indice.x < 0 || indice.x > 16 || indice.y < 0 || indice.y > 16){
            Debug.Log("Path node not in field of play");
            return new Vector2();
        }
        BoundsInt bounds = cCombat.rTilemap.cellBounds;
        Vector2 tileWorldPos = cCombat.rTilemap.CellToWorld(new Vector3Int(indice.x + bounds.x, indice.y + bounds.y, 0));
        tileWorldPos.x += 0.5f; tileWorldPos.y += 0.5f;
        return tileWorldPos;
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

    public Vector2Int FGetTileClosestToSpot(Vector2 posOfObj)
    {
        BoundsInt bounds = cCombat.rTilemap.cellBounds;

        for(int x=bounds.x; x<(bounds.x + bounds.size.x); x++){
            for(int y=bounds.y; y<(bounds.y + bounds.size.y); y++){
                Vector3 tileWorldPos = cCombat.rTilemap.CellToWorld(new Vector3Int(x, y, 0));

                if(Vector2.Distance(posOfObj, tileWorldPos) < 1f){
                    return new Vector2Int(x - bounds.x, y - bounds.y);
                }
            }
        }

        return new Vector2Int(-1,-1);
    }

    public DIRECTION FGetCardinalDirection(Vector2 vDir)
    {
        vDir = vDir.normalized;

        List<DotFloatText> dots = new List<DotFloatText>();
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(0,-1)), DIRECTION.DOWN));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(1,-1).normalized), DIRECTION.DOWNRIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(-1,-1).normalized), DIRECTION.DOWNLEFT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(1,0)), DIRECTION.RIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(-1,0)), DIRECTION.LEFT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(1,1).normalized), DIRECTION.UPRIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(-1,1).normalized), DIRECTION.UPLEFT));
        dots.Add(new DotFloatText(Vector2.Dot(vDir, new Vector2(0,1)), DIRECTION.UP));

        int indLargest = -1;
        float largest = -10000f;
        for (int i=0; i<dots.Count; i++){
            if(dots[i].mDot > largest){
                largest = dots[i].mDot;
                indLargest = i;
            }
        }

        if(indLargest == -1){
            Debug.Log("No direction? That doesn't make sense");
            return DIRECTION.NONE;
        }

        return dots[indLargest].mDir;
    }

    public Vector3 PointToLookAtAlongHeading(DIRECTION heading)
    {
        Vector2 vDir = new Vector2();
        switch(heading){
            case DIRECTION.DOWN: vDir = new Vector2(0f, -1f).normalized; break;
            case DIRECTION.UP: vDir = new Vector2(0f, 1f).normalized; break;
            case DIRECTION.RIGHT: vDir = new Vector2(1f, 0f).normalized; break;
            case DIRECTION.LEFT: vDir = new Vector2(-1f, 0f).normalized; break;
            case DIRECTION.DOWNRIGHT: vDir = new Vector2(1f, -1f).normalized; break;
            case DIRECTION.DOWNLEFT: vDir = new Vector2(-1f, -1f).normalized; break;
            case DIRECTION.UPRIGHT: vDir = new Vector2(1f, 1f).normalized; break;
            case DIRECTION.UPLEFT: vDir = new Vector2(-1f, 1f).normalized; break;
        }

        return (Vector3)vDir;
    }

    public bool FCanRaytraceDirectlyToPlayer(Vector2 playerPos, Vector2 ourPos, LayerMask mask)
    {
        Vector2 dif = playerPos - ourPos;
        RaycastHit2D hit = Physics2D.Raycast(ourPos, dif.normalized, 1000f, mask);

        if(hit.collider != null){
            if(!hit.collider.GetComponent<PC_Cont>()){
                Debug.DrawLine(ourPos, hit.collider.gameObject.transform.position, Color.grey);
            }
            if(hit.collider.GetComponent<PC_Cont>()){
                Debug.DrawLine(ourPos, hit.collider.gameObject.transform.position, Color.green);
                return true;
            }
        }
        return false;
    }

    // Problem is that we're hitting ourselves sometimes.
    public bool FCanSeePlayerFromAllCornersOfBox(Vector2 playerPos, Vector2 castPos, float boxSize, LayerMask mask)
    {
        Vector2 workingPos = castPos;
        workingPos.x -= boxSize; workingPos.y -= boxSize;
        if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
            workingPos.x = castPos.x + boxSize;
            if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                workingPos = castPos; workingPos.y += boxSize; workingPos.x -= boxSize;
                if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                    workingPos.x = castPos.x + boxSize;
                    if(FCanRaytraceDirectlyToPlayer(playerPos, workingPos, mask)){
                        return true;
                    }
                }
            }
        }
        return false;
    }

}
