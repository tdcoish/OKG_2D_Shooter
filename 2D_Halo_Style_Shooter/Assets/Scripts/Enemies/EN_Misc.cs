/*************************************************************************************
Just kind of a collection of random bullshit that all enemies thus far have. Mostly data,
but occassionaly a method.
*************************************************************************************/
using UnityEngine;

[System.Serializable]
public enum ENEMY_TYPE{E_ELITE, E_PTURRET, E_GTURRET, E_BRUTE, E_SNIPER, E_HUNTER}

[RequireComponent(typeof(Rigidbody2D))]
public class EN_Misc : MonoBehaviour
{
    // MAKE SURE YOU CHECK FOR NULL!!!!!!
    public PC_Cont                          rPC;
    public GameObject                       PF_Particles;
    public UI_EN                            gUI;
    public A_HealthShields                  cHpShlds;
    public ENEMY_TYPE                       _TYPE;

    void Awake()
    {
        cHpShlds = GetComponent<A_HealthShields>();
        if(cHpShlds == null){
            Debug.Log("no health shields");
        }

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
