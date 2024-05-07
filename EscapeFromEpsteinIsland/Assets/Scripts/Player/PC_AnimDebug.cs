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
    
    public PC_Cont                  cPC;

    // Selects appropriate sprite for PC. Need state.
    public void FRUN_Animation()
    {
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
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

}
