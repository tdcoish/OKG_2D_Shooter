using UnityEngine;

public class EN_SchlomoSpellcasterAnim : MonoBehaviour
{
    public Sprite               rSpellcasting;
    public Sprite               rFlee;
    public Sprite               rStun;

    EN_SchlomoSpellcaster       cSchlomo;
    void Start()
    {
        cSchlomo = GetComponent<EN_SchlomoSpellcaster>();
    }

    public void FAnimate()
    {   
        if(cSchlomo == null){
            return;
        }
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cSchlomo.mState){
            case EN_SchlomoSpellcaster.STATE.SPELLCASTING: sRender.sprite = rSpellcasting; break;
            case EN_SchlomoSpellcaster.STATE.FLEEING: sRender.sprite = rFlee; break;
            case EN_SchlomoSpellcaster.STATE.STUNNED: sRender.sprite = rStun; break;
            default: Debug.Log("state: " + cSchlomo.mState + " not covered"); break;
        }
    }
}
