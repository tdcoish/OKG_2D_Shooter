using UnityEngine;
using System.Collections.Generic;

public class PC_AnimDebug : MonoBehaviour
{
    public Sprite                   rIdle;
    public Sprite                   rMove;
    public Sprite                   rWindup;
    public Sprite                   rSlash;
    public Sprite                   rRecover;
    public Sprite                   rSlideCharge;
    public Sprite                   rSliding;
    public Sprite                   rSlideEnd;
    public Sprite                   rCastingIdle;
    public Sprite                   rCastingAtk;
    public Sprite                   rCastingRun;
    public Sprite                   rThrowing;
    public Sprite                   rShooting;
    public SpriteRenderer           sRender;

    public SpriteRenderer           sShields;

    public float                    _hitFlashTime = 0.2f;
    
    public PC_Cont                  cPC;

    // Selects appropriate sprite for PC. Need state.
    public void FRUN_Animation()
    {
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        // transform.up = h.PointToLookAtAlongHeading(cPC.mHeading);
        // Should probably have other variables that show if we recently fired. Things like that.
        if(cPC.mState == PC_Cont.STATE.IDLE){
            sRender.sprite = rCastingIdle;
        }else if(cPC.mState == PC_Cont.STATE.RUNNING){
            sRender.sprite = rCastingRun;
        }else if(cPC.mState == PC_Cont.STATE.WINDUP){
            sRender.sprite = rWindup;
        }else if(cPC.mState == PC_Cont.STATE.SLASHING){
            sRender.sprite = rSlash;
        }else if(cPC.mState == PC_Cont.STATE.BATTACK_RECOVERY){
            sRender.sprite = rRecover;
        }else if(cPC.mState == PC_Cont.STATE.THROW_RECOVERY){
            sRender.sprite = rThrowing;
        }else if(cPC.mState == PC_Cont.STATE.SHOT_RECOVERY){
            sRender.sprite = rShooting;
        }

    }

    public void FRUN_Shields(float percent)
    {        
        sShields.color = new Color(1f, 1f, 1f, percent);
    }

    public void FFlashDependingOnLastHitTime(float lastHitTmStmp, float shieldsAmount)
    {
        if(Time.time - lastHitTmStmp < _hitFlashTime){
            float percOver = (Time.time - lastHitTmStmp) / _hitFlashTime;
            if(shieldsAmount > 0f){
                sShields.color = new Color(1f, percOver, percOver, 1f);
            }else{
                sRender.color = new Color(1f, percOver, percOver, 1f);
            }
            Debug.Log("Within hit time");
            Debug.Log("Percent: " + percOver);
            Debug.Log("Time since last hit: " + (Time.time - lastHitTmStmp));
        }
    }

}
