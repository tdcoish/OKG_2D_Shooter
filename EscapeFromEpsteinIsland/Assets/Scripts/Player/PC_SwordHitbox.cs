using UnityEngine;

public class PC_SwordHitbox : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Sword hit something");
    }
}
