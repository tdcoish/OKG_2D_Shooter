/*************************************************************************************
Unlike Halo 2 Sniper Jackals, these guys need to charge up their attacks, and also be stunable
if you see them charging up their attacks.

This isn't super functional without a piece of cover that the player can temporarily hide behind.
It is also not super functional without a clear tell that the sniper is charging up their shot.

I think the graphics should be a laser line that goes from the sniper to either the player or some 
geometry that's in between, and if there's geometry in between, they don't take the shot.

The line itself needs to go from blue to red, or something like that.

If the sniper takes damage, then it is temporarily stopped from charging.
*************************************************************************************/
using UnityEngine;

public class EN_Sniper : MonoBehaviour
{
    public enum STATE{NOT_CHARGING, CHARGING, COOLDOWN, STUNNED}
    public STATE                        mState = STATE.NOT_CHARGING;

    private LineRenderer                cLine;
    private A_HealthShields             cHpShlds;
    public EN_Misc                      cMisc;

    public GameObject                   PF_ShotParticles;

    public float                        _chargeTime;
    float                               mChargeStTime;
    public float                        _cooldownTime;
    float                               mCoolStTime;
    public float                        _stunnedTime;
    float                               mStunStTime;
    public float                        _dam;
    public DAMAGE_TYPE                  _DAM_TYPE;

    void Start()
    {
        cLine = GetComponent<LineRenderer>();
        if(!cLine){
            Debug.Log("No line renderer");
        }

        cHpShlds = GetComponent<A_HealthShields>();
        cMisc = GetComponent<EN_Misc>();
    }

    void Update()
    {
        if(cMisc.rPC == null){
            Debug.Log("Player not here, sniper won't work");
            return;
        }
        switch(mState){
            case STATE.NOT_CHARGING: RUN_NotCharging(); break;
            case STATE.CHARGING: RUN_Charging(); break;
            case STATE.COOLDOWN: RUN_Cooldown(); break;
            case STATE.STUNNED: RUN_Stunned(); break;
        }

        cMisc.FUpdateUI();
    }

    void RUN_NotCharging(){
        // basically we look for the player, and if we can see him, we start charging.
        if(FCastForPlayer(transform.position, cMisc.rPC.transform.position)){
            mState = STATE.CHARGING;
            Debug.Log("Saw player, charging");
            ENTER_Charging();
        }
    }

    void ENTER_Charging(){
        mChargeStTime = Time.time;
        cLine.enabled = true;
        cLine.startWidth = 0f;
        cLine.endWidth = 0f;
    }
    void RUN_Charging(){
        if(cMisc.rPC == null){
            Debug.Log("No player");
            return;
        }
        // If we can't see the player, then we have to break our charging.
        if(FCastForPlayer(transform.position, cMisc.rPC.transform.position) == false){
            Debug.Log("Broke contact with player, no longer charging");
            mState = STATE.NOT_CHARGING;
        }else{
            // in the meantime, change the colour of the laser.
            cLine.startWidth = 0.1f;
            cLine.endWidth = 0.1f;
            cLine.SetPosition(0, transform.position);
            cLine.SetPosition(1, cMisc.rPC.transform.position);
            // also change the colour.
            float percentDone = (Time.time - mChargeStTime)/_chargeTime;
            Color c = Color.Lerp(Color.yellow, Color.red, percentDone);
            cLine.startColor = c; cLine.endColor = c;
            
            // If we're fully charged, fire.
            if(Time.time - mChargeStTime > _chargeTime){
                Vector2 vDir = cMisc.rPC.transform.position - transform.position;
                LayerMask mask = LayerMask.GetMask("ENV_Obj", "PC");

                RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir, 100f, mask);
                Instantiate(PF_ShotParticles, hit.point, transform.rotation);

                cMisc.rPC.GetComponent<A_HealthShields>().FTakeDamage(_dam, _DAM_TYPE);

                Debug.Log("Fired");
                mCoolStTime = Time.time;
                mState = STATE.COOLDOWN;
            }
        }

        if(mState != STATE.CHARGING){
            EXIT_Charging();
        }
    }
    void EXIT_Charging()
    {
        cLine.enabled = false;
    }
    
    void RUN_Cooldown()
    {
        if(Time.time - mCoolStTime > _cooldownTime){
            Debug.Log("Cooled down");
            mState = STATE.NOT_CHARGING;
        }
    }

    void RUN_Stunned()
    {
        if(Time.time - mStunStTime > _stunnedTime){
            Debug.Log("No longer stunned");
            mState = STATE.NOT_CHARGING;
        }
    }

    public void FHandleTookDamage()
    {
        if(mState == STATE.CHARGING){
            EXIT_Charging();
        }
        mState = STATE.STUNNED;
        mStunStTime = Time.time;
        Debug.Log("Took damage, stunned");
    }

    bool FCastForPlayer(Vector2 ourPos, Vector2 playerPos)
    {
        if(cMisc.rPC == null){
            Debug.Log("No player");
            return false;
        }
        // basically we look for the player, and if we can see him, we start charging.
        Vector2 vDir = cMisc.rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("ENV_Obj", "PC");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir, 100f, mask);

        if(hit.collider != null)
        {
            if(hit.collider.GetComponent<PC_Cont>())
            {
                return true;
            }else{
                Debug.Log("Sniper saw something" + hit.collider.gameObject);
            }
        }
        return false;
    }
}
