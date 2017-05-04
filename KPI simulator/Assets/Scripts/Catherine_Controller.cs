using UnityEngine;
using System.Collections;

public class Catherine_Controller : MonoBehaviour {

    string[] dialogue;

	// Use this for initialization
	void Start ()
    {
        dialogue = new string[] { "А? Это снова ты?", "Хватит крутиться вокруг меня, лузер!"};

    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager DM = FindObjectOfType<DialogueManager>();
            yield return StartCoroutine(DM.StartDialogue(dialogue));
        }
    }
}
