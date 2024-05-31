using UnityEngine;
using System.Collections.Generic;

public class SPL_StarDavid4 : MonoBehaviour
{
    public List<WP_DavidOrb>                rEmitters;

    public float                            mCreatedTmStmp;
    public float                            _emitterRotationSpeed = 30f;
    public float                            _emitterFireRate = 0.2f;
    public float                            mFireTmStmp;
    public PJ_EN_Plasmoid                   PF_Bullet;

    void Start()
    {
        mCreatedTmStmp = Time.time;
    }

    void Update()
    {
        if(Time.time - mFireTmStmp > _emitterFireRate){
            for(int i=0; i<rEmitters.Count; i++){
                PJ_EN_Plasmoid p = Instantiate(PF_Bullet, rEmitters[i].transform.position, rEmitters[i].transform.rotation);
                p.FShootAt(rEmitters[i].transform.up, gameObject);
            }
            mFireTmStmp = Time.time;
        }

        for(int i=0; i<rEmitters.Count; i++){
            float turnThisFrame = _emitterRotationSpeed * Time.deltaTime;
            rEmitters[i].transform.Rotate(0f,0f,turnThisFrame);


            // float percentCharged = (Time.time - mCreatedTmStmp) / _startFiringDelay;
            // float turnThisFrame = Time.deltaTime * _rotationRateDegreesPerSecond * percentCharged;
            // transform.Rotate(0f,0f,turnThisFrame);
        }
    }


}
