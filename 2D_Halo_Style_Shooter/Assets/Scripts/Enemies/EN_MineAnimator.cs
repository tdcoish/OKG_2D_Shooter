using UnityEngine;

public class EN_MineAnimator : MonoBehaviour
{
    public Sprite                       rHunting;
    public Sprite                       rPreExplosion;

    EN_MovingMine                       cMine;
    void Start()
    {
        cMine = GetComponent<EN_MovingMine>();
    }

    public void FAnimate()
    {
        MAN_Helper h = FindObjectOfType<MAN_Helper>();
        transform.up = h.PointToLookAtAlongHeading(cMine.mHeading);
        SpriteRenderer sRender = GetComponent<SpriteRenderer>();
        switch(cMine.mState){
            case EN_MovingMine.State.HUNTING: sRender.sprite = rHunting; break;
            case EN_MovingMine.State.PRE_EXPLOSION: sRender.sprite = rPreExplosion; break;
            default: Debug.Log("state: " + cMine.mState + " not covered"); break;
        }
    }
}
