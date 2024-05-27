using UnityEngine;

public class EN_BPBerthaAnim : MonoBehaviour
{
    public enum STATE{FOLLOWING_PLAYER, PRE_EXPLOSION, STUNNED}
    public Sprite                       rFollowing;
    public Sprite                       rPreExplosion;
    public Sprite                       rStunned;
    public Sprite                       rIdle;

    EN_BPBertha                         cBertha;
    void Start()
    {
        cBertha = GetComponent<EN_BPBertha>();
    }

    public void FAnimate()
    {
        if(cBertha == null) return;
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        if(cBertha.kState == cBertha.kFollowingPlayer){
            sRender.sprite = rFollowing;
        }else if(cBertha.kState == cBertha.kPreExplosion){
            sRender.sprite = rPreExplosion;
        }else if(cBertha.kState == cBertha.kPoiseBroke){
            sRender.sprite = rStunned;
        }else{
            Debug.Log("Bertha state not covered");
        }
    }
}
