/*************************************************************************************

*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_PC : MonoBehaviour
{
    public Image                        cShieldFill;
    public Image                        cHealthFill;

    public void FillShieldAmount(float percFill)
    {   
        cShieldFill.fillAmount = percFill;
    }
}
