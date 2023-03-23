using UnityEngine;
using System.Collections.Generic;

public class PC_AnimDebug : MonoBehaviour
{
    public Sprite                   rIdleDown;
    public Sprite                   rIdleDownLeft;
    public Sprite                   rIdleDownRight;
    public Sprite                   rIdleLeft;
    public Sprite                   rIdleRight;
    public Sprite                   rIdleUpRight;
    public Sprite                   rIdleUpLeft;
    public Sprite                   rIdleUp;

    public PC_Cont                  cPC;

    public void RUN_Start()
    {
        cPC = GetComponent<PC_Cont>();
    }

    public enum DIRECTION{DOWN, DOWNRIGHT, DOWNLEFT, RIGHT, LEFT, UPRIGHT, UPLEFT, UP}
    class DotFloatText{
        public DotFloatText(float dot, DIRECTION dir){mDot = dot; mDir = dir;}
        public float mDot;
        public DIRECTION mDir;
    }
    // Selects appropriate sprite for PC
    public void FRUN_Animation(Vector2 msPos, Vector2 ourPos)
    {
        Vector2 dif = msPos - ourPos;
        dif = dif.normalized;

        List<DotFloatText> dots = new List<DotFloatText>();
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(0,-1)), DIRECTION.DOWN));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(1,-1).normalized), DIRECTION.DOWNRIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(-1,-1).normalized), DIRECTION.DOWNLEFT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(1,0)), DIRECTION.RIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(-1,0)), DIRECTION.LEFT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(1,1).normalized), DIRECTION.UPRIGHT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(-1,1).normalized), DIRECTION.UPLEFT));
        dots.Add(new DotFloatText(Vector2.Dot(dif, new Vector2(0,1)), DIRECTION.UP));

        int indLargest = -1;
        float largest = -10000f;
        for (int i=0; i<dots.Count; i++){
            if(dots[i].mDot > largest){
                largest = dots[i].mDot;
                indLargest = i;
            }
        }

        if(indLargest == -1){
            Debug.Log("No largest? That doesn't make sense");
            return;
        }

        SpriteRenderer sRender = cPC.GetComponent<SpriteRenderer>();
        switch(dots[indLargest].mDir){
            case DIRECTION.DOWN: sRender.sprite = rIdleDown; break;
            case DIRECTION.DOWNRIGHT: sRender.sprite = rIdleDownRight; break;
            case DIRECTION.DOWNLEFT: sRender.sprite = rIdleDownLeft; break;
            case DIRECTION.RIGHT: sRender.sprite = rIdleRight; break;
            case DIRECTION.LEFT: sRender.sprite = rIdleLeft; break;
            case DIRECTION.UPRIGHT: sRender.sprite = rIdleUpRight; break;
            case DIRECTION.UPLEFT: sRender.sprite = rIdleUpLeft; break;
            case DIRECTION.UP: sRender.sprite = rIdleUp; break;
        }
    }

}
