using UnityEngine;

public class PC_RotOrbController : MonoBehaviour
{
    public WP_RotatingOrb               rOrb;
    public WP_RotatingOrb               PF_RotatingOrb;
    public enum STATE{HOT, COLD}
    public STATE                        mState;
    public float                        _rotationsPerSecond = 0.5f;
    public float                        _disFromPlayer = 3f;
    public float                        _cooldownTime = 1f;
    public float                        mHitTmStmp;

    void Start()
    {
        rOrb = Instantiate(PF_RotatingOrb, transform.position, transform.rotation);
        rOrb.rPC = this;
        mState = STATE.HOT;
    }

    void Update()
    {
        float modifiedTime = Time.time / _rotationsPerSecond;
        // However, bias over time.
        float xOffset = Mathf.Cos(modifiedTime) * _disFromPlayer;
        float yOffset = Mathf.Sin(modifiedTime) * _disFromPlayer;
        Vector2 newPos = transform.position;
        newPos.x += xOffset; newPos.y += yOffset;
        rOrb.transform.position = newPos;

        // Also have to keep track of whether it should be active or not.
        if(mState == STATE.HOT){
            rOrb.sRender.sprite = rOrb.rHot;
        }else{
            rOrb.sRender.sprite = rOrb.rCold;
            if(Time.time - mHitTmStmp > _cooldownTime){
                mState = STATE.HOT;
            }
        }
    }

    public void FRegisterHitEnemy()
    {
        mState = STATE.COLD;
        mHitTmStmp = Time.time;
    }
}
