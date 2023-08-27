/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class MSC_Lifespan : MonoBehaviour
{
    public float                        _lifeSpan = 1f;
    void Start()
    {
        Destroy(gameObject, _lifeSpan);
    }
}
