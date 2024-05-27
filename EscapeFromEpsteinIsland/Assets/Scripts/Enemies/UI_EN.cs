/*************************************************************************************
So like the healthbars over their heads.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_EN : MonoBehaviour
{
    public bool                         _hasShieldEver;
    // Not every enemy has a shield, ignore if that's true.
    public Image                        IMG_ShieldBar;
    public Image                        IMG_HealthBar;
    public Image                        IMG_PoiseBar;

    public void FUpdateShieldHealthPoiseBars(A_HealthShields hs, float mCurPoise, float _maxPoise, bool poiseBroken = false)
    {
        FUpdateShieldHealthPoiseBars(hs.mHealth.mAmt, hs.mHealth._max, hs.mShields.mStrength, hs.mShields._max, hs._hasShieldsEver, mCurPoise, _maxPoise, poiseBroken);

        // IMG_HealthBar.fillAmount = (healthShields.mHealth.mAmt / healthShields.mHealth._max);
        // if(healthShields._hasShieldsEver){
        //    IMG_ShieldBar.fillAmount = (healthShields.mShields.mStrength / healthShields.mShields._max);
        // }else{
        //     IMG_ShieldBar.enabled = false;
        // }

    }
    public void FUpdateShieldHealthPoiseBars(float mHealth, float _maxHealth, float mShields=0f, float _maxShields=0f, bool usesShieldsEver = false, float mCurPoise = 0f, float _maxPoise = 0f, bool poiseBroken = false)
    {
        IMG_HealthBar.fillAmount = (mHealth / _maxHealth);
        if(usesShieldsEver){
           IMG_ShieldBar.fillAmount = (mShields / _maxShields);
        }else{
            IMG_ShieldBar.enabled = false;
        }

        if(_maxPoise <= 0f){
            IMG_PoiseBar.enabled = false;
        }else{
            IMG_PoiseBar.fillAmount = mCurPoise / _maxPoise;
            if(poiseBroken){
                IMG_PoiseBar.color = new Color(1f, 1f, 1f, 0.4f);
            }else{
                IMG_PoiseBar.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}
