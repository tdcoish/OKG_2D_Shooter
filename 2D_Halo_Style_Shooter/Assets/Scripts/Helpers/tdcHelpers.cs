/*************************************************************************************

*************************************************************************************/

public static class tdcHelpers
{
    // Just because it's easy to forget to set the mouse.z to 0f.
    public static UnityEngine.Vector3 FGetVecToMouse(UnityEngine.Vector3 msPos, UnityEngine.Vector3 ourPos)
    {
        msPos.z = 0f;
        return msPos - ourPos;
    }
}
