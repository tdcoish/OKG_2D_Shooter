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

    public void FUpdateShieldHealthBars(float mHealth, float _maxHealth, float mShields=0f, float _maxShields=0f, bool usesShieldsEver = false)
    {
        IMG_HealthBar.fillAmount = (mHealth / _maxHealth);
        if(usesShieldsEver){
           IMG_ShieldBar.fillAmount = (mShields / _maxShields);
        }else{
            IMG_ShieldBar.enabled = false;
        }
    }
}
