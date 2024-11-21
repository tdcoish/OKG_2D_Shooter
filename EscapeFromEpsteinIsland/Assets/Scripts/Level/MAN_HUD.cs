/************************************
More than the HUD, also mouse trails.
************************************/

using UnityEngine;

public class MAN_HUD : MonoBehaviour
{
    public UI_HUD                   rHUD;           // background info, not weapon select stuff.
    public GameObject               UI_ActiveTarget;
    public UI_StuckHUD              rStuckHUD;

    public MS_Icon                  PF_MouseIcon;
    public MS_Trail                 PF_MouseTrail;
    public TH_Icon                  PF_TrueHeadingIcon;
    public TH_Trail                 PF_TrueHeadingTrail;
    // Want to make the number of trailing icons different. 
    public int                      _mouseTrailNumbers = 5;
    public float                    _minTrailSpacing = 0.25f;

    public void FRUN_Start()
    {
        rHUD = FindObjectOfType<UI_HUD>();
        if(rHUD == null){
            Debug.Log("No HUD found");
        }
        rStuckHUD = FindObjectOfType<UI_StuckHUD>();
        if(rStuckHUD == null){
            Debug.Log("No stuck HUD found");
        }
    }

    public void F_Update(PC_Cont rPC)
    {
        if(rHUD != null){
            if(rPC != null){
                rHUD.FillPCHealthAndShields(rPC.cHpShlds.mHealth.mAmt, rPC.cHpShlds.mHealth._max, rPC.cHpShlds.mShields.mStrength, rPC.cHpShlds.mShields._max);
                rHUD.FillWeaponOverheatAmounts(rPC);
                rHUD.FillBlanks(rPC._numBlanks);
                rHUD.ShowArmourIfActive(rPC.mArmourActive);
            }
        }
        if(rStuckHUD != null){
            if(rPC != null){
                rStuckHUD.transform.position = rPC.transform.position;
                rStuckHUD.FillBarsAndSetArmourDecal(rPC);
            }
        }
    }

    // Don't draw the mouse when we're switching targets.
    public void FDrawMouseIconAndTrailAndActiveTarget(Camera cam, PC_Cont rPC)
    {
        MS_Icon[] icons = FindObjectsOfType<MS_Icon>();
        for(int i=0; i<icons.Length; i++){
            Destroy(icons[i].gameObject);
        }
        MS_Trail[] trails = FindObjectsOfType<MS_Trail>();
        for(int i=0; i<trails.Length; i++){
            Destroy(trails[i].gameObject);
        }
        TH_Icon[] th_icons = FindObjectsOfType<TH_Icon>();
        for(int i=0; i<th_icons.Length; i++){
            Destroy(th_icons[i].gameObject);
        }
        TH_Trail[] th_trails = FindObjectsOfType<TH_Trail>();
        for(int i=0; i<th_trails.Length; i++){
            Destroy(th_trails[i].gameObject);
        }

        if(rPC == null) return;

        Vector2 msPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Instantiate(PF_MouseIcon, msPos, transform.rotation);
        Vector2 vDir = (msPos - (Vector2)rPC.transform.position).normalized;
        Vector2 trailPos = rPC.transform.position;
        float dis = Vector2.Distance(msPos, rPC.transform.position);
        float spacing = dis / _mouseTrailNumbers;
        for(int i=0; i<_mouseTrailNumbers; i++){
            trailPos = (Vector2)rPC.transform.position + (spacing * i * vDir);
            Instantiate(PF_MouseTrail, trailPos, transform.rotation);
        }

        // Now draw the true heading spot.
        PC_Heading h = rPC.GetComponent<PC_Heading>();
        Instantiate(PF_TrueHeadingIcon, h.mCurHeadingSpot, transform.rotation);
        // Draw those trails.
        vDir = (h.mCurHeadingSpot - (Vector2)rPC.transform.position).normalized;
        Vector2 th_trailPos = rPC.transform.position;
        dis = Vector2.Distance(h.mCurHeadingSpot, rPC.transform.position);
        spacing = dis / _mouseTrailNumbers;
        for(int i=0; i<_mouseTrailNumbers; i++){
            th_trailPos = (Vector2)rPC.transform.position + (spacing * i * vDir);
            Instantiate(PF_MouseTrail, th_trailPos, transform.rotation);
        }

        if(!rPC.mHasActiveTarget){
            UI_ActiveTarget.gameObject.SetActive(false);           
        }else{
            UI_ActiveTarget.gameObject.SetActive(true);
            UI_ActiveTarget.transform.position = rPC.rCurTarget.transform.position;
        }

    }

}
