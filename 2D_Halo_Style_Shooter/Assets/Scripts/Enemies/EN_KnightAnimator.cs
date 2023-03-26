using UnityEngine;

public class EN_KnightAnimator : MonoBehaviour
{
    public Sprite               rHunting;
    public Sprite               rBoomCharge;
    public Sprite               rBoomRec;
    public Sprite               rSlashCharge;
    public Sprite               rSlashCutting;
    public Sprite               rSlashRec;

    EN_Knight                   cKnight;
    void Start()
    {
        cKnight = GetComponent<EN_Knight>();
    }

    public void FAnimate()
    {   
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cKnight.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cKnight.mState){
            case EN_Knight.STATE.HUNTING: sRender.sprite = rHunting; break;
            case EN_Knight.STATE.BOOMER_CHARGE: sRender.sprite = rBoomCharge; break;
            case EN_Knight.STATE.BOOMER_RECOVER: sRender.sprite = rBoomRec; break;
            case EN_Knight.STATE.SLASH_CHARGE: sRender.sprite = rSlashCharge; break;
            case EN_Knight.STATE.SLASH_CUTTING: sRender.sprite = rSlashCutting; break;
            case EN_Knight.STATE.SLASH_RECOVER: sRender.sprite = rSlashRec; break;
        }
    }
}