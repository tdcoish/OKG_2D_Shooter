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
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cBertha.mState){
            case EN_BPBertha.STATE.FOLLOWING_PLAYER: sRender.sprite = rFollowing; break;
            case EN_BPBertha.STATE.PRE_EXPLOSION: sRender.sprite = rPreExplosion; break;
            case EN_BPBertha.STATE.STUNNED: sRender.sprite = rStunned; break;
            default: Debug.Log("state: " + cBertha.mState + " not covered"); break;
        }
    }
}
