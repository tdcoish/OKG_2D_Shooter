/****************************************************************************************************
Since Jequeerus never moves away from the player, we can just give him a shambling state and that 
works for all distances.
****************************************************************************************************/

using UnityEngine;

public class EN_Jequeerus : EN_Base
{
    public uint                         kShambling = 1<<2;
    public uint                         kFiring = 1<<3;
    public uint                         kReloading = 1<<4;
    public uint                         kFlaming = 1<<5;
    public uint                         kFlameRec = 1<<6;
    public uint                         kMeleeWindup = 1<<7;
    public uint                         kMeleeThrust = 1<<8;
    public uint                         kMeleeThrustRec = 1<<9;
    public uint                         kMeleeSlash = 1<<10;
    public uint                         kMeleeSlashRec = 1<<11;
    public uint                         kFlamePrep = 1<<12;

    public Sprite                       rShambing;
    public Sprite                       rHitstun;
    public Sprite                       rFiring;
    public Sprite                       rReloading;
    public Sprite                       rFlaming;
    public Sprite                       rFlameRec;
    public Sprite                       rFlamePrep;
    public Sprite                       rMeleeWindup;
    public Sprite                       rMeleeThrust;
    public Sprite                       rMeleeThrustRec;
    public Sprite                       rMeleeSlash;
    public Sprite                       rMeleeSlashRec;

    public float                        _maxShotDistance = 10f;
    public int                          _shotsInSalvo = 8;
    public float                        mShotCounter;
    public float                        _fireInterval = 0.1f;
    public float                        mFireTmStmp;
    public float                        _reloadTime = 2f;
    public float                        mReloadTmStmp;
    public float                        _flamePrepTime = 0.5f;
    public float                        mFlamePrepTmStmp;
    public float                        _flameSpreadTriggerDis = 4f;
    public float                        _shortRangeDis = 3f;
    public float                        _flameShotInterval = 0.1f;
    public float                        mFlameShotTmStmp;
    public float                        _flameSpreadAngleBetweenShots = 2f;
    public float                        _flameBallsPerSalvo = 20;
    public float                        mFlameShotCounter;
    public float                        _flamingRecTime = 1f;
    public float                        mFlameRecTmStmp;
    public Vector2                      vFlamingCenterDir;

    public Vector2                      vMeleeThrustDir;
    public float                        _meleeWindupTime = 0.5f;
    public float                        mMeleeWindupTmStmp;
    public float                        _meleeThrustSpd = 2f;
    public float                        _meleeThrustTime = 0.3f;
    public float                        mMeleeThrustTmStmp;
    public float                        _meleeThrustRec = 0.4f;
    public float                        mMeleeThrustRecTmStmp;
    public float                        _meleeSlashTime = 0.1f;
    public float                        mMeleeSlashTmStmp;
    public float                        _meleeRecTime = 0.5f;
    public float                        mMeleeRecTmStmp;
    public float                        _meleeTriggerDis = 1f;

    public PJ_EN_Plasmoid               PF_HandgunBullet;
    public PJ_EN_Plasmoid               PF_FlamePellet;

    public EN_JeqKnifeHitbox            rThrustHitBox;
    public EN_JeqKnifeHitbox            rSlashHitBox;

    public override void F_CharSpecStart()
    {
        kState = kShambling;
    }

    public override void F_CharSpecUpdate()
    {
        if(kState != kMeleeThrust) rThrustHitBox.gameObject.SetActive(false);
        if(kState != kMeleeSlash) rSlashHitBox.gameObject.SetActive(false);

        if(kState == kShambling){
            F_RunShambling();
        }else if(kState == kFiring){
            F_RunFiring();
        }else if(kState == kReloading){
            F_RunReloading();
        }else if(kState == kFlamePrep){
            F_RunFlamePrep();
        }else if(kState == kFlaming){
            F_RunFlaming();   
        }else if(kState == kFlameRec){
            F_RunFlameRec();
        }else if(kState == kMeleeWindup){
            F_RunMeleeWindup();
        }else if(kState == kMeleeThrust){
            F_RunMeleeThrust();
        }else if(kState == kMeleeThrustRec){
            F_RunMeleeThrustRec();
        }else if(kState == kMeleeSlash){
            F_RunMeleeSlash();
        }else if(kState == kMeleeSlashRec){
            F_RunMeleeRecover();
        }else if(kState == kPoiseBroke){
            F_RunStunRecovery();
        }

        F_Animate();
    }

    public override void EXIT_PoiseBreak()
    {
        kState = kShambling;
        mShotCounter = 0;
        mFlameShotCounter = 0;
    }

    public void F_RunShambling()
    {
        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(dis < _meleeTriggerDis){
            ENTER_MeleeWindup();
            return;
        }

        if(dis < _flameSpreadTriggerDis && dis > _shortRangeDis && F_CanSeePlayer()){
            ENTER_Flaming();
            return;
        }

        if(F_CanSeePlayer() && dis < _maxShotDistance && dis > _shortRangeDis){
            kState = kFiring;
            return;
        }

        MoveToPlayer();
    }

    public void ENTER_MeleeWindup()
    {
        kState = kMeleeWindup;
        vMeleeThrustDir = (rOverseer.rPC.transform.position - transform.position).normalized;
        cRigid.velocity = Vector2.zero;
        mMeleeWindupTmStmp = Time.time;
    }

    public void F_RunMeleeWindup()
    {
        if(Time.time - mMeleeWindupTmStmp > _meleeWindupTime){
            // Now we thrust.
            cRigid.velocity = _meleeThrustSpd * vMeleeThrustDir;
            mMeleeThrustTmStmp = Time.time;
            kState = kMeleeThrust;
            rThrustHitBox.gameObject.SetActive(true);
        }
    }

    public void F_RunMeleeThrust()
    {
        if(Time.time - mMeleeThrustTmStmp > _meleeThrustTime){
            kState = kMeleeThrustRec;
            mMeleeThrustRecTmStmp = Time.time;
            cRigid.velocity = Vector2.zero;
        }
    }
    
    public void F_RunMeleeThrustRec()
    {
        if(Time.time - mMeleeThrustRecTmStmp > _meleeThrustRec){
            kState = kMeleeSlash;
            mMeleeSlashTmStmp = Time.time;
            transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
            rSlashHitBox.gameObject.SetActive(true);
        }
    }

    public void F_RunMeleeSlash()
    {
        if(Time.time - mMeleeSlashTmStmp > _meleeSlashTime){
            kState = kMeleeSlashRec;
            mMeleeRecTmStmp = Time.time;
        }
    }

    public void F_RunMeleeRecover()
    {
        if(Time.time - mMeleeRecTmStmp > _meleeRecTime){
            kState = kShambling;
        }
    }

    public void F_RunFiring()
    {
        float dis = Vector3.Distance(rOverseer.rPC.transform.position, transform.position);
        if(!F_CanSeePlayer() || dis > _maxShotDistance){
            kState = kShambling;
            return;
        }

        if(dis < _flameSpreadTriggerDis){
            ENTER_Flaming();
            return;
        }

        if(Time.time - mFireTmStmp > _fireInterval){
            PJ_EN_Plasmoid p = Instantiate(PF_HandgunBullet, transform.position, transform.rotation);
            p.FShootAt((rOverseer.rPC.transform.position - transform.position), gameObject);
            mFireTmStmp = Time.time;
            mShotCounter++;
            if(mShotCounter >= _shotsInSalvo){
                kState = kReloading;
                mReloadTmStmp = Time.time;
            }
        }

        cRigid.velocity = Vector2.zero;
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }

    public void F_RunReloading()
    {
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _flameSpreadTriggerDis){
            ENTER_Flaming();
            return;
        }

        if(Time.time - mReloadTmStmp > _reloadTime){
            kState = kShambling;
            mShotCounter = 0;
        }
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }

    public void ENTER_Flaming()
    {
        vFlamingCenterDir = (rOverseer.rPC.transform.position - transform.position).normalized;
        kState = kFlamePrep;
        cRigid.velocity = Vector2.zero;
        mFlamePrepTmStmp = Time.time;
        transform.up = (rOverseer.rPC.transform.position - transform.position).normalized;
    }

    public void F_RunFlamePrep()
    {
        if(Time.time - mFlamePrepTmStmp > _flamePrepTime){
            kState = kFlaming;
        }
    }

    public void F_RunFlaming()
    {
        if(Time.time - mFlameShotTmStmp > _flameShotInterval){
            PJ_EN_Plasmoid p = Instantiate(PF_FlamePellet, transform.position, transform.rotation);

            // Figure out where to shoot the pellet.
            float totalAngleSpread = _flameSpreadAngleBetweenShots * _flameBallsPerSalvo;
            float curAngle = totalAngleSpread / 2f;
            curAngle -= _flameSpreadAngleBetweenShots * mFlameShotCounter;
            float radiansOfTurn = curAngle*Mathf.Deg2Rad;
            Vector2 vDir = vFlamingCenterDir;
            vDir.x = vFlamingCenterDir.x * Mathf.Cos(radiansOfTurn) - vDir.y * Mathf.Sin(radiansOfTurn);
            vDir.y = vFlamingCenterDir.x * Mathf.Sin(radiansOfTurn) + vDir.y * Mathf.Cos(radiansOfTurn);
            p.cRigid.velocity = vDir.normalized * p.mProjD._spd;
            p.transform.up = p.cRigid.velocity.normalized;
            p.mProjD.rOwner = gameObject;

            mFlameShotTmStmp = Time.time;
            mFlameShotCounter++;
            if(mFlameShotCounter >= _flameBallsPerSalvo){
                kState = kFlameRec;
                mFlameRecTmStmp = Time.time;
            }
        }
    }

    public void F_RunFlameRec()
    {
        mFlameShotCounter = 0;
        if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _shortRangeDis){
            kState = kShambling;
            return;
        }

        if(Time.time - mFlameRecTmStmp > _flamingRecTime){
            kState = kShambling;
        }
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
            sRender.sprite = rShambing;
        }else if(kState == kFiring){
            sRender.sprite = rFiring;
        }else if(kState == kReloading){
            sRender.sprite = rReloading;
        }else if(kState == kFlaming){
            sRender.sprite = rFlaming;
        }else if(kState == kFlaming){
            sRender.sprite = rFlaming;
        }else if(kState == kFlameRec){
            sRender.sprite = rFlameRec;
        }else if(kState == kFlamePrep){
            sRender.sprite = rFlamePrep;
        }else if(kState == kMeleeWindup){
            sRender.sprite = rMeleeWindup;
        }else if(kState == kMeleeThrust){
            sRender.sprite = rMeleeThrust;
        }else if(kState == kMeleeThrustRec){
            sRender.sprite = rMeleeThrustRec;
        }else if(kState == kMeleeSlash){
            sRender.sprite = rMeleeSlash;
        }else if(kState == kMeleeSlashRec){
            sRender.sprite = rMeleeSlashRec;
        }else{
            Debug.Log("State not covered");
        }
    }
    
}
