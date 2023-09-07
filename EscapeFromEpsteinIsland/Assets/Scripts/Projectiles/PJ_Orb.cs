/***************************************************************************************
A shameless copy of Li Ming's orb.
***************************************************************************************/

using UnityEngine;

public class PJ_Orb : MonoBehaviour
{
    public float                _maxDistance = 2f;
    public float                _radialGrowth = 2f;
    float                       mTimeCreated;
    public SpriteRenderer       rSprite;
    public GameObject           PF_DeathParticles;

    public float                _spd = 10f;
    public Rigidbody2D          cRigid;

    public void F_FireMe(Vector3 vDir)
    {
        cRigid.velocity = vDir * _spd;
        mTimeCreated = Time.time;
    }
    void Update()
    {
        // The orb grows larger until it hits peak distance and winks out of existence.
        // It also increases in damage as it goes. 
        // float 

        // For now the idea is that the bolts go from white to yellow to red, then poof out of existence.
        float timeItShouldTake = _maxDistance/_spd;
        float percentDone = (Time.time - mTimeCreated) / timeItShouldTake;
        // Debug.Log("Percent: " + percentDone);
        
        rSprite.color = new Color(1f, 1f-percentDone, 1f-percentDone, 1f);

        if(percentDone >= 1f){
            // death particles
            Instantiate(PF_DeathParticles, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

}
