using UnityEngine;

public class PC_Needler : MonoBehaviour
{
    public enum STATE {READY, UNREADY}
    public STATE                        mState = STATE.READY;

    PC_Cont                             cPC;

    public float                        _fireInterval;
    public float                        mFireTmStmp;
    public float                        _needleSpd = 1.5f;
    public float                        _needleTurnRate = 30f;
    public float                        _needleTimeBeforeDetonation = 1f;
    public float                        _needleLifespan = 4f;
    
    public PJ_PC_Needle                 PF_Needle;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
    }

    public void FRunNeedlerUpdate()
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
            if(cPC.mCurEnergy < cPC._energyDrainPerNeedleFire){
                return false;
            }

            // Basically we cast around until we find an actor, and then lock on them.
            Actor actorToTarget;
            Actor[] actors = FindObjectsOfType<Actor>();
            if(actors.Length <= 1){
                actorToTarget = null;
            }else{
                float smallestDis = 10000f;
                int indClosest = -1;
                for(int i=0; i<actors.Length; i++){
                    if(actors[i].GetComponent<PC_Cont>()) continue;

                    float tempDis = Vector2.Distance(msPos, actors[i].transform.position);
                    if(tempDis < smallestDis){
                        smallestDis = tempDis;
                        indClosest = i;
                    }
                }

                actorToTarget = actors[indClosest];
            }

            Vector3 vDif = (msPos - shotPoint).normalized;
            PJ_PC_Needle n = Instantiate(PF_Needle, shotPoint, transform.rotation);
            n.mLifespan = _needleLifespan;
            n.mCreatedTimeStamp = Time.time;
            n.mTarget = actorToTarget;
            n.cRigid.velocity = vDif * _needleSpd;

            mFireTmStmp = Time.time;
            mState = STATE.UNREADY;
            return true;
        }
        return false;
    }
}
