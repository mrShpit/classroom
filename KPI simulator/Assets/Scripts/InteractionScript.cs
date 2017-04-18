using UnityEngine;
using System.Collections;

public class InteractionScript : MonoBehaviour {

    public TextAsset textFile;

    public void Interact()
    {
        TextBoxManager tb_manager = FindObjectOfType<TextBoxManager>();
        tb_manager.EnableTextPanel(textFile);
    }
}
