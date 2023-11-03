/******************************************************************************************************
Refills weapons and health.
******************************************************************************************************/
using UnityEngine;

public class Pk_Powerup : MonoBehaviour
{
    public int                          _healthRestore = 20;
    public int                          _ammoRestore = 100;

    public GameObject                   PF_DeathParticles;

    public void F_Death()
    {
        Instantiate(PF_DeathParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
