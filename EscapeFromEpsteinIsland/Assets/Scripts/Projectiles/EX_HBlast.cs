using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EX_HBlast : MonoBehaviour
{
    public float                            _damage = 50f;
    public float                            lifespan = 1f;
    void Start()
    {
        Destroy(gameObject, lifespan);
    }

}
