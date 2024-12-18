
/************************************************************************************************************************************************
This is the entire HUD for the game that wraps around the square battlefield. We've got to show the players stamina, health, shields, on the right.
Guns information below that, and maybe items. Then beneath all that, the minimap. 

On the left side of the screen I'm undecided as to what I'll be putting there. Maybe something like the enemy count, ally information, boss facts, 
that kind of thing. It's the place for the miscellaneous facts to go. 

We also need to programattically figure out the size of these things, depending on the screen aspect ratio. That goes for the main camera as well,
which needs to have its viewport figured out programmatically, since -1,1 is different in pixels for width and height.

Medusa's last stand, through Wk_2dgame, has great resources on the Crysis MMB selection screen.
************************************************************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_HUD : MonoBehaviour
{
    public Image                        cPCHealthFill;
    public Image                        cPCShieldsFill;
    public Image                        cPCStaminaFill;
    public Image                        cPCPRifleFill;
    public Image[]                      cPCGrenadesFill;
    public Text                         txt_PRifleAmmo;
    public Text                         txt_health;
    public Text                         txt_shields;
    public Text                         txt_stamina;

    public Image[]                      cBlanks;
    public Sprite                       rBlankActive;
    public Sprite                       rBlankGrey;

    public Image                        cArmour;
    public Sprite                       rArmourActive;
    public Sprite                       rArmourInactive;

    public void FillPCHealthAndShields(float healthAmt, float _healthMax, float shieldsAmt, float _shieldsMax)
    {
        cPCHealthFill.fillAmount = healthAmt/_healthMax;
        // cShieldFill.fillAmount = amt/_max;
        cPCShieldsFill.fillAmount = shieldsAmt/_shieldsMax;
        txt_health.text = ((int)healthAmt).ToString();
        txt_shields.text = ((int)shieldsAmt).ToString();
    }

    public void FillBlanks(int numActive)
    {
        for(int i=0; i<cBlanks.Length; i++){
            if(i < numActive){
                cBlanks[i].sprite = rBlankActive;
            }else{
                cBlanks[i].sprite = rBlankGrey;
            }
        }
    }

    public void ShowArmourIfActive(bool armourActive)
    {
        if(armourActive){
            cArmour.sprite = rArmourActive;
        }else{
            cArmour.sprite = rArmourInactive;
        }
    }

    // public void FillPCStaminaAmount(float amt, float _max)
    // {
    //     // cPCStaminaFill.fillAmount = amt/_max;
    //     cPCStaminaFill.fillAmount = (_max-amt)/_max;
    //     txt_stamina.text = ((int)amt).ToString();
    // }

    public void FillWeaponOverheatAmounts(PC_Cont pc)
    {
        // PC_Guns guns = pc.GetComponent<PC_Guns>();
        // cPCPRifleFill.fillAmount = guns.mPRifle.mCurHeating / guns.mPRifle._maxHeating;
        // if(guns.mPRifle.mOverheated){
        //     cPCPRifleFill.color = new Color(1f, 0f, 0f, 1f);
        // }else{
        //     cPCPRifleFill.color = new Color(0f, 1f, 0f, 1f);
        // }
        // // Also have to update the ammo count.
        // txt_PRifleAmmo.text = guns.mPRifle.mCurAmmo.ToString("000"); 

        for(int i=0; i<pc.cGrenader._maxCharges; i++){
            cPCGrenadesFill[i].fillAmount = (Time.time - pc.cGrenader.mThrowTmStmps[i]) / pc.cGrenader._cooldownRate;
        }
    }
}
