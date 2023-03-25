/*************************************************************************************

*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_PC : MonoBehaviour
{
    public Image                        cShieldFill;
    public Image                        cHealthFill;

    public void FillShieldAmount(float amt, float _max)
    {           
        cShieldFill.fillAmount = amt/_max;
    }

    public void FillHealthAmount(float amt, float _max)
    {
        cHealthFill.fillAmount = amt/_max;
    }
}
