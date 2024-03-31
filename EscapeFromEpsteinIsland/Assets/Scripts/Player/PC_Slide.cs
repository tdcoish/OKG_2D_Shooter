/*************************************************************************************
I have been wanting to put this in the game for a very long time. It's basically the 
ETC powerslide, plus additional damage dealt by balls that spit out at the end. 

Just hold shift, and you can steer the player character purely with the mouse. Brief charge
period, then brief cooldown period at the end.
*************************************************************************************/

using UnityEngine;

public class PC_Slide : MonoBehaviour
{
    public enum STATE{S_NotSliding, S_Charging, S_Sliding, S_Recover}
    public STATE                        mState;
    public float                        _chargeTime = 0.25f;
    public float                        mChargeTmStmp;
    public float                        _staminaDrainOnStart = 20f;
    public float                        _staminaDrainPerSec = 20f;
    public float                        _recoverTime = 0.25f;
    public float                        mRecTmStmp;

    public PC_Cont                      cPC;
    public Rigidbody2D                  cRigid;

    void Start()
    {
        cPC = GetComponent<PC_Cont>();
        cRigid = GetComponent<Rigidbody2D>();
    }

    // MsPos already in world coordinates. 
    public void FStartSlide()
    {
        mState = STATE.S_Charging;
        mChargeTmStmp = Time.time;
    }
    
    public void FRunSlide()
    {
        switch(mState)
        {
            case STATE.S_Charging: RUN_Charging(); break;
            case STATE.S_Sliding: RUN_Sliding(); break;
            case STATE.S_Recover: RUN_Recover(); break;
        }
    }

    public void RUN_Charging()
    {
        if(Time.time - mChargeTmStmp > _chargeTime){
            mState = STATE.S_Sliding;
        }
    }

    public void RUN_Sliding()
    {
        // basically when we run out of energy, or we stop holding down the shift key.
    }

    public void RUN_Recover()
    {
        if(Time.time - mRecTmStmp > _recoverTime){
            mState = STATE.S_NotSliding;
        }
    }

}
