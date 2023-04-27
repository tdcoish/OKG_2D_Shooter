using UnityEngine;

public class PJ_PC_ShotgunPellet : MonoBehaviour
{
    // Will be set by the shotgun
    [HideInInspector]
    public float                mLifespan;
    [HideInInspector]
    public float                mStartSpd;
    [HideInInspector]
    public float                mCurSpd;
    [HideInInspector]
    public float                mCreatedTimeStamp;
    public Rigidbody2D          cRigid;
    [HideInInspector]
    public Vector2              vDir;

    void Update()
    {
        float percentLifetimeRemaining = (mLifespan - (Time.time - mCreatedTimeStamp)) / mLifespan;
        mCurSpd = mStartSpd * percentLifetimeRemaining;
        cRigid.velocity = vDir * mCurSpd;

        if(Time.time - mCreatedTimeStamp > mLifespan){
            Destroy(gameObject);
        }
    }
}
