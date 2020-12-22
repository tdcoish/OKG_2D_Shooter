/*************************************************************************************
Unlike Halo 2 Sniper Jackals, these guys need to charge up their attacks, and also be stunable
if you see them charging up their attacks.

This isn't super functional without a piece of cover that the player can temporarily hide behind.
It is also not super functional without a clear tell that the sniper is charging up their shot.

I think the graphics should be a laser line that goes from the sniper to either the player or some 
geometry that's in between, and if there's geometry in between, they don't take the shot.

The line itself needs to go from blue to red, or something like that.
*************************************************************************************/
using UnityEngine;

public class EN_Sniper : MonoBehaviour
{
    public enum STATE{NOT_CHARGING, CHARGING, COOLDOWN}
    public STATE                        mState = STATE.NOT_CHARGING;

    private PC_Cont                     rPC;

    public float                        _chargeTime;
    float                               mChargeStTime;
    public float                        _cooldownTime;
    float                               mCoolStTime;

    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null){
            Debug.Log("No player in scene");
        }
    }

    void Update()
    {
        switch(mState){
            case STATE.NOT_CHARGING: RUN_NotCharging(); break;
            case STATE.CHARGING: RUN_Charging(); break;
            case STATE.COOLDOWN: RUN_Cooldown(); break;
        }
    }

    void RUN_NotCharging(){
        // basically we look for the player, and if we can see him, we start charging.
        if(FCastForPlayer(transform.position, rPC.transform.position)){
            mState = STATE.CHARGING;
            mChargeStTime = Time.time;
            Debug.Log("Saw player, charging");
        }
    }

    void RUN_Charging(){
        // If we can't see the player, then we have to break our charging.
        if(FCastForPlayer(transform.position, rPC.transform.position) == false){
            Debug.Log("Broke contact with player, no longer charging");
            mState = STATE.NOT_CHARGING;
            return;
        }

        // in the meantime, change the colour of the laser.
        
        
        // If we're fully charged, fire.
        if(Time.time - mChargeStTime > _chargeTime){
            // here is where we fire, then change states.
            Debug.Log("Fired");
            mCoolStTime = Time.time;
            mState = STATE.COOLDOWN;
        }
    }
    
    void RUN_Cooldown()
    {
        if(Time.time - mCoolStTime > _cooldownTime){
            Debug.Log("Cooled down");
            mState = STATE.NOT_CHARGING;
        }
    }

    bool FCastForPlayer(Vector2 ourPos, Vector2 playerPos)
    {
        // basically we look for the player, and if we can see him, we start charging.
        Vector2 vDir = rPC.transform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir);

        if(hit.collider != null)
        {
            if(hit.collider.GetComponent<PC_Cont>())
            {
                return true;
            }
        }
        return false;
    }
}
