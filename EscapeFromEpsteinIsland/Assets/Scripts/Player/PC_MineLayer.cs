using UnityEngine;

public class PC_MineLayer : MonoBehaviour
{
    public WP_Mine                  PF_Mine;
    public float                    _layingRate = 5f;
    public float                    mLayTmStmp;

    void Update()
    {
        if(Time.time - mLayTmStmp > _layingRate){
            Instantiate(PF_Mine, transform.position, transform.rotation);
            mLayTmStmp = Time.time;
        }
    }
}
