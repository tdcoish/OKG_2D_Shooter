/********************************************************************************************
Can't believe I couldn't come up with a better name than "Stuck HUD". 

This is the UI, but on the player, so that way the player can see while looking at themselves whether
or not they can do things like take a melee swipe, or fire their weapon.
Would really need a decal that glows when I can't melee anymore. Also, would need a golden highlight
thing that shows whether we're using guns or melee, at least 
***************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_StuckHUD : MonoBehaviour
{
    public Image                        cPCStaminaFill;
    public Image                        cPCPRifleFill;
    public Image[]                      cPCGrenadesFill;

    public Sprite                       rMeleeAvailable;
    public Sprite                       rMeleeUnavailable;
    public Image                        IMG_MeleeDecal;

    public Image                        IMG_ArmourDecal;

    public void FillBarsAndSetArmourDecal(PC_Cont rPC)
    {
        // // Stamina needs to know if you can melee.
        // cPCStaminaFill.fillAmount = (rPC._staminaMax-(float)rPC.mCurStamina)/rPC._staminaMax;
        // if(rPC.mCurStamina < rPC._staminaDrainSlash){
        //     IMG_MeleeDecal.sprite = rMeleeUnavailable;
        // }else{
        //     IMG_MeleeDecal.sprite = rMeleeAvailable;
        // }
        
        // PC_Guns guns = rPC.GetComponent<PC_Guns>();
        // cPCPRifleFill.fillAmount = guns.mPRifle.mCurHeating / guns.mPRifle._maxHeating;
        // if(guns.mPRifle.mOverheated){
        //     cPCPRifleFill.color = new Color(1f, 0f, 0f, 1f);
        // }else{
        //     cPCPRifleFill.color = new Color(0f, 1f, 1f, 1f);
        // }

        // for(int i=0; i<rPC.cGrenader._maxCharges; i++){
        //     cPCGrenadesFill[i].fillAmount = (Time.time - rPC.cGrenader.mThrowTmStmps[i]) / rPC.cGrenader._cooldownRate;
        // }

        if(rPC.mArmourActive){
            IMG_ArmourDecal.color = new Color(1f, 1f, 1f, 1f);
        }else{
            IMG_ArmourDecal.color = new Color(1f, 1f, 1f, 0f);
        }

    }

}
