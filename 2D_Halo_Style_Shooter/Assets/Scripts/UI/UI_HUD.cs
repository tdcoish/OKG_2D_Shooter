
/************************************************************************************************************************************************
This is the entire HUD for the game that wraps around the square battlefield. We've got to show the players stamina, health, shields, on the right.
Guns information below that, and maybe items. Then beneath all that, the minimap. 

On the left side of the screen I'm undecided as to what I'll be putting there. Maybe something like the enemy count, ally information, boss facts, 
that kind of thing. It's the place for the miscellaneous facts to go. 

We also need to programattically figure out the size of these things, depending on the screen aspect ratio. That goes for the main camera as well,
which needs to have its viewport figured out programmatically, since -1,1 is different in pixels for width and height.
************************************************************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_HUD : MonoBehaviour
{
    public Image                        cPCHealthFill;
    public Image                        cPCShieldsFill;
    public Image                        cPCStaminaFill;
    public Image                        cPCManaFill;

    public void FillPCHealthAndShields(float healthAmt, float _healthMax, float shieldsAmt, float _shieldsMax)
    {
        cPCHealthFill.fillAmount = healthAmt/_healthMax;
        // cShieldFill.fillAmount = amt/_max;
        cPCShieldsFill.fillAmount = shieldsAmt/_shieldsMax;
    }

    public void FillPCStaminaAmount(float amt, float _max)
    {
        cPCStaminaFill.fillAmount = amt/_max;
    }

    public void FillPCManaAmount(float amt, float _max)
    {
        cPCManaFill.fillAmount = amt/_max;
    }
}
