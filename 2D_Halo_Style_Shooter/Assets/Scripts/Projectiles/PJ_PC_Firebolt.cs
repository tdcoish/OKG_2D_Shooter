using UnityEngine;

public class PJ_PC_Firebolt : MonoBehaviour
{
    public float                _lifespan = 5f;
    public float                _spd = 10f;
    public Rigidbody2D          cRigid;
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _lifespan);
    }
}
