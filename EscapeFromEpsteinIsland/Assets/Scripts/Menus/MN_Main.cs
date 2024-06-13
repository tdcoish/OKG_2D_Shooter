﻿/*************************************************************************************
Adding song playing knowledge. 

Really it's the scenario creation at this point.
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class MN_Main : MonoBehaviour
{
    public GameObject               SCN_Main;
    public GameObject               SCN_Outro;
    public GameObject               SCN_ScenarioCreation;
    public GameObject               SCN_Practice;

    public SO_PlayDetails           SO_PlayDetails;

    bool                            mQuitting = false;
    public float                    _outroTime = 0.5f;
    float                           mOutroTimeStmp;
    public IO_BinaryScore           cScores;
    // Details for the game.
    public bool                     mSetGameToEndlessWaves;

    public int                      mSongInd;
    public AudioSource              mSongPlayer;
    public List<AudioClip>          rSongs;
    public Text                     txt_song;
    public Text                     txt_highScores;

    void Start()
    {
        System.Random rand = new System.Random();
        mSongInd = rand.Next(rSongs.Count);
        mSongPlayer.clip = rSongs[mSongInd];
        mSongPlayer.Play();
        txt_song.text = rSongs[mSongInd].name;
        
        cScores.F_Start();
        string sHighScores = "Top Ten High Scores: ";
        for(int i=0; i<10; i++){
            if(i >= cScores.mHighScores.Count){
                sHighScores += "\n" + (i+1) + ": No Score Yet";
            }else{
                sHighScores += "\n" + (i+1) + ": " + cScores.mHighScores[i].ToString();
            }
        }
        txt_highScores.text = sHighScores;
    }
    
    void Update()
    {
        Cursor.visible = true;
        if(mQuitting){
            if(Time.time - mOutroTimeStmp > _outroTime){
                Application.Quit();
            }
        }

        // When the song finishes, wrap around to the next song.
        if(!mSongPlayer.isPlaying){
            mSongInd++;
            if(mSongInd >= rSongs.Count){
                mSongInd = 0;
            }
            mSongPlayer.clip = rSongs[mSongInd];
            txt_song.text = rSongs[mSongInd].name;
            mSongPlayer.Play();
        }
    }

    public void BTN_HitPlay()
    {
        SO_PlayDetails.mRunEndless = true;
        // WritePlayDetailsToFile(true);
        SceneManager.LoadScene("SN_EnemyTesting");
    }
    public void BTN_HitWaves()
    {
        SO_PlayDetails.mRunEndless = false;
        // WritePlayDetailsToFile(false);
        SceneManager.LoadScene("SN_EnemyTesting");
    }
    // public void WritePlayDetailsToFile(bool endless)
    // {
    //     mSetGameToEndlessWaves = endless;
    //     string path = Application.streamingAssetsPath+"/PlayControl/Details.bin";
    //     FileStream fs = new FileStream(path, FileMode.Create);
    //     BinaryWriter bw = new BinaryWriter(fs);
    //     bw.Write(mSetGameToEndlessWaves);
    //     bw.Close();
    //     fs.Close();
    // }
    public void BTN_CreateScenarios()
    {
        SCN_ScenarioCreation.SetActive(true);
        SCN_Main.SetActive(false);
    }
    public void BTN_HitQuit()
    {
        SCN_Main.SetActive(false);
        SCN_Outro.SetActive(true);
        mOutroTimeStmp = Time.time;
        mQuitting = true;
    }
    public void BTN_WaveCreationBack()
    {
        SCN_ScenarioCreation.SetActive(false);
        SCN_Main.SetActive(true);
    }
    public void BTN_PracticeModeBack()
    {
        SCN_Practice.SetActive(false);
        SCN_Main.SetActive(true);
    }
    public void BTN_PracticeMode()
    {
        SCN_Practice.SetActive(true);
        SCN_Main.SetActive(false);
    }
 
}
