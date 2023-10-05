using UnityEngine;
using System.Collections.Generic;
/**************************************************************
Mindlessly follows the player and then "explodes" itself, doing damage.

That went really well. Love how they force the player to keep moving.
**************************************************************/

public class EN_FloodInfectionForm : Actor
{
    public float                        _invulnerabilityTime = 0.1f;
    public float                        mCreatedTmStmp;
    public bool                         mInvulnerable;

    public float                        _maxHealth = 10f;
    public float                        mHealth;
    public float                        _spd = 2f;
    public float                        _damage = 20f;
    Rigidbody2D                         cRigid;

    List<Vector2Int>                    mPath;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        rOverseer = FindObjectOfType<Man_Combat>();
        mHealth = _maxHealth;
        mInvulnerable = true;
        mCreatedTmStmp = Time.time;
    }

    public override void RUN_Update()
    {
        if(rOverseer.rPC == null) return;
        if(mInvulnerable){
            if(Time.time - mCreatedTmStmp > _invulnerabilityTime){
                mInvulnerable = false;
            }
        }

        // Just follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(hit.collider == null) return;
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            if(mPath == null){
                return;
            }
            mPath.RemoveAt(0);
            Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
            cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }
    }

    public void F_TakeDamage(float amt)
    {
        mHealth -= amt;
        if(mHealth <= 0f){
            // This particular enemy has to explode.
            rOverseer.FRegisterDeadEnemy(this);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(mInvulnerable) return;

        if(col.GetComponent<PC_SwordHitbox>()){
            F_TakeDamage(90f);
        }
        if(col.GetComponent<PJ_PC_Firebolt>()){
            F_TakeDamage(20f);
        }
    }
}
