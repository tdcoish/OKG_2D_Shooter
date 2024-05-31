using UnityEngine;

public class SPL_AimedCircling : MonoBehaviour
{
    public PJ_CirclePath                PF_Bullets;

    public float                        _fireRate = 0.2f;
    public float                        mFireTmStmp;

    void Update()
    {
        PC_Cont rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null) return;

        transform.up = (rPC.transform.position - transform.position).normalized;

        if(Time.time - mFireTmStmp > _fireRate){
            PJ_CirclePath p = Instantiate(PF_Bullets, transform.position, transform.rotation);
            p.FShootMe(rPC.transform.position, transform.position, transform.right);
            PJ_CirclePath p2 = Instantiate(PF_Bullets, transform.position, transform.rotation);
            p2.FShootMe(rPC.transform.position, transform.position, transform.right*-1f);

            mFireTmStmp = Time.time;
        }
    }
}
