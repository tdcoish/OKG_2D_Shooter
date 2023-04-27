using UnityEngine;

/************************************************************************************
As of right now, the players needles intercept the enemy. Then they stun once when hitting 
the enemy, and again when exploding. So they're great in the role of tracking and then
interrupting fast moving strafing enemies. 
************************************************************************************/

public class PJ_PC_Needle : MonoBehaviour
{
    [HideInInspector]
    public float                        mLifespan;
    [HideInInspector]
    public float                        mCreatedTimeStamp;
    [HideInInspector]
    public Actor                        mTarget;
    public Rigidbody2D                  cRigid;

    void Update()
    {
        if(mTarget == null){
            return;
        }

        if(Time.time - mCreatedTimeStamp > mLifespan){
            Destroy(gameObject);
        }
        
    }

}
