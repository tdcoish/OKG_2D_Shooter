/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EN_GrenadeTurret : MonoBehaviour
{
    public PJ_EN_PGrenade               PF_Grenade;

    public A_HealthShields              cHpShlds;
    public EN_Misc                      cMisc;
    public GunData                      mGunD;

    void Start()
    {
        cHpShlds = GetComponent<A_HealthShields>();
        cMisc = GetComponent<EN_Misc>();
    }

    void Update()
    {
        if(cMisc.rPC == null){
            Debug.Log("No player");
            return;
        }
        if(Time.time - mGunD.mLastFireTmStmp > mGunD._fireInterval){
            Debug.Log("Shoot grenade");
            PJ_EN_PGrenade p = Instantiate(PF_Grenade, transform.position, transform.rotation);
            p.GetComponent<PJ_Base>().FShootAt(cMisc.rPC.transform.position, transform.position, gameObject);
            p.mGrenD.vDest = cMisc.rPC.transform.position;

            mGunD.mLastFireTmStmp = Time.time;

        }
        cMisc.FUpdateUI();
    }
}
