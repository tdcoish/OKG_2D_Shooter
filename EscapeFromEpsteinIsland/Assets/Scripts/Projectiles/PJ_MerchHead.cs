/***************************************************************************************************
Dead simple. It's just the Medusa Head from Castlevania.

What if it's just a projectile? No, still need it to not get killed when it hits the edge of the stage.
***************************************************************************************************/
using UnityEngine;

public class PJ_MerchHead : PJ_Base
{
    public float                    _wavesPerSecond = 0.5f;
    public float                    _yHeight = 1f;
    public float                    mStartY;

    bool launched = false;

    public void F_LaunchMe(float startHeight, bool right)
    {
        mStartY = startHeight;
        Vector2 startPos = transform.position;

        // hack for the start x, but whatever.
        if(right){
            cRigid.velocity = Vector2.right * mProjD._spd;
            startPos.x = -10f;
        }else{
            cRigid.velocity = Vector2.right * mProjD._spd;
            startPos.x = 10f;
        }
        startPos.y = startHeight;
        transform.position = startPos;
    }

    void Update()
    {
        if(!launched){
            PC_Cont rPC = FindObjectOfType<PC_Cont>(); if(rPC == null) return;
            F_LaunchMe(rPC.transform.position.y, true);
            launched = true;
        }

        // for some reason, the Cos period is six seconds. No idea.
        float fudgeFactor = 6f * _wavesPerSecond;
        float newY = Mathf.Cos(Time.time * fudgeFactor) + mStartY;
        Vector2 newPos = transform.position;
        newPos.y = newY;
        transform.position = newPos;
    }

}
