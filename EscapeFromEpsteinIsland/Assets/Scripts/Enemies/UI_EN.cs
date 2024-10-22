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
        FUpdateShieldHealthPoiseBars(hs.mHealth.mAmt, hs.mHealth._max, hs.mShields.mStrength, hs.mShields._max, mCurPoise, _maxPoise, poiseBroken);
    }
    public void FUpdateShieldHealthPoiseBars(float mHealth, float _maxHealth, float mShields=0f, float _maxShields=0f, float mCurPoise = 0f, float _maxPoise = 0f, bool poiseBroken = false)
    {
        IMG_HealthBar.fillAmount = (mHealth / _maxHealth);
        if(mShields > 0f){
            IMG_ShieldBar.enabled = true;
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
