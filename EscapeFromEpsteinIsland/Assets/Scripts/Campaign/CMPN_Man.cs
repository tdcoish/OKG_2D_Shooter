/*******************************************************************************************************
What is the campaign? Theoretically, all we need are scenarios, strung together through cutscenes.
The logic for running such a system shouldn't be too hard.

First, I need to create a bunch of cutscenes, and scenarios. Then, I need to link them together. Only
after that is done do we need to program anything. 

Right now all the cutscenes are just text.

https://forum.unity.com/threads/drag-and-drop-streaming-asset-to-inspector-to-get-file-path.499055/
Link to potential solution allowing me to drag binary files into fields.
*******************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class CMPN_Man : MonoBehaviour
{
    public SO_PlayDetails               SO_PlayDetails;
    public string                       CTSN_Intro;
    public string                       NextScenarioName;

    public Text                         TXT_Cutscene;

    void Awake()
    {
        // Load in the cutscene text.
        // Then just display it.
        string path = Application.streamingAssetsPath+"/Cutscenes/"+CTSN_Intro+".txt";
        if(!File.Exists(path)){
            Debug.Log("Cutscene does not exist. Figure it out, buddy.");
            return;
        }

        string cutsceneText = File.ReadAllText(path);
        TXT_Cutscene.text = cutsceneText;
        Debug.Log(cutsceneText);
    }           

    public void F_BTN_Play()
    {
        SO_PlayDetails.mMode = SO_PlayDetails.MODE.CAMPAIGN;
        SO_PlayDetails.mCampaignLevel = NextScenarioName;
        SceneManager.LoadScene("SN_EnemyTesting");
    }

}
