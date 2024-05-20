using UnityEngine;


public class UI_WepImgBack : MonoBehaviour
{
    public Sprite                       rUnselected;
    public Sprite                       rSelected;
    public SpriteRenderer               sRender;

    public void F_SetActiveSprite(bool active)
    {
        if(active){
            sRender.sprite = rSelected;
        }else{
            sRender.sprite = rUnselected;
        }
    }
}
