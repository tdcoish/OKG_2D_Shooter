using UnityEngine;

public class WP_RotatingOrb : MonoBehaviour
{
    public PC_RotOrbController              rPC;
    public float                            _damage = 100f;
    public SpriteRenderer                   sRender;
    public Sprite                           rHot;
    public Sprite                           rCold;

    public void                             FRegisterHitEnemy()
    {
        rPC.FRegisterHitEnemy();
    }
}
