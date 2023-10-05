using UnityEngine;

public class EN_EliteAnimator : MonoBehaviour
{
    public Sprite               rStrafing;
    public Sprite               rChasing;
    public Sprite               rPain;
    public Sprite               rSwingPrep;
    public Sprite               rSwingAtk;
    public Sprite               rGrenPrep;
    public Sprite               rGrenAtk;
    public Sprite               rStun;

    EN_Elite                    cElite;
    void Start()
    {
        cElite = GetComponent<EN_Elite>();
    }

    public void FAnimate()
    {   
        if(cElite == null){
            return;
        }
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cElite.kState == cElite.kLongRangeFiring){
            sRender.sprite = rStrafing;
        }else if(cElite.kState == cElite.kClosingToLongRangeFiringSPot){
            sRender.sprite = rChasing;
        }else if(cElite.kState == cElite.kSeekingLongRangeFiringSpot){
            sRender.sprite = rPain;
        }else if(cElite.kState == cElite.kClosing){
            sRender.sprite = rChasing;
        }else if(cElite.kState == cElite.kPrepMelee){
            sRender.sprite = rSwingPrep;
        }else if(cElite.kState == cElite.kMelee){
            sRender.sprite = rSwingAtk;
        }else if(cElite.kState == cElite.kRecMelee){
            sRender.sprite = rGrenAtk;
        }else if(cElite.kState == cElite.kStunned){
            sRender.sprite = rStun;   
        }else{
            Debug.Log("state: " + cElite.kState + " not covered"); 
        }
    }
}
