﻿/*************************************************************************************

*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

public class UI_PC : MonoBehaviour
{
    public Image                        cShieldFill;
    public Image                        cHealthFill;

    // This is annoying.
    public Image[]                      cARifleBullets;
    public Image                        cARBulPlacement;
    public int                          _aRBulletSpacing = 10;          // in pixels.
    public Sprite                       rARBulletFull;
    public Sprite                       rARBulletEmpty;

    public Image                        cReloadHorBar;
    public Image                        cReloadVertBar;


    public void FillShieldAmount(float percFill)
    {           
        cShieldFill.fillAmount = percFill;
    }

    public void FSetARifleUI(int numInClip, int maxClip, PC_Gun.STATE state, float rldTmStmp, float _reloadTime)
    {
        for(int i=0; i<maxClip; i++){
            if(numInClip > i){
                cARifleBullets[i].sprite = rARBulletFull;
            }else{
                Debug.Log("Use empty sprite");
                cARifleBullets[i].sprite = rARBulletEmpty;
            }
            Vector3 vPos = cARBulPlacement.transform.position;
            vPos.x += _aRBulletSpacing * i;
            if(i >= 10){
                vPos.y -= 25;
                vPos.x -= _aRBulletSpacing * 10;
            } 
            cARifleBullets[i].transform.position = vPos;
        }

        // now we also render the reloading stuffs.
        if(state == PC_Gun.STATE.RELOADING){
            cReloadHorBar.gameObject.SetActive(true);
            cReloadVertBar.gameObject.SetActive(true);
        }else{
            cReloadHorBar.gameObject.SetActive(false);
            cReloadVertBar.gameObject.SetActive(false);
        }
    }
}
