/**************************************************************************************************
Throws weaves at the player in a circular pattern from a distance. Twerks on the player when close.
**************************************************************************************************/
using UnityEngine;

public class EN_Shaniqua : EN_Base
{
    public uint                         kLongRange = 1<<2;
    public uint                         kThrowing = 1<<3;
    public uint                         kShortRange = 1<<4;
    public uint                         kTwerking = 1<<5;
    public uint                         kTwerkPrep = 1<<6;

    public Sprite                       rMoving;
    public Sprite                       rHitstun;
    public Sprite                       rThrowing;
    public Sprite                       rTwerking;
    public Sprite                       rTwerkPrep;

    public float                        _weaveThrowInterval = 1f;
    public float                        mThrowTmStmp;
    public float                        _weaveThrowRange = 5f;
    public float                        _switchToCloseRange = 2.5f;
    public float                        _switchToLongRange = 3.5f;
    public float                        _twerkTriggerDistance = 1f;
    public float                        _twerkPrepTime = 0.2f;
    public float                        mTwerkPrepTmStmp;
    public float                        _twerkTime = 1f;
    public float                        mTwerkTmStmp;
    public EN_TwerkHitbox               rTwerkHitbox;

    public PJ_Weave                     PF_Weave;

    public override void F_CharSpecStart()
    {
        kState = kLongRange;
        rTwerkHitbox.gameObject.SetActive(false);
    }
    
    public override void F_CharSpecUpdate()
    {
        if(kState == kShortRange){
            F_RunShortRange();
        }else if(kState == kLongRange){
            F_RunLongRange();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }else if(kState == kThrowing){
            F_RunThrowing();
        }else if(kState == kTwerking){
            F_RunTwerking();
        }else if(kState == kTwerkPrep){
            F_RunTwerkPrep();
        }

        F_Animate();
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kLongRange;
    }

    public void F_RunShortRange()
    {
        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(dis < _twerkTriggerDistance){
            ENTER_TwerkPrep();
            return;
        }
        if(dis > _switchToLongRange){
            kState = kLongRange;
            return;
        }

        MoveToPlayer();
    }

    public void F_RunLongRange()
    {
        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(dis < _weaveThrowRange){
            kState = kThrowing;
            return;
        }
        if(dis < _switchToCloseRange){
            kState = kShortRange;
            return;
        }

        MoveToPlayer();
    }

    public void ThrowWeave()
    {
        mThrowTmStmp = Time.time;
        PJ_Weave w = Instantiate(PF_Weave, transform.position, transform.rotation);
        w.FShootMe(rOverseer.rPC.transform.position, transform.position, transform.right);
        PJ_Weave w2 = Instantiate(PF_Weave, transform.position, transform.rotation);
        w2.FShootMe(rOverseer.rPC.transform.position, transform.position, transform.right * -1f);
        kState = kThrowing;
        cRigid.velocity = Vector2.zero;
    }

    public void MoveToPlayer()
    {
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC", "ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, Mathf.Infinity, mask);
        if(hit.collider == null){
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

        transform.up = cRigid.velocity.normalized; 
    }

    public void ENTER_TwerkPrep()
    {
        mTwerkPrepTmStmp = Time.time;
        kState = kTwerkPrep;
        cRigid.velocity = Vector2.zero;
    }

    public void F_RunTwerkPrep()
    {
        if(Time.time - mTwerkPrepTmStmp > _twerkPrepTime){
            ENTER_Twerk();
        }
    }

    public void ENTER_Twerk()
    {
        kState = kTwerking;
        mTwerkTmStmp = Time.time;
        rTwerkHitbox.gameObject.SetActive(true);
        cRigid.velocity = Vector2.zero;
        transform.up = (transform.position - rOverseer.rPC.transform.position).normalized;
    }
    
    // Figure this out later.
    public void F_RunTwerking()
    {
        if(Time.time - mTwerkTmStmp > _twerkTime){
            kState = kShortRange;
            rTwerkHitbox.gameObject.SetActive(false);
        }
    }

    public void F_RunThrowing()
    {
        // If we can't see the player, start tracking again.
        Vector2 vDir = rOverseer.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC", "ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, Mathf.Infinity, mask);
        if(hit.collider == null){
            Debug.Log("NPC raycast hit null. Weird");
            return;
        }
        if(!hit.collider.GetComponent<PC_Cont>()){
            kState = kLongRange;
            return;
        }

        // If the player gets close, try to just chase them.
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _switchToCloseRange){
            kState = kShortRange;
            return;
        }
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) > _weaveThrowRange){
            kState = kLongRange;
            return;
        }

        // Otherwise keep tossing those weaves.
        cRigid.velocity = Vector2.zero;
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
        if(Time.time - mThrowTmStmp > _weaveThrowInterval){
            // Throw another weave.
            ThrowWeave();
        }
    }
    
    public void F_Animate()
    {
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(kState == kPoiseBroke){
            sRender.sprite = rHitstun;
        }else if(kState == kLongRange || kState == kShortRange){
            sRender.sprite = rMoving;
        }else if(kState == kTwerking){
            sRender.sprite = rTwerking;
        }else if(kState == kThrowing){
            sRender.sprite = rThrowing;
        }else if(kState == kTwerkPrep){
            sRender.sprite = rTwerkPrep;
        }else{
            Debug.Log("State not covered");
        }
    }

}
