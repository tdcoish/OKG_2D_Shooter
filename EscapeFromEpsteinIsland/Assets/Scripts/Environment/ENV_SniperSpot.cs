/**************************************************************************************************
Tag for the sniper spots.
**************************************************************************************************/
using UnityEngine;

public class ENV_SniperSpot : MonoBehaviour
{

    public bool                         mCanSeePlayer;

    public void F_CheckCanSeePlayer(PC_Cont rPC)
    {
        if(rPC == null) return;

        Vector2 vDir = rPC.transform.position - transform.position;
        LayerMask mask = LayerMask.GetMask("PC"); mask |= LayerMask.GetMask("ENV_Obj");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vDir.normalized, mask);
        // If we can see the player, immediately go to charging.
        if(hit.collider != null){
            if(hit.collider.GetComponent<PC_Cont>()){
                mCanSeePlayer = true;
                return;
            }
        }

        mCanSeePlayer = false;
    }

}
