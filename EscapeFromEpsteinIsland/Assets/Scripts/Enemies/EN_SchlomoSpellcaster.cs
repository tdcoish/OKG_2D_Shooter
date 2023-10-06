/*************************************************************************************
The first spellcaster in the game, and the first Schlomo. This guy casts a six pointed
star that has each point turn into an orb which then fires upon the player. I want a blurb
to pop up at the bottom of the screen warning the user that he's spellcasting, which is similar
to Star Ocean's spells system.

In between spellcasts the guy runs around, mostly away from the player.
*************************************************************************************/
using UnityEngine;

public class EN_SchlomoSpellcaster : EN_Base
{
    public uint                         kFleeing = 1<<2;
    public uint                         kSpellcasting = 1<<3;

    public float                        _spellcastTime = 2f;
    public float                        mLastSpellcastTmStmp;
    public float                        _fleeDistance = 4f;
    public float                        _fleeTime = 3f;
    public float                        mFleeTmStmp;
    public float                        _fleeSpd = 4f;

    public EN_SchlomoSpellcasterAnim    cAnim;
    public SPL_StarOfDavid              PF_StarOfDavidSpell;

    public override void F_CharSpecStart()
    {
        kState = kSpellcasting;
        mLastSpellcastTmStmp = Time.time - _spellcastTime*0.8f;
        cRigid.velocity = Vector2.zero;
    }
    public override void F_CharSpecUpdate()
    {
        if(kState == kStunned){
            F_RunStunRecovery();
        }else if(kState == kSpellcasting){
            RUN_Spellcasting();
        }else if(kState == kFleeing){
            RUN_Fleeing();
        }

        cAnim.FAnimate();
    }

    public void ENTER_Spellcasting()
    {
        kState = kSpellcasting;
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
        kState = kFleeing;
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
    public override void EXIT_Stun()
    {
        ENTER_Spellcasting();
    }

}
