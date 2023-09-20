using UnityEngine;
using UnityEngine.UI;

public class MAN_Score : MonoBehaviour
{
    public Text                     TXT_Time;
    public Text                     TXT_Score;
    public int                      mScore;
    public float                    mTimeStartTmStmp;

    public void FRUN_Start()
    {
        mTimeStartTmStmp = Time.time;
    }

    public void FRUN_Update()
    {
        TXT_Time.text = "Time: " + (Time.time - mTimeStartTmStmp).ToString("F2");

        mScore = (int)((Time.time - mTimeStartTmStmp) * 1000f);
        TXT_Score.text = "Score: " + mScore;
    }


}
