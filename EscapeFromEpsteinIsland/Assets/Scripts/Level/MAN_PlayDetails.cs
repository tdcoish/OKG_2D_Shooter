using UnityEngine;
using System.IO;

public class MAN_PlayDetails : MonoBehaviour
{
    public SO_PlayDetails               SO_PlayDetails;

    // public void F_LoadDetails()
    // {
    //     string path = Application.streamingAssetsPath+"/PlayDetails/details.bin";
    //     if(!File.Exists(path)){
    //         Debug.Log("ERROR! No play details found.");
    //     }

    //     FileStream fStream = new FileStream(path, FileMode.Open);
    //     BinaryReader br = new BinaryReader(fStream);
    //     mEndless = br.ReadBoolean();

    //     br.Close();
    //     fStream.Close();
    // }
}
