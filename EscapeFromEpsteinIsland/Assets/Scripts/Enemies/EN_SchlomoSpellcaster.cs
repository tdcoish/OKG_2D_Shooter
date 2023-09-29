/*************************************************************************************
The first spellcaster in the game, and the first Schlomo. This guy casts a six pointed
star that has each point turn into an orb which then fires upon the player. I want a blurb
to pop up at the bottom of the screen warning the user that he's spellcasting, which is similar
to Star Ocean's spells system.

In between spellcasts the guy runs around, mostly away from the player.
*************************************************************************************/
using UnityEngine;

public class EN_SchlomoSpellcaster : Actor
{
    public enum STATE{FLEEING, SPELLCASTING, STUNNED}
    public STATE                        mState = STATE.SPELLCASTING;

    public Rigidbody2D                  cRigid;

    public float                        _spellcastTime = 2f;
    public float                        mLastSpellcastTmStmp;
    public float                        _stunRecTime = 1f;
    public float                        mStunTmStmp;
    public float                        _fleeDistance = 4f;
    public float                        _fleeTime = 3f;
    public float                        mFleeTmStmp;
    public float                        _fleeSpd = 4f;
    public float                        _maxHealth = 100f;
    public float                        mHealth;

    public GameObject                   PF_Particles;
    public UI_EN                        gUI;
    public EN_SchlomoSpellcasterAnim    cAnim;

    public SPL_StarOfDavid              PF_StarOfDavidSpell;

    public override void RUN_Start()
    {
        mLastSpellcastTmStmp = Time.time - _spellcastTime*0.8f;
        mHealth = _maxHealth;
        cRigid.velocity = Vector2.zero;
    }
    public override void RUN_Update()
    {
        switch(mState)
        {
            case STATE.FLEEING: RUN_Fleeing(); break;
            case STATE.SPELLCASTING: RUN_Spellcasting(); break;
            case STATE.STUNNED: RUN_Stunned(); break;
        }

        gUI.FUpdateShieldHealthBars(mHealth, _maxHealth);
        cAnim.FAnimate();
    }

    public void ENTER_Spellcasting()
    {
        mState = STATE.SPELLCASTING;
        mLastSpellcastTmStmp = Time.time;           // So they always start from zero.
        cRigid.velocity = Vector2.zero;
    }
    public void RUN_Spellcasting()
    {
        if(rOverseer.rPC == null) return;
        if(Vector3.Distance(transform.position, rOverseer.rPC.transform.position) < _fleeDistance){
            ENTER_Fleeing();
            return;
        }

        if(Time.time - mLastSpellcastTmStmp > _spellcastTime){
            // cast the spell again.
            Instantiate(PF_StarOfDavidSpell, rOverseer.rPC.transform.position, transform.rotation);
            mLastSpellcastTmStmp = Time.time;
        }

        // Make them point to the player.
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }
    public void ENTER_Fleeing()
    {
        mState = STATE.FLEEING;
        mFleeTmStmp = Time.time;
        cRigid.velocity = _fleeSpd * (transform.position - rOverseer.rPC.transform.position).normalized;
    }
    // Have to get him to naturally reposition in such a way that he doesn't bump into anything.
    public void RUN_Fleeing()
    {
        transform.up = cRigid.velocity.normalized;
        // I really don't want to have to cross the pathfinding bridge today, so I'm not going to.
        // Maybe he doesn't run away unless he sees the player.
        if(Time.time - mFleeTmStmp > _fleeTime){
            ENTER_Spellcasting();
        }
    }

    void ENTER_Stun()
    {
        mState = STATE.STUNNED;
        mStunTmStmp = Time.time;
        cRigid.velocity = Vector2.zero;
    }
    public void RUN_Stunned()
    {
        if(Time.time - mStunTmStmp > _stunRecTime){
            // Figure out the correct state.
            ENTER_Spellcasting();
        }
    }


    public void FTakeDamage(float amt, DAMAGE_TYPE type)
    {
        ENTER_Stun();
        mHealth -= amt;

        if(mHealth <= 0f){
            Instantiate(PF_Particles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<PJ_PC_Bullet>()){
            FTakeDamage(2f, DAMAGE_TYPE.BULLET);
        }else if(col.GetComponent<PJ_PC_Plasmoid>()){
            FTakeDamage(2f, DAMAGE_TYPE.PLASMA);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            FTakeDamage(10f, DAMAGE_TYPE.PLASMA);
            Destroy(col.gameObject);
        }else if(col.GetComponent<PC_SwordHitbox>()){
            FTakeDamage(80f, DAMAGE_TYPE.SLASH);
            col.GetComponentInParent<PC_Cont>().FHeal(col.GetComponentInParent<PC_Melee>()._healAmtFromSuccessfulHit);
        }else if(col.GetComponent<PJ_PC_Firebolt>()){
            FTakeDamage(40f, DAMAGE_TYPE.PLASMA);
        }
        else if(col.GetComponent<PJ_PC_BeamRifle>()){
            FTakeDamage(40f, DAMAGE_TYPE.BULLET);
        }
        else if(col.GetComponent<PJ_PC_ShotgunPellet>()){
            FTakeDamage(40f, DAMAGE_TYPE.BULLET);
            Destroy(col.gameObject);
        }
        else if(col.GetComponent<PJ_PC_Needle>()){
            FTakeDamage(40f, DAMAGE_TYPE.BULLET);
            Destroy(col.gameObject);
        }
    }

    public override void FAcceptHolyWaterDamage(float amt)
    {
        FTakeDamage(amt, DAMAGE_TYPE.HOLYWATER);
    }

}
