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

        if(cPC.mMeleeMode){
            switch(cPC.mState)
            {
                case PC_Cont.STATE.IDLE: sRender.sprite = rIdle; break;
                case PC_Cont.STATE.RUNNING: sRender.sprite = rMove; break;
                case PC_Cont.STATE.SLASHING: sRender.sprite = rSlash; break;
                case PC_Cont.STATE.BATTACK_RECOVERY: sRender.sprite = rRecover; break;
            }
        }else{

            // Figure out if we just casted.
            PC_FireboltSpell s = GetComponent<PC_FireboltSpell>();
            if(s.mState == PC_FireboltSpell.STATE.UNREADY){
                sRender.sprite = rCastingAtk;
            }else{
                switch(cPC.mState)
                {
                    case PC_Cont.STATE.IDLE: sRender.sprite = rCastingIdle; break;
                    case PC_Cont.STATE.RUNNING: sRender.sprite = rCastingRun; break;
                }
            }
        }
    }

}
