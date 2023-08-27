using UnityEngine;

/************************************************************************************
As of right now, the players needles intercept the enemy. Then they stun once when hitting 
the enemy, and again when exploding. So they're great in the role of tracking and then
interrupting fast moving strafing enemies. 
************************************************************************************/

public class PJ_PC_Needle : MonoBehaviour
{
    [HideInInspector]
    public float                        _lifespan;
    [HideInInspector]
    public float                        _spd;
    [HideInInspector]
    public float                        _turnRate;
    [HideInInspector]
    public float                        _timeBeforeDetonation;
    [HideInInspector]
    public float                        mCreatedTimeStamp;
    [HideInInspector]
    public Actor                        rTarget;
    public Rigidbody2D                  cRigid;

    void Update()
    {
        if(rTarget == null){
            return;
        }

        if(Time.time - mCreatedTimeStamp > _lifespan){
            Destroy(gameObject);
        }

        // Crude homing in on target.
        if(rTarget != null){
            // transform.rotation = Quaternion.LookRotation(cRigid.velocity.normalized);
            Vector2 vDif = rTarget.transform.position - transform.position;

            float angleDif = Vector3.Angle(cRigid.velocity.normalized, vDif.normalized);
            Vector2 vNewHeading = Vector3.RotateTowards(cRigid.velocity.normalized, vDif.normalized, (Mathf.Deg2Rad* _turnRate)*Time.deltaTime, 0f); 
            cRigid.velocity = vNewHeading.normalized * _spd;

            // cRigid.velocity = vDif * _spd;
            transform.up = vNewHeading.normalized;
        }
        
    }

}
