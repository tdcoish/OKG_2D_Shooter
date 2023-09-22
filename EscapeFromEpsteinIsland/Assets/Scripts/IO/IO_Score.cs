/***************************************************************************************************
Use SW_QB as a good reference. The playwriter/reader files are relevant.
***************************************************************************************************/

using UnityEngine;
using System.IO;

public class IO_Score : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Default path: " + Application.dataPath);

        string path = Application.dataPath+"/Files/Scores/HighScore.txt";
        string fileContents = "Score:0";

        File.WriteAllText(path, fileContents);

        // And to read in the scores...
        StreamReader sReader = new StreamReader(Application.dataPath+"/Files/Scores/HighScore.txt");
        string sLine = sReader.ReadLine();
        int score = -1;
        for(int i=0; i<sLine.Length; i++){
            if(sLine[i] == ':'){
                score = int.Parse(sLine[i+1].ToString());
            }
        }
        Debug.Log("Score was read in as: " + score);
    }
}
