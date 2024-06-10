/**************
I want it to throw a cloud of testosterone pills from a distance, and do a complicated attack pattern
where it deliberately misses left, then right. 

**************/
using UnityEngine;

public class EN_FtM : EN_Base
{
    public uint                         kShambling = 1<<2;
    public uint                         kThrowPrep = 1<<3;
    public uint                         kThrowRec = 1<<4;
    public uint                         kMeleeWindupRight = 1<<5;
    public uint                         kMeleeThrustRight = 1<<6;
    public uint                         kMeleeThrustRightRec = 1<<7;
    public uint                         kMeleeWindupLeft = 1<<8;
    public uint                         kMeleeThrustLeft = 1<<9;
    public uint                         kMeleeThrustLeftRec = 1<<10;

    public Sprite                       rHitstun;
    public Sprite                       rShambling;
    public Sprite                       rThrowPrep;
    public Sprite                       rThrowRec;
    public Sprite                       rWindupRight;
    public Sprite                       rThrustRight;
    public Sprite                       rThrustRightRec;    
    public Sprite                       rWindupLeft;
    public Sprite                       rThrustLeft;
    public Sprite                       rThrustLeftRec;

    public float                        _maxShotDistance = 10f;
    public float                        _shortChaseDistance = 2.5f;
    public float                        _throwPrepTime = 2f;
    public float                        mThrowPrepTmStmp;
    public float                        _throwRecTime = 0.5f;
    public float                        mThrowRecTmStmp;
    public int                          _numPellets = 10;
    public float                        _angleBetweenPellets = 2f;
    public float                        _pelletRandomSpeedVariation = 1f;
    public float                        _pelletRandomSpread = 1f;

    public float                        _meleeTriggerDis = 1f;
    public float                        _meleeWindupTime = 0.5f;
    public float                        _secondWindupTime = 0.1f;
    public float                        mMeleeWindupTmStmp;
    public float                        _meleeMissFactor = 30f;
    public float                        _meleeThrustSpd = 2f;
    public float                        _meleeThrustTime = 0.3f;
    public float                        mMeleeThrustTmStmp;
    public float                        _meleeThrustRec = 0.4f;
    public float                        mMeleeThrustRecTmStmp;
    public float                        _meleeRecTime = 0.5f;
    public Vector2                      vSlashDir;

    public PJ_EN_TPill                  PF_TestosteronePills;

    public EN_FTM_FistHitbox            rThrustRightHitBox;
    public EN_FTM_FistHitbox            rThrustLeftHitBox;

    public override void F_CharSpecStart()
    {
        kState = kShambling;
    }

    public override void F_CharSpecUpdate()
    {
        if(kState != kMeleeThrustLeft) rThrustLeftHitBox.gameObject.SetActive(false);
        if(kState != kMeleeThrustRight) rThrustRightHitBox.gameObject.SetActive(false);

        if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }else if(kState == kShambling){
            F_RunShambling();
        }else if(kState == kThrowPrep){
            F_RunThrowPrep();
        }else if(kState == kThrowRec){
            F_RunThrowRec();
        }else if(kState == kMeleeWindupRight){
            F_RunWindupRight();
        }else if(kState == kMeleeThrustRight){
            F_RunRightThrust();
        }else if(kState == kMeleeThrustRightRec){
            F_RunRightThrustRec();
        }else if(kState == kMeleeWindupLeft){
            F_RunWindupLeft();
        }else if(kState == kMeleeThrustLeft){
            F_RunLeftThrust();
        }else if(kState == kMeleeThrustLeftRec){
            F_RunLeftThrustRec();
        }

        F_Animate();
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kShambling;
    }

    public void F_RunShambling()
    {
        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(dis < _meleeTriggerDis){
            ENTER_MeleeWindupRight();
            return;
        }

        if(F_CanSeePlayer() && dis < _maxShotDistance && dis > _shortChaseDistance){
            kState = kThrowPrep;
            mThrowPrepTmStmp = Time.time;
            return;
        }

        MoveToPlayer();
    }

    // The slash direction is set when we start the windup.
    public Vector2 FindSlashDir(float missFactor)
    {
        Vector2 vDirToPC = (rOverseer.rPC.transform.position - transform.position).normalized;
        Vector2 vDir = vDirToPC;
        float radiansOfTurn = _meleeMissFactor * Mathf.Deg2Rad;
        vDir.x = vDirToPC.x * Mathf.Cos(radiansOfTurn) - vDirToPC.y * Mathf.Sin(radiansOfTurn);
        vDir.y = vDirToPC.x * Mathf.Sin(radiansOfTurn) + vDirToPC.y * Mathf.Cos(radiansOfTurn);

        return vDir; 
    }
    
    public void ENTER_MeleeWindupRight()
    {
        kState = kMeleeWindupRight;
        cRigid.velocity = Vector2.zero;
        mMeleeWindupTmStmp = Time.time;

        vSlashDir = FindSlashDir(_meleeMissFactor);
    }

    public void F_RunWindupRight()
    {
        if(Time.time - mMeleeWindupTmStmp > _meleeWindupTime){
            mMeleeThrustTmStmp = Time.time;
            kState = kMeleeThrustRight;
            cRigid.velocity = _meleeThrustSpd * vSlashDir;
            rThrustRightHitBox.gameObject.SetActive(true);
            transform.up = vSlashDir;
        }
    }

    public void F_RunRightThrust()
    {
        if(Time.time - mMeleeThrustTmStmp > _meleeThrustTime){
            kState = kMeleeThrustRightRec;
            mMeleeThrustRecTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            rThrustRightHitBox.gameObject.SetActive(false);
        }
    }
    
    public void F_RunRightThrustRec()
    {
        if(Time.time - mMeleeThrustRecTmStmp > _meleeThrustRec){
            kState = kMeleeWindupLeft;
            cRigid.velocity = Vector2.zero;
            mMeleeWindupTmStmp = Time.time;

            vSlashDir = FindSlashDir(_meleeMissFactor * -1f);
        }
    }

    public void F_RunWindupLeft()
    {
        if(Time.time - mMeleeWindupTmStmp > _secondWindupTime){
            mMeleeThrustTmStmp = Time.time;
            kState = kMeleeThrustLeft;
            cRigid.velocity = _meleeThrustSpd * vSlashDir;
            rThrustLeftHitBox.gameObject.SetActive(true);
            transform.up = vSlashDir;
        }
    }

    public void F_RunLeftThrust()
    {
        if(Time.time - mMeleeThrustTmStmp > _meleeThrustTime){
            kState = kMeleeThrustLeftRec;
            mMeleeThrustRecTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
            rThrustLeftHitBox.gameObject.SetActive(false);
        }
    }
    
    public void F_RunLeftThrustRec()
    {
        if(Time.time - mMeleeThrustRecTmStmp > _meleeRecTime){
            kState = kShambling;
        }
    }

    public void F_RunThrowPrep()
    {
        // Have to figure out how they shoot.

        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(!F_CanSeePlayer() || dis > _maxShotDistance || dis < _shortChaseDistance){
            kState = kShambling;
            return;
        }

        if(dis < _meleeTriggerDis){
            ENTER_MeleeWindupRight();
            return;
        }

        // How is it going to throw this cloud of testosterone pills.
        // Let's just not make it random to start, to get something close to what we want.
        if(Time.time - mThrowPrepTmStmp > _throwPrepTime){
            Vector2 vDirToPC = (rOverseer.rPC.transform.position - transform.position).normalized;
            for(int i=0; i<_numPellets; i++){
                PJ_EN_TPill p = Instantiate(PF_TestosteronePills, transform.position, transform.rotation);
                float randSpread = Random.Range(-0.5f * _pelletRandomSpread, 0.5f * _pelletRandomSpread);
                float radiansOfTurn = (_angleBetweenPellets * Mathf.Deg2Rad * _numPellets/2f) - (_angleBetweenPellets * Mathf.Deg2Rad * i) + randSpread;
                Vector2 vDir = vDirToPC;
                vDir.x = vDirToPC.x * Mathf.Cos(radiansOfTurn) - vDir.y * Mathf.Sin(radiansOfTurn);
                vDir.y = vDirToPC.x * Mathf.Sin(radiansOfTurn) + vDir.y * Mathf.Cos(radiansOfTurn);
                p.FShootAt(vDir, gameObject);

                // Fuck it. We're starting off with a random value for speed.
                float randSpdChange = Random.Range(-0.5f*_pelletRandomSpeedVariation, 0.5f*_pelletRandomSpeedVariation);
                p.cRigid.velocity = p.cRigid.velocity.normalized * (p.mProjD._spd + randSpdChange);
            }

            mThrowRecTmStmp = Time.time;
            kState = kThrowRec;
        }

        cRigid.velocity = Vector2.zero;
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }

    public void F_RunThrowRec()
    {
        if(Time.time - mThrowRecTmStmp > _throwRecTime){
            kState = kShambling;
        }
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }

    public void MoveToPlayer()
    {
        if(!F_CanSeePlayer()){
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
            cRigid.velocity = (rOverseer.rPC.transform.position - transform.position).normalized * _spd;
        }

        transform.up = cRigid.velocity.normalized; 
    }
    
    public void F_Animate()
    {
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(kState == kPoiseBroke){
            sRender.sprite = rHitstun;
        }else if(kState == kShambling){
            sRender.sprite = rShambling;
        }else if(kState == kThrowPrep){
            sRender.sprite = rThrowPrep;
        }else if(kState == kThrowRec){
            sRender.sprite = rThrowRec;
        }else if(kState == kMeleeWindupRight){
            sRender.sprite = rWindupRight;
        }else if(kState == kMeleeThrustRight){
            sRender.sprite = rThrustRight;
        }else if(kState == kMeleeThrustRightRec){
            sRender.sprite = rThrustRightRec;
        }else if(kState == kMeleeWindupLeft){
            sRender.sprite = rWindupLeft;
        }else if(kState == kMeleeThrustLeft){
            sRender.sprite = rThrustLeft;
        }else if(kState == kMeleeThrustLeftRec){
            sRender.sprite = rThrustLeftRec;
        }else{
            Debug.Log("State not covered");
        }
    }
}
