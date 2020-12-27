/*************************************************************************************
Basically we just follow the player constantly.
*************************************************************************************/
using UnityEngine;

public class CAM_Main : MonoBehaviour
{
    public PC_Cont                      rPC;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
    }

    void Update()
    {
        if(rPC != null){
            Vector3 vPos = rPC.transform.position;
            vPos.z = -10f;
            transform.position = vPos;
        }else{
            Debug.Log("No player");
        }
    }
}
