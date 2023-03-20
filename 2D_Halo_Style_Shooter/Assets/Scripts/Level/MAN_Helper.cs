using UnityEngine;

public class MAN_Helper : MonoBehaviour
{
    [HideInInspector]
    public Man_Combat               cCombat;

    public MSC_SquareMarker         PF_Red1;
    public MSC_SquareMarker         PF_Green2;
    public MSC_SquareMarker         PF_Blue3;
    public MSC_SquareMarker         PF_Purple4;
    public MSC_SquareMarker         PF_Yellow5;

    void Start()
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

}
