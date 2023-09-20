/*************************************************************************************************
The welcome menu is used to explain to playtesters what is going on with the build, and what I expect
from them. 
*************************************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;

public class MN_Welcome : MonoBehaviour
{

    void Update()
    {
        if(Input.anyKey)
        {
            SceneManager.LoadScene("SN_MN_Main");
        }
    }
}
