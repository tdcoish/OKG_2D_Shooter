using UnityEngine;

/****************************************************
To be inherited
****************************************************/

public class Actor : MonoBehaviour
{
    public Man_Combat               rOverseer;

    public virtual void RUN_Start(){}
    public virtual void RUN_Update(){}
    public virtual void FAcceptHolyWaterDamage(float amt){}
}
