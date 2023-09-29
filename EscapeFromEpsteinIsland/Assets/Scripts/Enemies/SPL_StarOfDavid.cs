/*************************************************************************************
Spawns with an image of a six pointed star, with the points being orbs. After a while 
each orb shoots towards the center of the star. 
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class SPL_StarOfDavid : MonoBehaviour
{
    public enum STATE{CHARGING, FIRED}
    public STATE                            mState = STATE.CHARGING;

    public float                            mCreatedTmStmp;
    public float                            _shootDelay = 1f;
    public float                            mFiredTmStmp;
    public float                            _fadeawayTime = 1f;

    public SpriteRenderer                   rSprite;

    public List<WP_DavidOrb>                rOrbs;
    public PJ_DavidOrb                      PF_OrbProjectile;

    void Start()
    {
        mCreatedTmStmp = Time.time;
    }

    public void Update()
    {
        switch(mState)
        {
            case STATE.CHARGING: RUN_Charging(); break;
            case STATE.FIRED: RUN_Fired(); break;
        }
    }

    public void RUN_Charging()
    {
        float percentCharged = (Time.time - mCreatedTmStmp) / _shootDelay;
        for(int i=0; i<rOrbs.Count; i++){
            SpriteRenderer sr = rOrbs[i].GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 1f, 1f, percentCharged);
        }

        if(Time.time - mCreatedTmStmp > _shootDelay){
            // Fire
            for(int i=0; i<rOrbs.Count; i++){
                // Fire their projectile towards the center.
                PJ_DavidOrb d = Instantiate(PF_OrbProjectile, rOrbs[i].transform.position, transform.rotation);
                // Now make them go towards the center.
                d.FFireOrb((transform.position - d.transform.position).normalized);
            }

            mFiredTmStmp = Time.time;
            mState = STATE.FIRED;
        }
    }

    public void RUN_Fired()
    {
        // Not sure. I guess we just fade away?
        float percentFaded = (Time.time - mFiredTmStmp) / _fadeawayTime;
        rSprite.color = new Color(1f, 1f, 1f, (1f-percentFaded));

        for(int i=0; i<rOrbs.Count; i++){
            SpriteRenderer sr = rOrbs[i].GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0f, 0f, (1f-percentFaded));
        }

        if(percentFaded >= 1f){
            Destroy(gameObject);
        }
    }
}
