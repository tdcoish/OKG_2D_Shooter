/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EX_PGrenade : MonoBehaviour
{
    public EX_Gren                          cBase;

    public float                            lifespan = 1f;
    void Start()
    {
        cBase = GetComponent<EX_Gren>();
        Destroy(gameObject, lifespan);
    }

    void Update()
    {
        
    }
}
