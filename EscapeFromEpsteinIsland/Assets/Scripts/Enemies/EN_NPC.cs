/****************************************************************************************************
Okay. The new basic enemy, the NPC. These are the totally unthinking reddit-tier golems who make up
the mass of the golem army. The current design is just them slowly shambling their way to the player, 
and then attacking you with a very difficult to dodge melee swipe. I think each individual one should 
be dodgeable, but I'm not sure. We can also just go the Vampire Survivors route and have the enemies
just damage you in a radius around them. That way you can't just run through them. Saves me the need 
to create a melee. Hell, saves me the need to create any states but shambling and Hitstunned.
****************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EN_NPC : Actor
{
    public enum State {SHAMBLING, HITSTUNNED}
    public State                        mState;
    public float                        _preExplosionTime = 0.5f;
    public float                        mPreExplosionTmStmp;
    public float                        _attackRadius = 1f;
    public float                        _spd = 0.5f;
    public float                        _damagePerSecond = 40f;
    public float                        _health = 100f;
    public float                        mHealth;
    public float                        _hitstunTime = 2f;
    public float                        mHitTmStmp;
    Rigidbody2D                         cRigid;
    EN_NPCAnim                          cAnim;
    public DIRECTION                    mHeading;

    // Have to use rotation constraints + make a parent gameobject.
    public Image                        UI_HealthBarFill;

    List<Vector2Int>                    mPath;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mState = State.SHAMBLING;
        cAnim = GetComponent<EN_NPCAnim>();
        mHealth = _health;
    }

    public void FRUN_Shambling()
    {
        // Just follow the player.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        if(hit.collider == null){
            Debug.Log("NPC raycast hit null. Weird");
            return;
        }
        if(!hit.collider.GetComponent<PC_Cont>()){
            // Can't see player.
            // Now just pathfind to player.
            MAN_Pathing p = rOverseer.GetComponent<MAN_Pathing>();
            Vector2Int ourNode = p.FFindClosestValidTile(transform.position);
            Vector2Int pcNode = p.FFindClosestValidTile(rOverseer.rPC.transform.position);
            mPath = p.FCalcPath(ourNode, pcNode);
            // start node will always be ours.
            if(mPath != null){
                mPath.RemoveAt(0);
                Vector2 curDestPos = rOverseer.GetComponent<MAN_Helper>().FGetWorldPosOfTile(mPath[0]);
                cRigid.velocity = (curDestPos - (Vector2)transform.position).normalized * _spd;
            }else{
                Debug.Log("NPC path null.");
            }

        }else{
            cRigid.velocity = vDir.normalized * _spd;
        }
        
        PC_Cont rPC = rOverseer.rPC;
        if(Vector3.Distance(rPC.transform.position, transform.position) < _attackRadius){
            // Attack player.
            rPC.F_GetZappedByNPC(_damagePerSecond);
        }

        mHeading = rOverseer.GetComponent<MAN_Helper>().FGetCardinalDirection(cRigid.velocity.normalized);
    }

    public void FRUN_HitStunned()
    {
        cRigid.velocity = Vector2.zero;
        if(Time.time - mHitTmStmp > _hitstunTime){
            mState = State.SHAMBLING;
        }
    }

    public override void RUN_Update()
    {
        switch(mState){
            case State.SHAMBLING: FRUN_Shambling(); break;
            case State.HITSTUNNED: FRUN_HitStunned(); break;
        }

        cAnim.FAnimate();
        UI_HealthBarFill.fillAmount = mHealth / _health;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        void TakeDamage(float amt)
        {
            mHealth -= amt;
            if(mHealth <= 0f){
                rOverseer.FRegisterDeadEnemy(this);
            }else{
                mState = State.HITSTUNNED;
                mHitTmStmp = Time.time;
            }
        }

        if(col.GetComponent<PC_SwordHitbox>()){
            TakeDamage(30f);
        }
        if(col.GetComponent<PJ_PC_Firebolt>()){
            TakeDamage(30f);
            Destroy(col.gameObject);
        }

        // if we collide with another actor, move ourselves away. Not a perfect solution, because we should
        // do it simultaneously for both entities.
        if(col.GetComponent<Actor>()){
            Vector2 ourPos = transform.position;
            Vector2 theirPos = col.gameObject.transform.position;

            // I guess just move both 10% further away?
            float dis = Vector2.Distance(ourPos, theirPos);
            Vector2 center = (ourPos + theirPos)/2f;
            ourPos = center + (ourPos - center) * 1.1f;
            theirPos = center + (theirPos - center) * 1.1f;

            transform.position = ourPos;
            col.gameObject.transform.position = theirPos;
        }
    }
}
