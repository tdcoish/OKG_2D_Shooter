using UnityEngine;
using UnityEngine.SceneManagement;

public class MN_Arcade : MonoBehaviour
{
    public MN_Main                  cMain;

    public void BTN_HitPlayArcade()
    {
        cMain.SO_PlayDetails.mMode = SO_PlayDetails.MODE.ARCADE;
        // WritePlayDetailsToFile(true);
        SceneManager.LoadScene("SN_EnemyTesting");
    }
}
