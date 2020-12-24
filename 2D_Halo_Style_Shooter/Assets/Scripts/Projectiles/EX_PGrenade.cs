/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EX_PGrenade : MonoBehaviour
{
    public float                            lifespan = 1f;
    void Start()
    {
        Destroy(gameObject, lifespan);
    }

    void Update()
    {
        
    }
}
