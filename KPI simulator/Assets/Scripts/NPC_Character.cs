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

    public DialogueSystem dialogueSystem { get; set; }

    void Start()
    {
        if(this.GetComponent<DialogueSystem>() != null)
            dialogueSystem = this.GetComponent<DialogueSystem>();

        if (this.GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();

    }

}
