using UnityEngine;

public class EN_HunterAnimator : MonoBehaviour
{
    public Sprite               rIdle;
    public Sprite               rChasing;
    public Sprite               rPain;
    public Sprite               rCannonCharge;
    public Sprite               rCannonJustFire;
    public Sprite               rLeapPrep;
    public Sprite               rLeaping;
    public Sprite               rBackSwingPrep;
    public Sprite               rBackSwingAtk;

    EN_Hunter                   cHunter;
    void Start()
    {
        cHunter = GetComponent<EN_Hunter>();
    }

    public void FAnimate()
    {   
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cHunter.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cHunter.mState){
            case EN_Hunter.STATE.LOOKING_FOR_VANTAGE_POINT: sRender.sprite = rIdle; break;
            case EN_Hunter.STATE.LONG_RANGE: sRender.sprite = rCannonCharge; break;
            case EN_Hunter.STATE.CLOSE_RANGE: sRender.sprite = rChasing; break;
            case EN_Hunter.STATE.PREP_LEAP: sRender.sprite = rLeapPrep; break;
            case EN_Hunter.STATE.LEAPING: sRender.sprite = rLeaping; break;
            case EN_Hunter.STATE.RECOVER_FROM_LEAP: sRender.sprite = rIdle; break;
            case EN_Hunter.STATE.FLYING_AFTER_DAMAGED: sRender.sprite = rPain; break;
            default: Debug.Log("state not covered"); break;
        }
    }
}
