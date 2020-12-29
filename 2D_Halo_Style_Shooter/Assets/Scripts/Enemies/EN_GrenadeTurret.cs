/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EN_GrenadeTurret : MonoBehaviour
{
    public PJ_EN_PGrenade               PF_Grenade;

    private PC_Cont                     rPC;

    public float                        _shotInterval = 4f;
    private float                       mShotTmStmp;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
    }

    void Update()
    {
        if(Time.time - mShotTmStmp > _shotInterval){
            Debug.Log("Shoot grenade");
            PJ_EN_PGrenade p = Instantiate(PF_Grenade, transform.position, transform.rotation);
            p.mGrenD.vDest = rPC.transform.position;

            mShotTmStmp = Time.time;
        }
    }
}
