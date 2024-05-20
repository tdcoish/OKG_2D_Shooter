/*************
The shots that radiate outwards from the center of the player periodically. Slows over time.
*************/
using UnityEngine;

public class PJ_PC_RadShot : PJ_Base
{
    public float                    mCreatedTmStmp;
    void Update()
    {
        // Unlike traditional projectiles, this starts out super fast, then decays in speed to zero
        // as it gets closer to death.
        float percentDone = (Time.time - mCreatedTmStmp) / mProjD._lifespan;
        float inversePercent = 1f - percentDone;
        cRigid.velocity = (cRigid.velocity.normalized) * inversePercent * mProjD._spd;
    }
}
