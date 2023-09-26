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
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cElite.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cElite.mState){
            case EN_Elite.STATE.LONG_RANGE_FIRING_SPOT: sRender.sprite = rStrafing; break;
            case EN_Elite.STATE.LOOKING_FOR_FIRING_SPOT: sRender.sprite = rPain; break;
            case EN_Elite.STATE.CLOSING: sRender.sprite = rChasing; break;
            case EN_Elite.STATE.PREP_MELEE: sRender.sprite = rSwingPrep; break;
            case EN_Elite.STATE.MELEEING: sRender.sprite = rSwingAtk; break;
            case EN_Elite.STATE.STUN: sRender.sprite = rStun; break;
            default: Debug.Log("state: " + cElite.mState + " not covered"); break;
        }
    }
}
