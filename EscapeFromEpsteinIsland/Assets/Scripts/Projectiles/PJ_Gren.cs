/*************************************************************************************
At this point just a tag.
*************************************************************************************/
using UnityEngine;

public enum GRENADE_TYPE{PLASMA, FRAG}

public class PJ_Gren : MonoBehaviour
{
    public GRENADE_TYPE             _TYPE;

    public void FHandleHitObj()
    {
        if(_TYPE == GRENADE_TYPE.PLASMA)
        {
            PJ_EN_PGrenade p = GetComponent<PJ_EN_PGrenade>();
            if(p == null){
                Debug.Log("Grenade is null, maybe wrong type?");
                return;
            }
            p.FEnter_Landed();
        }else if(_TYPE == GRENADE_TYPE.FRAG)
        {
            // bounce around a little, but maybe just actually stop or something.
            PJ_PC_FGren p = GetComponent<PJ_PC_FGren>();
            if(p == null){
                Debug.Log("Grenade null, maybe wrong type?");
                return;
            }
            p.FEnter_Landed();
        }
    }
}
