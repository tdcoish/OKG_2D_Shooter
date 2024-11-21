using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class MAN_Score : MonoBehaviour
{
    public Text                     TXT_Time;
    public Text                     TXT_Score;
    public int                      mScore;
    public float                    mTimeStartTmStmp;

    // annoying little hacks
    public int                      mLastSecond;

    public List<int>                mHighScores;

    public void FRUN_Start()
    {
        mHighScores = new List<int>();
        mTimeStartTmStmp = Time.time;

        string path = Application.streamingAssetsPath+"/Scores/HighScore.bin";
        if(!File.Exists(path)){
            Debug.Log("No high scores yet. Creating binary file.");
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Close();
            fs.Close();
        }

        FileStream fStream = new FileStream(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fStream);

        bool hitEOF = false;
        while (!hitEOF){
            if(br.BaseStream.Position == br.BaseStream.Length){
                hitEOF = true;
            }else{
                mHighScores.Add(br.ReadInt32());
            }
        }

        br.Close();
        fStream.Close();
    }

    public bool FCheckIfScoreIsNewHighest(int score)
    {
        for(int i=0; i<mHighScores.Count; i++){
            if(score < mHighScores[i]){
                return false;
            }
        }
        return true;
    }

    public void FSaveScoresToFile()
    {
        string path = Application.streamingAssetsPath+"/Scores/HighScore.bin";

        // Should probably use append, but whatever.
        FileStream fs = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        for(int i=0; i<mHighScores.Count; i++){
            Debug.Log("Score to write: " + mHighScores[i]);
            bw.Write(mHighScores[i]);
        }
        bw.Close();
        fs.Close();
    }

    public void FRUN_Update()
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(Time.time - mTimeStartTmStmp);
        // Looks a bit ugly.
        if(Time.time - mTimeStartTmStmp < 10f){
            // Debug.Log("here");
            TXT_Time.text = "Time: " + t.Seconds.ToString();
            // TXT_Time.text = "Time: " + (Time.time - mTimeStartTmStmp).ToString("F2");
        }else if(Time.time - mTimeStartTmStmp < 60f){
            TXT_Time.text = "Time: " + t.Seconds.ToString();
        }else if(Time.time - mTimeStartTmStmp < 600f){
            TXT_Time.text = "Time: " + t.ToString("m':'ss");
        }else{
            TXT_Time.text = "Time: " + t.ToString("mm':'ss");
        }

        int second = (int)(Time.time - mTimeStartTmStmp);
        if(second > mLastSecond){
            mScore += second - mLastSecond;
            mLastSecond = second;
        }
        TXT_Score.text = "Score: " + mScore;
    }


}
