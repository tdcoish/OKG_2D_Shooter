/*************************************************************************************
Adding song playing knowledge. 
*************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MN_Main : MonoBehaviour
{
    public GameObject               SCN_Main;
    public GameObject               SCN_Outro;

    bool                            mQuitting = false;
    public float                    _outroTime = 0.5f;
    float                           mOutroTimeStmp;

    public int                      mSongInd;
    public AudioSource              mSongPlayer;
    public List<AudioClip>          rSongs;
    public Text                     txt_song;

    void Start()
    {
        System.Random rand = new System.Random();
        mSongInd = rand.Next(rSongs.Count);
        mSongPlayer.clip = rSongs[mSongInd];
        mSongPlayer.Play();
        txt_song.text = rSongs[mSongInd].name;
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
