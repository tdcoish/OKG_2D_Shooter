/*************************************************************************************
Just kind of a collection of random bullshit that all enemies thus far have. Mostly data,
but occassionaly a method.
*************************************************************************************/
using UnityEngine;

public class EN_Misc : MonoBehaviour
{
    // MAKE SURE YOU CHECK FOR NULL!!!!!!
    public PC_Cont                          rPC;
    public GameObject                       PF_Particles;
    public UI_EN                            gUI;
    public A_HealthShields                  cHpShlds;

    void Start()
    {
        cHpShlds = GetComponent<A_HealthShields>();

        rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null){
            Debug.Log("No player character found");
        }

        gUI = GetComponentInChildren<UI_EN>();
        if(gUI == null){
            Debug.Log("No enemy UI found");
        }
    }

    public void FUpdateUI()
    {
        gUI.FUpdateShieldHealthBars(cHpShlds.mHealth.mAmt, cHpShlds.mHealth._max, cHpShlds.mShields.mStrength, cHpShlds.mShields._max, cHpShlds._hasShieldsEver);
    }
}
