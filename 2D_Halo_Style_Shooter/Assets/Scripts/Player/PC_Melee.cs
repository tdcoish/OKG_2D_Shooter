/*************************************************************************************
Just flat out detect if an enemy is in range of our melee, and melee them.

Now, instead of melee over time, we just deal damage to enemies once. However, this takes 
a little bit of time, so you can't spam melee, and can't do any other attacks in that time.
*************************************************************************************/
using UnityEngine;

public class PC_Melee : MonoBehaviour
{
    public enum STATE{S_NOT_Meleeing, S_Closing, S_Meleeing}
    public STATE                        mState;
    public float                        _recoverTime;
    public float                        mTmStmp;
    public float                        _dis;
    public float                        _dam;
    public float                        _spdClose;          // the sucked in to enemy speed.
    public Vector3                      vTarget;

    private Rigidbody2D                 cRigid;
    void Start()
    {
        cRigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch(mState)
        {
            case STATE.S_NOT_Meleeing: RUN_NotMeleeing(); break;
            case STATE.S_Closing: RUN_Closing(); break;
            case STATE.S_Meleeing: RUN_Meleeing(); break;
        }
    }

    void RUN_NotMeleeing()
    {
        if(Input.GetKey(KeyCode.Q))
        {
            Debug.Log("Started melee process");
            bool foundEnemyToHit = false;
            mTmStmp = Time.time;

            EN_HandleHits[] enemies = FindObjectsOfType<EN_HandleHits>();
            for(int i=0; i<enemies.Length; i++)
            {
                if(Vector3.Distance(enemies[i].transform.position, transform.position) < _dis){
                    // also have to be in front of the player.
                    Vector3 vDir = (enemies[i].transform.position - transform.position).normalized;
                    Vector3 vPDir = transform.up;
                    if(Vector3.Dot(vDir, vPDir) >= 0.7f){
                        foundEnemyToHit = true;
                        vTarget = enemies[i].transform.position;
                        enemies[i].FHandleMeleeHit(_dam, DAMAGE_TYPE.MELEE);
                        break;
                    }
                }
            }
            if(foundEnemyToHit){
                Debug.Log("Closing");
                mState = STATE.S_Closing;
            }else{
                Debug.Log("Meleeing");
                mState = STATE.S_Meleeing;
            }
        }
    }
    void RUN_Closing()
    {
        Vector3 vDirToTarget = (vTarget - transform.position).normalized;
        cRigid.velocity = vDirToTarget.normalized * _spdClose;
        if(Vector3.Distance(transform.position, vTarget) < 1.5f)
        {
            Debug.Log("Arrived at target, can hit now");
            mState = STATE.S_Meleeing;
        }
    }
    void RUN_Meleeing()
    {
        if(Time.time - mTmStmp > _recoverTime)
        {
            Debug.Log("Finished meleeing");
            mState = STATE.S_NOT_Meleeing;
        }
    }
}
