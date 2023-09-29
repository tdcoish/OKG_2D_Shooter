using UnityEngine;
using System.Collections.Generic;

public class MAN_Music : MonoBehaviour
{
    public int                              mSongInd;
    public List<AudioClip>                  rSongTracks;
    public AudioSource                      mSongPlayer;

    void Start()
    {
        System.Random rand = new System.Random();
        mSongInd = rand.Next(rSongTracks.Count);
        mSongPlayer.clip = rSongTracks[mSongInd];
        mSongPlayer.Play();
    }

    void Update()
    {
        // When the song finishes, wrap around to the next song.
        if(!mSongPlayer.isPlaying){
            mSongInd++;
            if(mSongInd >= rSongTracks.Count){
                mSongInd = 0;
            }
            mSongPlayer.clip = rSongTracks[mSongInd];
            mSongPlayer.Play();
        }
    }
}
