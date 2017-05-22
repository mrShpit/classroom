using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;

public class NPC_Character : CharacterData
{
    [SerializeField]
    private int reputation;
    public int Reputation
    {
        get
        {
            return reputation;
        }

        set
        {
            if (value > 100)
                reputation = 100;
            else if (value < 0)
                reputation = 0;
            else
                reputation = value;
        }
    }
    public List<Flag> Character_Flags;
    public GameObject floatingText;
    public DialogueSystem dialogueSystem { get; set; }

    void Start()
    {
        if(this.GetComponent<DialogueSystem>() != null)
            dialogueSystem = this.GetComponent<DialogueSystem>();

        if (this.GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
    }

    public void ChangeReputation(int rep)
    {
        this.Reputation += rep;
        GameObject clone = (GameObject)Instantiate(this.floatingText, new Vector2(transform.position.x, transform.position.y + 0.8f), Quaternion.Euler(Vector3.zero));
        clone.GetComponent<FloatingText>().Clone = true;

        if (rep > 0)
        {
            clone.GetComponent<FloatingText>().textColor = Color.cyan;
            clone.GetComponent<FloatingText>().text = "+" + rep + " Репутация";
        }
        else if (rep < 0)
        {
            clone.GetComponent<FloatingText>().textColor = Color.red;
            clone.GetComponent<FloatingText>().text = rep + " Репутация";
        }
    }

    public void FloatingTextReaction(string textLine, bool positive)
    {
        float a = this.floatingText.GetComponent<FloatingText>().moveSpeed;
        GameObject clone = (GameObject)Instantiate(this.floatingText, new Vector2(transform.position.x, transform.position.y + 0.8f), Quaternion.Euler(Vector3.zero));
        clone.GetComponent<FloatingText>().Clone = true;
        if (positive)
        {
            clone.GetComponent<FloatingText>().textColor = Color.cyan;
            clone.GetComponent<FloatingText>().text = textLine;
        }
        else
        {
            clone.GetComponent<FloatingText>().textColor = Color.red;
            clone.GetComponent<FloatingText>().text = textLine;
        }
    }
}
