using UnityEngine;

public class EN_TroonBloodDrop : MonoBehaviour
{
    public bool                     mInactiveAndFadingOut = false;
    public float                    _lifespan = 10f;
    public float                    mCreatedTmStmp;
    public float                    _fadeoutTime = 0.5f;
    public float                    mFadeoutTmStmp;
    public float                    _damagePerSecond = 20f;
    public float                    _damageRadius = 1f;
    public float                    _damageTickRate = 0.1f;
    public float                    mLastDamTickTmStmp;

    public Man_Combat               rOverseer;

    public SpriteRenderer           mRenderer;

    public void FRunStart(Man_Combat overseer)
    {
        rOverseer = overseer;
        mLastDamTickTmStmp = Time.time;
        mCreatedTmStmp = Time.time;
    }

    public void Update()
    {
        if(rOverseer == null) return;
        if(rOverseer.rPC == null) return;

        if(!mInactiveAndFadingOut){
            if(Time.time - mLastDamTickTmStmp > _damageTickRate){
                if(Vector3.Distance(rOverseer.rPC.transform.position, transform.position) < _damageRadius){
                    mLastDamTickTmStmp = Time.time;
                    rOverseer.rPC.F_ReceiveTroonBloodSpotDamage(_damagePerSecond * _damageTickRate);
                }
            }
            // Do I want it to fade out? Yes.
            if(Time.time - mCreatedTmStmp > _lifespan){
                mInactiveAndFadingOut = true;
                mFadeoutTmStmp = Time.time;
            }
        }else{
            // Here we figure out the percent of fading that they undergo.
            float percentFadedOut = (Time.time - mFadeoutTmStmp) / _fadeoutTime;
            mRenderer.color = new Color(1f, 1f, 1f, (1f-percentFadedOut));
            if(Time.time - mFadeoutTmStmp > _fadeoutTime){
                Destroy(gameObject);
            }
        }
    }
}
