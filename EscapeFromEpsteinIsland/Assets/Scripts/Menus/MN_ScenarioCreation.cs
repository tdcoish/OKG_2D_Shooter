/*************************************************************************************
Also controls menu stuff. 

What is in a scenario file? 
Name
Number of waves
Content of waves
Time before next wave?
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class Wave
{
    public int                      timeBeforeStarting;
    public int                      numNPCs;
    public int                      numSchlomoSpellcasters;
    public int                      numKnightTroons;
    public int                      numHunters;
    public int                      numGrunts;
    public int                      numEliteZOGbots;
    public int                      numBeamers;
    public int                      numBPBerthas;
}

public class Scenario{
    public string                   name;
    public List<Wave>               waves;
}

public class MN_ScenarioCreation : MonoBehaviour
{
    public Text                     txt_currentType;
    public Text                     txt_currentAmount;
    public Text                     txt_waveDetails;
    public Text                     txt_scenarioName;
    public InputField               if_scenarioName;
    public Button                   btn_typePrev;
    public Button                   btn_typeNext;
    public Button                   btn_amountPrev;
    public Button                   btn_amountNext;

    public int                      mCurAmt = 0;
    public int                      mCurType = 0;

    public Dictionary<int, string>  mTypeDictionary;
    public Dictionary<string, int>  mWaveDetails;

    public MN_Main                  cMain;

    void Awake()
    {
        cMain = GetComponent<MN_Main>();
        mTypeDictionary = new Dictionary<int, string>();
        mTypeDictionary.Add(0, "NPC");
        mTypeDictionary.Add(1, "SchlomoSpellcaster");
        mTypeDictionary.Add(2, "Knight/Troon");
        mTypeDictionary.Add(3, "Hunter");
        mTypeDictionary.Add(4, "Grunt");
        mTypeDictionary.Add(5, "Elite/ZOGbot");
        mTypeDictionary.Add(6, "Beamer");
        mTypeDictionary.Add(7, "BodyPositiveBertha");
        // Set the wave amounts to zero.
        mWaveDetails = new Dictionary<string, int>();
        for(int i=0; i<mTypeDictionary.Count; i++){
            mWaveDetails.Add(mTypeDictionary[i], 0);
        }

        txt_currentAmount.text = "Amount: " + mCurAmt;
        txt_currentType.text = "Type: " + mTypeDictionary[mCurType];
        PrintWaveDetails();

        FCreateWaveFileIfNoneExists();
    }

    public void F_BTN_NextAmount()
    {
        mCurAmt++;
        ChangeAmountOfType(mCurAmt);
    }
    public void F_BTN_PrevAmount()
    {
        mCurAmt--;
        if(mCurAmt < 0) mCurAmt = 0;
        ChangeAmountOfType(mCurAmt);
    }
    public void ChangeAmountOfType(int newAmount)
    {
        txt_currentAmount.text = "Amount: " + mCurAmt;
        mWaveDetails[mTypeDictionary[mCurType]] = mCurAmt;
        PrintWaveDetails();
    }
    public void PrintWaveDetails()
    {
        txt_waveDetails.text = "Wave Details:\n";
        for(int i=0; i<mWaveDetails.Count; i++){
            txt_waveDetails.text += mWaveDetails.ElementAt(i).Key + ": " + mWaveDetails.ElementAt(i).Value + "\n";
        }
    }
    public void F_BTN_NextType()
    {
        mCurType++;
        if(mCurType >= mTypeDictionary.Count) mCurType = 0;
        ChangeType();
    }
    public void F_BTN_PrevType()
    {
        mCurType--;
        if(mCurType < 0) mCurType = 0;
        ChangeType();
    }
    public void ChangeType()
    {
        txt_currentType.text = "Type: " + mTypeDictionary[mCurType];
        mCurAmt = mWaveDetails[mTypeDictionary[mCurType]];
        txt_currentAmount.text = "Amount: " + mCurAmt;   
    }

    // Save the wave as a binary, for now.
    public void F_BTN_Save()
    {
        FWriteWaveToFile();
    }
    public void F_BTN_Load()
    {
        FReadWaveFromFile();
    }
    public void F_BTN_Back()
    {
        // Go back to the main menu.
    }

    public void FCreateWaveFileIfNoneExists()
    {
        string path = Application.streamingAssetsPath+"/Waves/FirstWave.bin";
        if(File.Exists(path)){
            Debug.Log("First Wave file already created.");
            return;
        }
        Debug.Log("No wave file, creating.");
        FileStream fStream = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fStream);
        bw.Close();
        fStream.Close();
    }

    public void FWriteWaveToFile()
    {
        Debug.Log("Writing file from disk");
        string path = Application.streamingAssetsPath+"/Waves/FirstWave.bin";
        FileStream fs = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        // We already know the order, the number is enough.
        for(int i=0; i<mWaveDetails.Count; i++){
            bw.Write(mWaveDetails[mTypeDictionary[i]]);
        }
        bw.Close();
        fs.Close();
    }

    public void FReadWaveFromFile()
    {
        Debug.Log("Reading file from disk");
        string path = Application.streamingAssetsPath+"/Waves/FirstWave.bin";
        if(!File.Exists(path)){
            Debug.Log("Wave previously did not exist, making dummy file.");
            FWriteWaveToFile();
        }

        FileStream fStream = new FileStream(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fStream);

        for(int i=0; i<mTypeDictionary.Count; i++){
            mWaveDetails[mTypeDictionary[i]] = br.ReadInt32();
        }

        br.Close();
        fStream.Close();
        PrintWaveDetails();
        mCurAmt = mWaveDetails[mTypeDictionary[mCurType]];
        txt_currentAmount.text = "Amount: " + mCurAmt;
    }

    public void IF_ScenarioNameChanged()
    {
        Debug.Log("Write scenario name change function.");
        txt_scenarioName.text = if_scenarioName.text;
    }
}
