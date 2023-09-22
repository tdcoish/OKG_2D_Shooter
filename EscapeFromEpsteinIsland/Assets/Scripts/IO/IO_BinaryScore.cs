/****************************************************************************************************
Coming back to this after dinner. Looks like it's coming along well.
****************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IO_BinaryScore : MonoBehaviour
{
    public List<int>                    mHighScores;

    public void F_Start()
    {
        mHighScores = new List<int>();
        FCreateScoreFileIfNoneExists();
        ReadTopTenScoresFile();
    }

    public void FCreateScoreFileIfNoneExists()
    {
        string path = Application.dataPath+"/Files/Scores/HighScore.bin";
        if(File.Exists(path)){
            Debug.Log("We've already got a score file.");
            return;
        }
        Debug.Log("Creating score file.");
        FileStream fStream = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fStream);
        bw.Close();
        fStream.Close();
    }

    public void ReadTopTenScoresFile()
    {
        string path = Application.dataPath+"/Files/Scores/HighScore.bin";

        FileStream fStream = new FileStream(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fStream);

        bool hitEndOfFile = false;
        while(!hitEndOfFile)
        {
            if(br.BaseStream.Position == br.BaseStream.Length){
                hitEndOfFile = true;
            }else{
                mHighScores.Add(br.ReadInt32());
            }
        }

        // Now we need to sort them. I'll use insertion sort.
        for(int i=1; i<mHighScores.Count; i++){
            for(int j=i; j>0; j--){
                if(mHighScores[j] > mHighScores[j-1]){
                    int temp = mHighScores[j];
                    mHighScores[j] = mHighScores[j-1];
                    mHighScores[j-1] = temp;
                }
            }
        }

        fStream.Close();
        br.Close();
    }
}
