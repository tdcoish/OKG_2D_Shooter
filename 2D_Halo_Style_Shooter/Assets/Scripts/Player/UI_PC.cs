/*************************************************************************************
Health and shields now on the same bar.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_PC : MonoBehaviour
{
    public Image                        cHealthFill;
    public Image                        cShieldsFill;
    public Image                        cStaminaFill;
    public Image                        cManaFill;

    public void FillHealthAndShields(float healthAmt, float _healthMax, float shieldsAmt, float _shieldsMax)
    {
        cHealthFill.fillAmount = healthAmt/_healthMax;
        // cShieldFill.fillAmount = amt/_max;
        cShieldsFill.fillAmount = shieldsAmt/_shieldsMax;
    }

    public void FillStaminaAmount(float amt, float _max)
    {
        cStaminaFill.fillAmount = amt/_max;
    }

    public void FillManaAmount(float amt, float _max)
    {
        cManaFill.fillAmount = amt/_max;
    }
}
