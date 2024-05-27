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
        if(cSchlomo.kState == cSchlomo.kSpellcasting){
            sRender.sprite = rSpellcasting;
        }else if(cSchlomo.kState == cSchlomo.kFleeing){
            sRender.sprite = rFlee;
        }else if(cSchlomo.kState == cSchlomo.kPoiseBroke){
            sRender.sprite = rStun;
        }
    }
}
