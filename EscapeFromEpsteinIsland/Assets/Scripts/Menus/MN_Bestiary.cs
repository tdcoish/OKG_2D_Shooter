using UnityEngine;
using UnityEngine.UI;

public class MN_Bestiary : MonoBehaviour
{
    public Text                 txt_CharTitle;
    public Text                 txt_CharCatchPhrases;
    public Text                 txt_CharDescription;

    public void BTN_CharIconClicked(BTN_BestiaryNPC type)
    {
        txt_CharTitle.text = "Name: " + type.mTitle;
        txt_CharCatchPhrases.text = "Catchphrase: " + type.mCatchphrase;
        txt_CharDescription.text = "Description: " + type.mDescription;
    }

}
