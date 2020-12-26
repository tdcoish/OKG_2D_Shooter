/*************************************************************************************

*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_PC : MonoBehaviour
{
    public Image                        mShieldFill;
    public Image                        mHealthFill;

    void Start()
    {

    }

    void Update()
    {
        FillShieldAmount(0.5f);
    }

    public void FillShieldAmount(float percFill)
    {
        // WTF? WHy does this work every second frame?
        if(mShieldFill == null){
            Debug.Log("No fill");
            Debug.Log(mShieldFill);
            // return;
        }else{
            Debug.Log("Fill");
            mShieldFill.fillAmount = percFill;
        }
        // cShieldFill.fillAmount = Time.time % 1.0f;

        if(mHealthFill == null){
            Debug.Log("Health null as well");
        }else{
            Debug.Log("Health not null");
        }
    }
}
