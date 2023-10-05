using UnityEngine;
using System.Collections.Generic;

public class EN_BPBertha : Actor
{
    public enum STATE{FOLLOWING_PLAYER, PRE_EXPLOSION, STUNNED}
    public STATE                        mState;
    public float                        _maxHealth = 100f;
    public float                        mHealth;
    public float                        _explosionChargeTime = 4f;
    public float                        mChargeTmStmp;
    public float                        _disToTriggerPreExplosion;
    public float                        _stunTime = 2f;
    public float                        mStunTmStmp;
    public float                        _spd = 0.5f;
    public int                          _numWormsToSpawnUponDeath = 3;
    Rigidbody2D                         cRigid;
    EN_BPBerthaAnim                     cAnim;
    List<Vector2Int>                    mPath;

    public UI_EN                        gUI;
    public EX_BPBertha                  PF_Explosion;
    public EN_FloodInfectionForm        PF_InfectionForm;

    public override void RUN_Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
        mState = STATE.FOLLOWING_PLAYER;
        cAnim = GetComponent<EN_BPBerthaAnim>();
        mHealth = _maxHealth;
    }

    public override void RUN_Update()
    {
        if(rOverseer.rPC == null) return;
        // Move to player.
        // Actually for now don't bother making this one move. 
        switch(mState){
            case STATE.FOLLOWING_PLAYER: F_Run_FollowPlayer(); break;
            case STATE.PRE_EXPLOSION: F_Run_PreExplosion(); break;
            case STATE.STUNNED: F_Run_Stunned(); break;
        }

        gUI.FUpdateShieldHealthBars(mHealth, _maxHealth);
        cAnim.FAnimate();
    }

    public void F_Run_FollowPlayer()
    {
        if(Vector2.Distance(transform.position, rOverseer.rPC.transform.position) < _disToTriggerPreExplosion){
            mState = STATE.PRE_EXPLOSION;
            mChargeTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            return;
        }

        // Otherwise follow the player.
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
    public void F_Run_PreExplosion()
    {
        if(Time.time - mChargeTmStmp > _explosionChargeTime){
            F_Death();
        }
    }
    void F_Run_Stunned()
    {
        cRigid.velocity = Vector2.zero;
        if(Time.time - mStunTmStmp > _stunTime){
            mState = STATE.FOLLOWING_PLAYER;
        }
    }

    public void F_TakeDamage(float amt)
    {
        mHealth -= amt;
        if(mHealth <= 0f){
            // This particular enemy has to explode.
            F_Death();
            // Instantiate(PF_Particles, transform.position, transform.rotation);
        }
        if(mState != STATE.PRE_EXPLOSION){
            mState = STATE.STUNNED;
            mStunTmStmp = Time.time;
        }
    }

    public void F_Death()
    {
        // Create huge explosion
        EX_BPBertha e = Instantiate(PF_Explosion, transform.position, transform.rotation);
        e.F_Start(rOverseer);
        // Spawn in the flood infection form thingies. Technically sentient cancers combined with worms.
        // Don't want them to be immediately killed by the explosion.
        // We can figure that out with the flood infection form script.
        for(int i=0; i<_numWormsToSpawnUponDeath; i++){
            EN_FloodInfectionForm f = Instantiate(PF_InfectionForm, transform.position, transform.rotation);
            rOverseer.rActors.Add(f);
            f.RUN_Start();
        }
        // Then die.
        rOverseer.FRegisterDeadEnemy(this);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PC_SwordHitbox>()){
            F_TakeDamage(90f);
        }
        if(col.GetComponent<PJ_PC_Firebolt>()){
            F_TakeDamage(20f);
            Destroy(col.gameObject);
        }
    }
}
