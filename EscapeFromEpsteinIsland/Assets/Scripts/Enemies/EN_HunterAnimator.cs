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
        if(cHunter == null) return;
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cHunter.kState == cHunter.kLookingForVantagePoint){
            sRender.sprite = rIdle;
        }else if(cHunter.kState == cHunter.kLongRange){
            sRender.sprite = rCannonCharge;
        }else if(cHunter.kState == cHunter.kCloseRange){
            sRender.sprite = rChasing;
        }else if(cHunter.kState == cHunter.kPrepLeap){
            sRender.sprite = rLeapPrep;
        }else if(cHunter.kState == cHunter.kLeaping){
            sRender.sprite = rLeaping;
        }else if(cHunter.kState == cHunter.kRecoveringFromLeap){
            sRender.sprite = rIdle;
        }else if(cHunter.kState == cHunter.kFlyingAfterDamaged){
            sRender.sprite = rPain;
        }else if(cHunter.kState == cHunter.kPoiseBroke){
            sRender.sprite = rPain;
        }else{
            Debug.Log("state not covered");
            Debug.Log("State: " + cHunter.kState);
        }
    }
}
