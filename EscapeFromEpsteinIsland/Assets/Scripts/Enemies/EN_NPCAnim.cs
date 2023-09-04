using UnityEngine;

public class EN_NPCAnim : MonoBehaviour
{
    public Sprite                       rShambling;
    public Sprite                       rHitstun;

    EN_NPC                              cNPC;
    
    public void FAnimate()
    {
        cNPC = GetComponent<EN_NPC>();
        MAN_Helper h = cNPC.rOverseer.GetComponent<MAN_Helper>();
        // MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cNPC.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cNPC.mState){
            case EN_NPC.State.SHAMBLING: sRender.sprite = rShambling; break;
            case EN_NPC.State.HITSTUNNED: sRender.sprite = rHitstun; break;
            default: Debug.Log("state: " + cNPC.mState + " not covered"); break;
        }
    }
}
