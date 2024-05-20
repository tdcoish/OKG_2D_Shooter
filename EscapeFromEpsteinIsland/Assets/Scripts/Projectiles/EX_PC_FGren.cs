/*************************************************************************************

*************************************************************************************/
using UnityEngine;

public class EX_PC_FGren : MonoBehaviour
{
    public float                            _lifeSpan;

    void Start()
    {
        Destroy(gameObject, _lifeSpan);
    }
}
