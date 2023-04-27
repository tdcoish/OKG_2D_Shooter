using UnityEngine;
using System.Collections.Generic;
/**************************************************************
Mindlessly follows the player and then "explodes" itself, doing damage.

That went really well. Love how they force the player to keep moving.
**************************************************************/

public class EN_FloodInfectionForm : Actor
{
    public float                        _spd = 2f;
    public float                        _damage = 20f;
    Rigidbody2D                         cRigid;

    List<Vector2Int>                    mPath;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
    }

    public override void RUN_Update()
    {
        // Just follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            mPath.RemoveAt(0);
            Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
            cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Destroy(gameObject);
    }
}
