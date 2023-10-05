using UnityEngine;

public class EN_BeamerAnimator : MonoBehaviour
{
    public Sprite                       rHunting;
    public Sprite                       rCharging;
    public Sprite                       rStunned;
    public Sprite                       rSettingUpShot;
    public Sprite                       rIdle;

    EN_Beamer                           cBeamer;
    void Start()
    {
        cBeamer = GetComponent<EN_Beamer>();
    }

    public void FAnimate()
    {
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        // transform.up = h.PointToLookAtAlongHeading(cBeamer.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cBeamer.mState){
            case EN_Beamer.STATE.LOOKING_FOR_VANTAGE: sRender.sprite = rHunting; break;
            case EN_Beamer.STATE.SETTING_UP_SHOT: sRender.sprite = rSettingUpShot; break;
            case EN_Beamer.STATE.CHARGING: sRender.sprite = rCharging; break;
            case EN_Beamer.STATE.COOLDOWN: sRender.sprite = rIdle; break;
            case EN_Beamer.STATE.STUNNED: sRender.sprite = rStunned; break;
            default: Debug.Log("state: " + cBeamer.mState + " not covered"); break;
        }
    }

}
