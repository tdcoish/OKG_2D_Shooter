using UnityEngine;

[CreateAssetMenu]
public class SO_PlayDetails : ScriptableObject
{
    public enum MODE {PRACTICE, ARCADE, CAMPAIGN}
    public MODE                     mMode;
    public string                   mCampaignLevel;
    public EN_Base                  PF_Enemy;
}
