using UnityEngine;

public class EN_AntifaAnim : MonoBehaviour
{
    public Sprite                       rShambling;
    public Sprite                       rHitstun;
    public Sprite                       rWindup;
    public Sprite                       rThrowRec;

    EN_Antifa                           cAntifa;
    
    public void FAnimate()
    {
        cAntifa = GetComponent<EN_Antifa>();
        if(cAntifa == null) return;
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cAntifa.kState == cAntifa.kPoiseBroke){
            sRender.sprite = rHitstun;
        }else if(cAntifa.kState == cAntifa.kShambling){
            sRender.sprite = rShambling;
        }else if(cAntifa.kState == cAntifa.kWindup){
            sRender.sprite = rWindup;
        }else if(cAntifa.kState == cAntifa.kThrowRec){
            sRender.sprite = rThrowRec;
        }else{
            Debug.Log("State not covered");
        }
    }
}
