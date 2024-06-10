using UnityEngine;

public class PJ_EN_TPill : PJ_Base
{
    public float                    _rotPerSec = 2f;

    void Update()
    {
        transform.up = new Vector2(Mathf.Cos(Time.time * _rotPerSec), Mathf.Sin(Time.time * _rotPerSec));
    }
}
