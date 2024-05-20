using UnityEngine;

public class WP_Mine : MonoBehaviour
{
    public enum STATE{S_LAYING, S_WAITING, S_TRIGGERED}
    public STATE                mState;

    public SpriteRenderer           sRender;
    public Sprite                   rLaying;
    public Sprite                   rWaiting;
    public Sprite                   rTriggered;
    public SpriteRenderer           sMinimapRenderer;

    public EX_PlayerMine            PF_Explosion;

    public float                    _layingTime;
    public float                    mLayTmStmp;
    public float                    _triggerTime;
    public float                    mTriggeredTmStmp;

    void Start()
    {
        mLayTmStmp = Time.time;
    }

    void Update()
    {
        switch(mState)
        {
            case STATE.S_LAYING: RUN_Laying(); break;
            case STATE.S_WAITING: RUN_Waiting(); break;
            case STATE.S_TRIGGERED: RUN_Triggered(); break;
        }
    }

    public void RUN_Laying()
    {
        sRender.sprite = rLaying;
        sMinimapRenderer.sprite = rLaying;
        if(Time.time - mLayTmStmp > _layingTime){
            mState = STATE.S_WAITING;
        }
    }
    public void RUN_Waiting()
    {
        sRender.sprite = rWaiting;
        sMinimapRenderer.sprite = rWaiting;
    }
    public void RUN_Triggered()
    {
        sRender.sprite = rTriggered;
        sMinimapRenderer.sprite = rTriggered;
        if(Time.time - mTriggeredTmStmp > _triggerTime){
            Instantiate(PF_Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if(mState != STATE.S_WAITING){
            return;
        }

        if(col.GetComponent<Actor>()){
            mState = STATE.S_TRIGGERED;
            mTriggeredTmStmp = Time.time;
        }
    }
}
