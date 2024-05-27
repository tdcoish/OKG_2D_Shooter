/**************
Designed to hem the player inside the box until the spell wears off. Currently needs SPL_StarDavid2 for
main functions.
**************/
using UnityEngine;

public class SPL_StarDavid3 : MonoBehaviour
{
    public float                        _maxScale;
    public float                        _minScale;
    public float                        _lifeSpan = 5f;
    public float                        mCreatedTmStmp;

    void Start()
    {
        mCreatedTmStmp = Time.time;
    }

    void Update()
    {
        // Linearly interpolate between max and min scale.
        float percentDone = (Time.time - mCreatedTmStmp) / _lifeSpan;
        if(percentDone >= 1f){
            SPL_StarDavid2 s = GetComponent<SPL_StarDavid2>();
            s.mState = SPL_StarDavid2.STATE.DONE;
            return;
        }
        
        float size = _maxScale * (1f-percentDone) + _minScale * percentDone;
        transform.localScale = new Vector3(size, size, size);
    }
}
