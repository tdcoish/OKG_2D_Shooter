using UnityEngine;

public class PC_Shotgun : MonoBehaviour
{
    public enum STATE {READY, UNREADY}
    public STATE                        mState = STATE.READY;

    PC_Cont                             cPC;

    public float                        _fireInterval;
    public float                        mFireTmStmp;

    public int                          _pelletsFiredPerBlast = 5;
    public float                        _spread = 30f;
    public float                        _pelletInitialSpeed = 5f;
    public float                        _pelletLifetime = 1.5f;
    
    public PJ_PC_ShotgunPellet          PF_Pellet;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
    }

    public void FRunShotgunUpdate()
    {
        if(mState == STATE.UNREADY){
            if(Time.time - mFireTmStmp > _fireInterval){
                mState = STATE.READY;
            }
        }
    }

    public bool FAttemptFire(Vector3 msPos, Vector3 shotPoint)
    {
        if(mState == STATE.READY){
            if(cPC.mCurEnergy < cPC._energyDrainPerShotgunBlast){
                return false;
            }
            
            // We need to shoot them out in a blast. For now don't make it random, just fire out five balls.
            void ShootPellet(Vector3 vDir)
            {
                PJ_PC_ShotgunPellet p = Instantiate(PF_Pellet, shotPoint, transform.rotation);
                p.cRigid.velocity = vDir * _pelletInitialSpeed;
                p.mStartSpd = _pelletInitialSpeed;
                p.vDir = vDir.normalized;
                p.mCreatedTimeStamp = Time.time;
                p.mLifespan = _pelletLifetime;
            }

            Vector3 vDif = (msPos - shotPoint).normalized;
            float step = _spread / (float)(_pelletsFiredPerBlast-1);
            float rightMost = _spread / 2f * -1f;
            for(int i=0; i<_pelletsFiredPerBlast; i++){
                ShootPellet(Quaternion.Euler(0f,0f, rightMost + (step * i)) * vDif);
            }

            mFireTmStmp = Time.time;
            mState = STATE.UNREADY;
            return true;
        }
        return false;
    }
}
