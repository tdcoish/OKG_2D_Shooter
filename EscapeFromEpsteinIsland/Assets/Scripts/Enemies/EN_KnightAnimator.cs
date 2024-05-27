using UnityEngine;

public class EN_KnightAnimator : MonoBehaviour
{
    public Sprite               rHunting;
    public Sprite               rBoomCharge;
    public Sprite               rBoomRec;
    public Sprite               rSlashCharge;
    public Sprite               rSlashCutting;
    public Sprite               rSlashRec;
    public Sprite               rStunRec;

    EN_Knight                   cKnight;
    void Start()
    {
        cKnight = GetComponent<EN_Knight>();
    }

    public void FAnimate()
    {   
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cKnight == null){
            Debug.Log("Knight animator: Knight entity is null for some reason.");
            return;
        }
            if(cKnight.kState == cKnight.kHunting){
                sRender.sprite = rHunting;
            }else if(cKnight.kState == cKnight.kBoomerCharge){
                sRender.sprite = rBoomCharge;
            }else if(cKnight.kState == cKnight.kBoomerRecover){
                sRender.sprite = rBoomRec;
            }else if(cKnight.kState == cKnight.kSlashCharge){
                sRender.sprite = rSlashCharge;
            }else if(cKnight.kState == cKnight.kSlashCutting){
                sRender.sprite = rSlashCutting;
            }else if(cKnight.kState == cKnight.kSlashRecover){
                sRender.sprite = rSlashRec;
            }else if(cKnight.kState == cKnight.kPoiseBroke){
                sRender.sprite = rStunRec;
            }
    }
}
