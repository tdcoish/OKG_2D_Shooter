using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PJ_EN_HunterBlast : PJ_Base
{

    // The way this works is that it blows up when it hits anything, or its intended destination. 
    public Vector2              mDestination;
    public EX_HBlast            PF_Explosion;

    public GameObject           PF_Particles;

    public void FFirePlasmoid(Vector3 normalizedDir)
    {
        if(cRigid == null){
            Debug.Log("no rigidbody2d");
        }
        cRigid.velocity = normalizedDir * mProjD._spd;
    }

    void Update()
    {
        if(Vector2.Distance(transform.position, mDestination) < 0.1f){
            Debug.Log("Got to destination point");
            FDeath();
        }
    }

    // if it hits a collider it explodes, or if it hits its destination.
    new void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<ENV_Rock>() || col.GetComponent<ENV_Wall>()){
            Debug.Log("Hit env component.");
            FDeath();
        }

        if(col.GetComponent<PC_Cont>() != null){
            Debug.Log("Hit the player");
            FDeath();
        }
    }

    new public void FDeath()
    {
        if(mProjD.PF_Particles == null){
            Debug.Log(gameObject + " is missing death particles");
        }else{
            Instantiate(mProjD.PF_Particles, transform.position, transform.rotation);
        }

        Instantiate(PF_Explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
