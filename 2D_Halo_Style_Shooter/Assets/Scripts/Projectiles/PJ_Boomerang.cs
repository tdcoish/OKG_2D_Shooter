using UnityEngine;

public class PJ_Boomerang : MonoBehaviour
{
    [HideInInspector]
    public float                    _timeToApexGiven;
    float                           mCreatedTmStmp;
    [HideInInspector]
    public float                    _spdGiven;
    bool                            mReachedApex = false;

    public float                    _anglesPerSecond;

    public bool                     _canSpin = true;

    public void FThrowBoomerang(float givenTimeToApex, float givenSpeed, Vector2 normalizedDir)
    {
        GetComponent<Rigidbody2D>().velocity = normalizedDir * givenSpeed;
        _timeToApexGiven = givenTimeToApex;
        _spdGiven = givenSpeed;

        mCreatedTmStmp = Time.time;
    }

    void Update()
    {
        if(_canSpin){
            float angleChangeThisFrame = _anglesPerSecond * Time.deltaTime;
            transform.RotateAround(transform.position, Vector3.forward, angleChangeThisFrame);
        }


        if(!mReachedApex){
            if(Time.time - mCreatedTmStmp > _timeToApexGiven){
                Debug.Log("Boomerang reached apex, going back now");
                Rigidbody2D cRigid = GetComponent<Rigidbody2D>();
                cRigid.velocity = cRigid.velocity * -1f;
                mReachedApex = true;
            }
        }

        if(Time.time - mCreatedTmStmp > _timeToApexGiven*2f){
            Debug.Log("Winking boomerang out of existence");
            Destroy(gameObject);
        }

    }
}
