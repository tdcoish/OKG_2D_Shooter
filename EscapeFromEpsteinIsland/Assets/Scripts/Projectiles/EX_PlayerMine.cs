using UnityEngine;

public class EX_PlayerMine : MonoBehaviour
{
    public float                            _lifeSpan;
    void Start()
    {
        Destroy(gameObject, _lifeSpan);
    }
}
