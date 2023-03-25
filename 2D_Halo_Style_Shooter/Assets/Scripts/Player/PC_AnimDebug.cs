using UnityEngine;
using System.Collections.Generic;

public class PC_AnimDebug : MonoBehaviour
{
    public Sprite                   rIdleDown;
    public Sprite                   rIdleDownRight;
    public Sprite                   rIdleDownLeft;
    public Sprite                   rIdleRight;
    public Sprite                   rIdleLeft;
    public Sprite                   rIdleUpRight;
    public Sprite                   rIdleUpLeft;
    public Sprite                   rIdleUp;

    public Sprite                   rRunDown;
    public Sprite                   rRunDownRight;
    public Sprite                   rRunDownLeft;
    public Sprite                   rRunRight;
    public Sprite                   rRunLeft;
    public Sprite                   rRunUpRight;
    public Sprite                   rRunUpLeft;
    public Sprite                   rRunUp;

    public Sprite                   rWindupDown;
    public Sprite                   rWindupDownRight;
    public Sprite                   rWindupDownLeft;
    public Sprite                   rWindupRight;
    public Sprite                   rWindupLeft;
    public Sprite                   rWindupUpRight;
    public Sprite                   rWindupUpLeft;
    public Sprite                   rWindupUp;

    public Sprite                   rSlashDown;
    public Sprite                   rSlashDownRight;
    public Sprite                   rSlashDownLeft;
    public Sprite                   rSlashRight;
    public Sprite                   rSlashLeft;
    public Sprite                   rSlashUpRight;
    public Sprite                   rSlashUpLeft;
    public Sprite                   rSlashUp;

    
    public PC_Cont                  cPC;

    public void RUN_Start()
    {
        cPC = GetComponent<PC_Cont>();
    }


    // Selects appropriate sprite for PC. Need state.
    public void FRUN_Animation(DIRECTION heading, PC_Cont.STATE state)
    {
        SpriteRenderer sRender = cPC.GetComponent<SpriteRenderer>();

        if(state == PC_Cont.STATE.IDLE){
            switch(heading){
                case DIRECTION.DOWN: sRender.sprite = rIdleDown; break;
                case DIRECTION.DOWNRIGHT: sRender.sprite = rIdleDownRight; break;
                case DIRECTION.DOWNLEFT: sRender.sprite = rIdleDownLeft; break;
                case DIRECTION.RIGHT: sRender.sprite = rIdleRight; break;
                case DIRECTION.LEFT: sRender.sprite = rIdleLeft; break;
                case DIRECTION.UPRIGHT: sRender.sprite = rIdleUpRight; break;
                case DIRECTION.UPLEFT: sRender.sprite = rIdleUpLeft; break;
                case DIRECTION.UP: sRender.sprite = rIdleUp; break;
            }
        }else if (state == PC_Cont.STATE.RUNNING){
            switch(heading){
                case DIRECTION.DOWN: sRender.sprite = rRunDown; break;
                case DIRECTION.DOWNRIGHT: sRender.sprite = rRunDownRight; break;
                case DIRECTION.DOWNLEFT: sRender.sprite = rRunDownLeft; break;
                case DIRECTION.RIGHT: sRender.sprite = rRunRight; break;
                case DIRECTION.LEFT: sRender.sprite = rRunLeft; break;
                case DIRECTION.UPRIGHT: sRender.sprite = rRunUpRight; break;
                case DIRECTION.UPLEFT: sRender.sprite = rRunUpLeft; break;
                case DIRECTION.UP: sRender.sprite = rRunUp; break;
            }
        }else if (state == PC_Cont.STATE.WINDUP){
            switch(heading){
                case DIRECTION.DOWN: sRender.sprite = rWindupDown; break;
                case DIRECTION.DOWNRIGHT: sRender.sprite = rWindupDownRight; break;
                case DIRECTION.DOWNLEFT: sRender.sprite = rWindupDownLeft; break;
                case DIRECTION.RIGHT: sRender.sprite = rWindupRight; break;
                case DIRECTION.LEFT: sRender.sprite = rWindupLeft; break;
                case DIRECTION.UPRIGHT: sRender.sprite = rWindupUpRight; break;
                case DIRECTION.UPLEFT: sRender.sprite = rWindupUpLeft; break;
                case DIRECTION.UP: sRender.sprite = rWindupUp; break;
            }
        }else if (state == PC_Cont.STATE.SLASHING){
            switch(heading){
                case DIRECTION.DOWN: sRender.sprite = rSlashDown; break;
                case DIRECTION.DOWNRIGHT: sRender.sprite = rSlashDownRight; break;
                case DIRECTION.DOWNLEFT: sRender.sprite = rSlashDownLeft; break;
                case DIRECTION.RIGHT: sRender.sprite = rSlashRight; break;
                case DIRECTION.LEFT: sRender.sprite = rSlashLeft; break;
                case DIRECTION.UPRIGHT: sRender.sprite = rSlashUpRight; break;
                case DIRECTION.UPLEFT: sRender.sprite = rSlashUpLeft; break;
                case DIRECTION.UP: sRender.sprite = rSlashUp; break;
            }
        }

    }

}
