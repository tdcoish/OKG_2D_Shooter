using UnityEngine;

/*********************************************
Moves out from the blast, slowly dwindles in hitpower.
*********************************************/
public class PJ_MineShot : MonoBehaviour
{
    public float                        _startDam = 50f;
    public float                        mCurDam;
    public float                        _spd = 3f;
    public float                        _lifetime = 4f;
    public float                        mLifeTmStmp;
    Rigidbody2D                         cRigid;

    public void FRUN_Start(Vector2 vDir)
    {
        cRigid = GetComponent<Rigidbody2D>();
        cRigid.velocity = vDir.normalized * _spd;
        mLifeTmStmp = Time.time;
    }

    void Update()
    {
        float timeAlive = Time.time - mLifeTmStmp;
        mCurDam = _startDam - (_startDam * (timeAlive/_lifetime));

        if(timeAlive > _lifetime){
            Destroy(gameObject);
        }
    }
}
