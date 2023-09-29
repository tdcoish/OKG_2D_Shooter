/*****************************************************************************************************
The plasma bolt, or whatever we end up calling it, needs to decay before reaching the end of the 
screen. This effect should be visual.
*****************************************************************************************************/
using UnityEngine;

public class PJ_PC_Firebolt : MonoBehaviour
{
    public float                _maxDistance = 2f;
    float                       mTimeCreated;
    SpriteRenderer              rSprite;
    public GameObject           PF_DeathParticles;

    public float                _spd = 10f;
    public Rigidbody2D          cRigid;
    void Awake()
    {
        cRigid = GetComponent<Rigidbody2D>();
        rSprite = GetComponent<SpriteRenderer>();
    }

    public void F_FireMe(Vector3 vDir)
    {
        cRigid.velocity = vDir * _spd;
        mTimeCreated = Time.time;
    }
    void Die()
    {
        Instantiate(PF_DeathParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    void Update()
    {
        // For now the idea is that the bolts go from white to yellow to red, then poof out of existence.
        float timeItShouldTake = _maxDistance/_spd;
        float percentDone = (Time.time - mTimeCreated) / timeItShouldTake;
        // Debug.Log("Percent: " + percentDone);
        
        rSprite.color = new Color(1f, 1f-percentDone, 1f-percentDone, 1f);

        if(percentDone >= 1f){
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<ENV_TileRock>()){
            Die();
        }
    }
}
