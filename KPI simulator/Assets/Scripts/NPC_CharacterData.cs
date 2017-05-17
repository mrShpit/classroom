using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;

public class NPC_CharacterData : MonoBehaviour {

    public string Name;

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

    [SerializeField]
    public List<Dialogue> allDialogues;

    public Dialogue activeDialogue { get; set; }
    public bool dialogueEnabled { get; set; }
    public int currentNodeId { get; set; }
    public AudioSource voice { get; set; }


    public void ChooseActiveDialogue()
    {
        activeDialogue = null;
        foreach (Dialogue dialogue_variant in allDialogues)
        {
            if (dialogue_variant.Used == true)
                continue;

            bool CharFlagsOK = Flag.FlagCheck(dialogue_variant.conditionCharFlags, Character_Flags);
            bool WorldFlagsOK = Flag.FlagCheck(dialogue_variant.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags);
            bool AllNeededQuestDone = true;
            foreach (string questName in dialogue_variant.questsPassedNeeded)
            {
                if (!FindObjectOfType<DirectorController>().CheckQuestPassed(questName))
                {
                    AllNeededQuestDone = false;
                    break;
                }
            }

            if (WorldFlagsOK && CharFlagsOK && AllNeededQuestDone && dialogue_variant.RepNeeded <= this.Reputation)
            {
                activeDialogue = dialogue_variant; //Сделать много вариантов диалога на выбора
                break;
            }

        }
    }
}
