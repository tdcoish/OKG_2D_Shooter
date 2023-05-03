
/*****************************************************************************************************
My idea with the Beam Rifle is to spawn in some invisible points, which let the beam rifle line renderer
continue to show and move with the player.
*****************************************************************************************************/
using UnityEngine;

public class PJ_PC_BeamRifle : MonoBehaviour
{
    public LineRenderer                     cLineRender;

    public float                            _damRadius = 1f;
    public float                            _spreadRadius;
    public float                            mLifespan;
    public float                            mCreatedTimeStamp;

    // The thing that shot us. 
    public Actor                            rShooter;

    void Update()
    {
        // Make the alpha fade quickly.
        if(rShooter != null){
            cLineRender.enabled = true;
            cLineRender.SetPosition(0, transform.position);
            cLineRender.SetPosition(1, rShooter.transform.position);
            cLineRender.useWorldSpace = true;
            Color col = cLineRender.startColor;
            col.a = 1f - (Time.time - mCreatedTimeStamp) / mLifespan;
            cLineRender.startColor = col; cLineRender.endColor = col;
        }

        if(Time.time - mCreatedTimeStamp > mLifespan){
            Destroy(gameObject);
        }
    }
}
