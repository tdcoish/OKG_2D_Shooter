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

    public List<int>                mHighScores;

    public void FRUN_Start()
    {
        mHighScores = new List<int>();
        mTimeStartTmStmp = Time.time;

        string path = Application.dataPath+"/Files/Scores/HighScore.bin";
        if(!File.Exists(path)){
            Debug.Log("No high scores yet. Creating binary file.");
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Close();
            fs.Close();
        }

        FileStream fStream = new FileStream(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fStream);

        for(int i=0; i<10; i++){
            // Debug.Log("Score" + i + " is " + br.ReadInt32());
            if(br.BaseStream.Position == br.BaseStream.Length){
                Debug.Log("Hit the end of the file");
            }else{
                Debug.Log("More file to come");
                mHighScores.Add(br.ReadInt32());
            }
        }
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
        string path = Application.dataPath+"/Files/Scores/HighScore.bin";
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
        TXT_Time.text = "Time: " + (Time.time - mTimeStartTmStmp).ToString("F2");

        mScore = (int)((Time.time - mTimeStartTmStmp) * 1000f);
        TXT_Score.text = "Score: " + mScore;
    }


}
