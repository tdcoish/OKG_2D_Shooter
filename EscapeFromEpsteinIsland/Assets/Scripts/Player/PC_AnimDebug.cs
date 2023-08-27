using UnityEngine;
using System.Collections.Generic;

public class PC_AnimDebug : MonoBehaviour
{
    public Sprite                   rIdle;
    public Sprite                   rMove;
    public Sprite                   rSlash;
    public Sprite                   rRecover;
    public Sprite                   rCastingIdle;
    public Sprite                   rCastingAtk;
    public Sprite                   rCastingRun;
    
    public PC_Cont                  cPC;

    public void RUN_Start()
    {
        cPC = GetComponent<PC_Cont>();
    }

    // Selects appropriate sprite for PC. Need state.
    public void FRUN_Animation()
    {
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cPC.mHeading);
        // Should probably have other variables that show if we recently fired. Things like that.
        switch(cPC.mState)
        {
            case PC_Cont.STATE.IDLE: sRender.sprite = rIdle; break;
            case PC_Cont.STATE.RUNNING: sRender.sprite = rMove; break;
            case PC_Cont.STATE.SLASHING: sRender.sprite = rSlash; break;
            case PC_Cont.STATE.BATTACK_RECOVERY: sRender.sprite = rRecover; break;
            // sRender.sprite = rCastingIdle;
        }
    }

}
