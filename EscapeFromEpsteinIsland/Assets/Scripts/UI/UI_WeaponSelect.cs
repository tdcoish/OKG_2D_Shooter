/**************************************************************************************
User pressed middle mouse button. Weapon select comes up. Move the mouse forward, and 
release MMB to make your selection. Eventually have a delay, so you can't instantly switch weapons.
**************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class UI_WeaponSelect : MonoBehaviour
{
    public PC_Cont                      rPC;

    public GameObject                   arrow;
    public float                        _arrowDisFromCenter = 0.1f;
    public Vector2                      msPosStart;
    public int                          mIndActive;
    public List<UI_WepImgBack>          mWeapons;

    // Similar to normal icon, yet only rendered when we're doing this.
    // public GameObject                   icon;

    void Update()
    {
        transform.position = rPC.transform.position;

        // Need the msPos to be clipped somehow. 
        Vector2 relativeMousePos = (Vector2)Input.mousePosition - msPosStart;
        arrow.transform.up = relativeMousePos.normalized;
        arrow.transform.position = transform.position + (Vector3)(relativeMousePos.normalized) * _arrowDisFromCenter;
        // Vector3 iconPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); iconPos.z = 0f;
        // icon.transform.position = iconPos;

        // Now comes the task of actually figuring out what got selected.
        // figure out which selection is closest?
        int indClosest = -1;
        float highestDot = -100000f;
        for(int i=0; i<mWeapons.Count; i++){
            Vector2 vDir = (mWeapons[i].transform.position - transform.position).normalized;
            float dot = Vector2.Dot(vDir, relativeMousePos.normalized);
            if(dot > highestDot){
                highestDot = dot;
                indClosest = i;
            }
        }

        if(indClosest != -1){
            mIndActive = indClosest;
            for(int i=0; i<mWeapons.Count; i++){
                mWeapons[i].F_SetActiveSprite(false);
            }
            mWeapons[indClosest].F_SetActiveSprite(true);

        }else{
            Debug.Log("Weird, no weapons");
        }

    }
}
