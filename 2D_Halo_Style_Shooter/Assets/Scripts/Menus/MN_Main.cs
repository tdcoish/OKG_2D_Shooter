/*************************************************************************************

*************************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;

public class MN_Main : MonoBehaviour
{
    public GameObject               SCN_Main;
    public GameObject               SCN_Outro;

    bool                            mQuitting = false;
    public float                    _outroTime = 0.5f;
    float                           mOutroTimeStmp;
    
    void Update()
    {
        if(mQuitting){
            if(Time.time - mOutroTimeStmp > _outroTime){
                Application.Quit();
            }
        }
    }

    public void BTN_HitPlay()
    {
        SceneManager.LoadScene("SN_EnemyTesting");
    }
    public void BTN_HitQuit()
    {
        SCN_Main.SetActive(false);
        SCN_Outro.SetActive(true);
        mOutroTimeStmp = Time.time;
        mQuitting = true;
    }
    
}
