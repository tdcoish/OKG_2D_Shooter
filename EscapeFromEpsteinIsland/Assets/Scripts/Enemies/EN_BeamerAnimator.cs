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
        if(cBeamer == null) return;

        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
            if(cBeamer.kState == cBeamer.kLookingForVantage){
                sRender.sprite = rHunting;
            }else if(cBeamer.kState == cBeamer.kSettingUpShot){
                sRender.sprite = rSettingUpShot;
            }else if(cBeamer.kState == cBeamer.kChargingShot){
                sRender.sprite = rCharging;
            }else if(cBeamer.kState == cBeamer.kCooldown){
                sRender.sprite = rIdle;
            }else if(cBeamer.kState == cBeamer.kPoiseBroke){
                sRender.sprite = rStunned;
            }else{
                Debug.Log("state: " + cBeamer.kState + " not covered");
                Debug.Log("Cooldown state: " + cBeamer.kCooldown);
            }
    }

}
