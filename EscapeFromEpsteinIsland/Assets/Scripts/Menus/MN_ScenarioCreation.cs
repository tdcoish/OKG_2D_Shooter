/*************************************************************************************
Also controls menu stuff. 

Better UI design, buttons beside each enemy type, so you don't have to scroll to them 
first. 

Luckily, each wave already saves the number of types of enemies.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public class Wave
{
    public int                      mTimeBeforeStarting;
    public Dictionary<string, int>  mNumEnemies;

    // Really, this should be saved in an additional text file.
    public Wave()
    {
        mTimeBeforeStarting = 10;
        mNumEnemies = new Dictionary<string, int>();
        mNumEnemies.Add("NPC", 0);
        mNumEnemies.Add("SchlomoSpellcaster", 0);
        mNumEnemies.Add("Knight/Troon", 0);
        mNumEnemies.Add("Hunter", 0);
        mNumEnemies.Add("Grunt", 0);
        mNumEnemies.Add("Elite/ZOGbot", 0);
        mNumEnemies.Add("Beamer", 0);
        mNumEnemies.Add("BodyPositiveBertha", 0);
        mNumEnemies.Add("Antifa", 0);
        mNumEnemies.Add("Jequeerus", 0);
        mNumEnemies.Add("Shaniqua", 0);
        mNumEnemies.Add("GruntCaptain", 0);
        mNumEnemies.Add("FtM", 0);
        mNumEnemies.Add("MGunner", 0);
    }
}

public class Scenario{
    public string                   mName;
    public List<Wave>               mWaves;

    public Scenario()
    {
        mWaves = new List<Wave>();
        mName = "Name Me Daddy";
    }

    public void FLoadScenarioFromFile(string name)
    {
        Debug.Log("Reading file from disk");
        string path = Application.streamingAssetsPath+"/Scenarios/"+name+".sro";
        if(!File.Exists(path)){
            Debug.Log("Scenario does not exist. Figure it out, buddy.");
            return;
        }

        Dictionary<int, string> mTypeDictionary = new Dictionary<int, string>();
        mTypeDictionary.Add(0, "NPC");
        mTypeDictionary.Add(1, "SchlomoSpellcaster");
        mTypeDictionary.Add(2, "Knight/Troon");
        mTypeDictionary.Add(3, "Hunter");
        mTypeDictionary.Add(4, "Grunt");
        mTypeDictionary.Add(5, "Elite/ZOGbot");
        mTypeDictionary.Add(6, "Beamer");
        mTypeDictionary.Add(7, "BodyPositiveBertha");
        mTypeDictionary.Add(8, "Antifa");
        mTypeDictionary.Add(9, "Jequeerus");
        mTypeDictionary.Add(10, "Shaniqua");
        mTypeDictionary.Add(11, "GruntCaptain");
        mTypeDictionary.Add(12, "FtM");
        mTypeDictionary.Add(13, "MGunner");

        FileStream fStream = new FileStream(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fStream);
        mName = br.ReadString();
        Debug.Log("Read in scenario name: " + mName);
        int numWaves = br.ReadInt32();
        Debug.Log("Numer of waves: " + numWaves);
        mWaves.Clear();
        for(int i=0; i<numWaves; i++){
            Wave tempWave = new Wave();
            tempWave.mTimeBeforeStarting = br.ReadInt32();
            int numTypes = br.ReadInt32();
            for(int j=0; j<numTypes; j++){
                tempWave.mNumEnemies[mTypeDictionary[j]] = br.ReadInt32();
            }
            mWaves.Add(tempWave);
        }

        br.Close();
        fStream.Close();
    }
}

public class MN_ScenarioCreation : MonoBehaviour
{
    public Text                     txt_scenarioName;
    public Text                     txt_numWaves;
    public Text                     txt_curWaveIndex;
    public Text                     txt_timeBeforeWaveStarts;
    public InputField               if_scenarioName;
    public Dropdown                 dp_scenario;
    public Button                   btn_typePrev;
    public Button                   btn_typeNext;
    public Button                   btn_amountPrev;
    public Button                   btn_amountNext;

    public List<MN_EntityDetails>   rEntityDetailShowers;

    public Dictionary<int, string>  mTypeDictionary;
    public int                      mActiveWaveIndex = 0;
    public Scenario                 mActiveScenario;

    public MN_Main                  cMain;

    void Awake()
    {
        cMain = GetComponentInParent<MN_Main>();
        PopulateScenarioDropdownMenu();

        // Creating a dummy scenario just to see what happens.
        mActiveScenario = new Scenario();
        mActiveScenario.mName = "Dummy Scenario";
        Wave dummyFirstWave = new Wave();
        dummyFirstWave.mNumEnemies["NPC"] = 3;
        dummyFirstWave.mNumEnemies["Hunter"] = 3;
        dummyFirstWave.mNumEnemies["Grunt"] = 7;
        dummyFirstWave.mTimeBeforeStarting = 1;
        Wave dummySecondWave = new Wave();
        dummySecondWave.mNumEnemies["NPC"] = 5;
        dummySecondWave.mNumEnemies["Hunter"] = 1;
        mActiveScenario.mWaves.Add(dummyFirstWave);
        mActiveScenario.mWaves.Add(dummySecondWave);

        mTypeDictionary = new Dictionary<int, string>();
        mTypeDictionary.Add(0, "NPC");
        mTypeDictionary.Add(1, "SchlomoSpellcaster");
        mTypeDictionary.Add(2, "Knight/Troon");
        mTypeDictionary.Add(3, "Hunter");
        mTypeDictionary.Add(4, "Grunt");
        mTypeDictionary.Add(5, "Elite/ZOGbot");
        mTypeDictionary.Add(6, "Beamer");
        mTypeDictionary.Add(7, "BodyPositiveBertha");
        mTypeDictionary.Add(8, "Antifa");
        mTypeDictionary.Add(9, "Jequeerus");
        mTypeDictionary.Add(10, "Shaniqua");
        mTypeDictionary.Add(11, "GruntCaptain");
        mTypeDictionary.Add(12, "FtM");
        mTypeDictionary.Add(13, "MGunner");

        PrintScenarioDetails();
    }

    public void F_BTN_IncWaveAmt()
    {
        mActiveScenario.mWaves.Add(new Wave());
        PrintScenarioDetails();
    }
    public void F_BTN_DecWaveAmt()
    {
        // Can't go lower than one wave.
        if(mActiveScenario.mWaves.Count <= 1) return;
        mActiveScenario.mWaves.RemoveAt(mActiveScenario.mWaves.Count-1);
        if(mActiveWaveIndex > mActiveScenario.mWaves.Count-1){
            mActiveWaveIndex = mActiveScenario.mWaves.Count-1;
            txt_curWaveIndex.text = "Current Wave: " + (mActiveWaveIndex+1);
        }
        PrintScenarioDetails();
    }
    public void F_BTN_NextWave()
    {
        mActiveWaveIndex++;
        if(mActiveWaveIndex > mActiveScenario.mWaves.Count-1){
            mActiveWaveIndex = mActiveScenario.mWaves.Count-1;
        }
        txt_curWaveIndex.text = "Current Wave: " + (mActiveWaveIndex+1);
        PrintScenarioDetails();
        txt_timeBeforeWaveStarts.text = "Time before wave starts: " + mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting.ToString();
    }
    public void F_BTN_PrevWave()
    {
        mActiveWaveIndex--;
        if(mActiveWaveIndex <= 0){
            mActiveWaveIndex = 0;
        }
        txt_curWaveIndex.text = "Current Wave: " + (mActiveWaveIndex+1);
        PrintScenarioDetails();
        txt_timeBeforeWaveStarts.text = "Time before wave starts: " + mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting.ToString();
    }

    public void F_BTN_NextAmount(MN_EntityDetails m)
    {
        mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies[m.mType]++;
        PrintScenarioDetails();
    }
    public void F_BTN_PrevAmount(MN_EntityDetails m)
    {
        mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies[m.mType]--;
        if(mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies[m.mType] < 0){
            mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies[m.mType] = 0;
        }
        PrintScenarioDetails();
    }

    public void PrintScenarioDetails()
    {
        txt_scenarioName.text = mActiveScenario.mName;
        for(int i=0; i<mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies.Count; i++){
            for(int j=0; j<rEntityDetailShowers.Count; j++){
                rEntityDetailShowers[j].txt_amt.text = mActiveScenario.mWaves[mActiveWaveIndex].mNumEnemies[rEntityDetailShowers[j].mType].ToString();
            }
        }

        txt_numWaves.text = mActiveScenario.mWaves.Count.ToString();
        txt_timeBeforeWaveStarts.text = "Time before wave starts: " + mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting.ToString();
    }

    // Save the wave as a binary, for now.
    public void F_BTN_Save()
    {
        Debug.Log("Writing scenario file to disk");
        string path = Application.streamingAssetsPath+"/Scenarios/"+mActiveScenario.mName+".sro";
        FileStream fs = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        // We already know the order, the number is enough.
        bw.Write(mActiveScenario.mName);
        Debug.Log("Scenario name: " + mActiveScenario.mName);
        // Absolutely need to know the number of waves.
        bw.Write(mActiveScenario.mWaves.Count);
        Debug.Log("Number of waves: " + mActiveScenario.mWaves.Count);
        for(int i=0; i<mActiveScenario.mWaves.Count; i++){
            bw.Write(mActiveScenario.mWaves[i].mTimeBeforeStarting);
            bw.Write(mActiveScenario.mWaves[i].mNumEnemies.Count);
            for(int j=0; j<mActiveScenario.mWaves[i].mNumEnemies.Count; j++){
                bw.Write(mActiveScenario.mWaves[i].mNumEnemies[mTypeDictionary[j]]);
            }
        }

        bw.Close();
        fs.Close();

        PopulateScenarioDropdownMenu();
    }

    public void F_BTN_Back()
    {
        cMain.SCN_Main.SetActive(true);
        cMain.SCN_ScenarioCreation.SetActive(false);
    }
    public void F_BTN_Play()
    {
        // Go to Enemy Testing scene, but loading in the right scenario.
        SceneManager.LoadScene("SN_EnemyTesting");
    }

    public void F_BTN_NextTimeAmt()
    {
        mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting += 1;
        txt_timeBeforeWaveStarts.text = "Time before wave starts: " + mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting.ToString();
    }
    public void F_BTN_PrevTimeAmt()
    {
        mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting -= 1;
        if(mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting < 1){
            mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting = 1;    
        }
        txt_timeBeforeWaveStarts.text = "Time before wave starts: " + mActiveScenario.mWaves[mActiveWaveIndex].mTimeBeforeStarting.ToString();
    }

    public void IF_ScenarioNameChanged()
    {
        Debug.Log("Write scenario name change function.");
        txt_scenarioName.text = if_scenarioName.text;
        mActiveScenario.mName = if_scenarioName.text;
    }

    public void PopulateScenarioDropdownMenu()
    {
        dp_scenario.ClearOptions();
        string directoryPath = Application.streamingAssetsPath+"/Scenarios/";
        string[] scenarioFiles = Directory.GetFiles(directoryPath, "*.sro");
        foreach(string f in scenarioFiles){
            string name = f;
            name = name.Replace(directoryPath, "");   // replace with nothing.
            name = name.Replace(".sro", "");
            Dropdown.OptionData temp = new Dropdown.OptionData();
            temp.text = name;
            dp_scenario.options.Add(temp);
        }
    }

    public void DP_ScenarioSelected()
    {
        Debug.Log("selected: " + dp_scenario.options[dp_scenario.value].text);
        mActiveScenario.FLoadScenarioFromFile(dp_scenario.options[dp_scenario.value].text);
        if_scenarioName.text = mActiveScenario.mName;
        mActiveWaveIndex = 0;
        txt_curWaveIndex.text = "Current Wave: " + (mActiveWaveIndex+1);
        PrintScenarioDetails();
    }
}
