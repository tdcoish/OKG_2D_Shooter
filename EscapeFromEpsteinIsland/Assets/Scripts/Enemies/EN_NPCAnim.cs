using UnityEngine;

public class EN_NPCAnim : MonoBehaviour
{
    public Sprite                       rShambling;
    public Sprite                       rHitstun;

    EN_NPC                              cNPC;
    
    public void FAnimate()
    {
        cNPC = GetComponent<EN_NPC>();
        if(cNPC == null) return;
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cNPC.kState == cNPC.kStunned){
            sRender.sprite = rHitstun;
        }else if(cNPC.kState == cNPC.kShambling){
            sRender.sprite = rShambling;
        }else{
            Debug.Log("State not covered");
        }
    }
}
