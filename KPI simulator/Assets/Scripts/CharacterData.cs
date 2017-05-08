using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Linq;


public class CharacterData : MonoBehaviour
{
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
    public GameObject floatingText;
    
    private Dialogue activeDialogue;
    private bool dialogueEnabled = false;
    private int currentNodeId;
    private AudioSource voice;

    // Use this for initialization
    void Start ()
    {
        if (GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        ChooseActiveDialogue();
        PhoneController phone = FindObjectOfType<PhoneController>();
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null && !phone.phoneActive)
        {
            yield return StartCoroutine(NPD_Dialogue());
        }
    }

    IEnumerator NPD_Dialogue()
    {
        dialogueEnabled = true;
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        int targetNodeID = 0;

        if(activeDialogue.passTheQuest != string.Empty)
        {
            PassTheQuestAndGetReward(activeDialogue.passTheQuest);
        }

        while (targetNodeID != -1)
        {
            DialogueNode currentNode = activeDialogue.dialogueNodes[targetNodeID];
            yield return StartCoroutine(DM.Talk(currentNode.nodeText, voice));

            if(currentNode.nodeChoices.Count == 0)
            {
                targetNodeID = -1;
            }
            else
            {
                // Исключить использованные варианты и те, которые недоступны по флагам
                List<DialogueChoice> choiceVariants = new List<DialogueChoice>();
                choiceVariants = currentNode.nodeChoices;

                choiceVariants.RemoveAll(choice => choice.Used == true
                    || !Flag.FlagCheck(choice.conditionCharFlags, Character_Flags)
                    || !Flag.FlagCheck(choice.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags));
                
                List <string> choicesTexts = choiceVariants.Select(x => x.choiceText).ToList();
                string comment = currentNode.nodeText[currentNode.nodeText.Count - 1];
                yield return StartCoroutine(DM.MakeChoice(comment, choicesTexts));
                int chosen= DM.currentChoice;

                ExecuteChoiceConsequences(choiceVariants[chosen]);
                if (choiceVariants[chosen].OnlyOneUse)
                    choiceVariants[chosen].Used = true;
            
                if(choiceVariants[chosen].answerText.Count != 0)
                {
                    yield return StartCoroutine(DM.Talk(choiceVariants[chosen].answerText, voice));
                }

                targetNodeID = choiceVariants[chosen].targetNode;
            }
        }


        DM.HideDialogBox();
        dialogueEnabled = false;

        if (activeDialogue.OnlyOneUse == true)
        {
            activeDialogue.Used = true;
        }
        
    }

    private void ChangeReputation(int rep)
    {
        this.Reputation += rep;
        var clone = (GameObject)Instantiate(floatingText, this.transform.position, Quaternion.Euler(Vector3.zero));
        clone.GetComponent<FloatingText>().Clone = true;

        if (rep > 0)
        {
            clone.GetComponent<FloatingText>().textColor = Color.cyan;
            clone.GetComponent<FloatingText>().text = "+" + rep + " REP";
        }
        else
        {
            clone.GetComponent<FloatingText>().textColor = Color.red;
            clone.GetComponent<FloatingText>().text = rep + " REP";
        }
    }

    private void ChooseActiveDialogue()
    {
        activeDialogue = null;
        foreach (Dialogue dialogue_variant in allDialogues)
        {
            if (dialogue_variant.Used == true)
                continue;

            bool CharFlagsOK = Flag.FlagCheck(dialogue_variant.conditionCharFlags, Character_Flags);
            bool WorldFlagsOK = Flag.FlagCheck(dialogue_variant.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags);
            bool AllNeededQuestDone = true;
            foreach(string questName in dialogue_variant.questsPassedNeeded)
            {
                if ( !FindObjectOfType<DirectorController>().CheckQuestPassed(questName) )
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

    private void ExecuteChoiceConsequences(DialogueChoice choice) 
    {
        if (choice.reputationBonus != 0)
        {
            ChangeReputation(choice.reputationBonus);
        }

        RaiseFlags(choice.consequencesWorldFlags, choice.consequencesCharFlags);

        if(choice.QuestToGet.Name != string.Empty)
        {
            FindObjectOfType<DirectorController>().activeQuests.Add(choice.QuestToGet);
        }
    }

    private void RaiseFlags(List<Flag> worldFlags, List<Flag> charFlags)
    {
        //Поднять флаги персонажа
        foreach (Flag flagCons in charFlags)
        {
            foreach (Flag flagCurrent in this.Character_Flags)
            {
                if (flagCurrent.flagName == flagCons.flagName)
                {
                    flagCurrent.flagStatus = flagCons.flagStatus;
                    break;
                }
            }
        }

        //Поднять флаги мира
        foreach (Flag flagCons in worldFlags)
        {
            foreach (Flag flagCurrent in FindObjectOfType<DirectorController>().WorldFlags)
            {
                if (flagCurrent.flagName == flagCons.flagName)
                {
                    flagCurrent.flagStatus = flagCons.flagStatus;
                    break;
                }
            }
        }
    }

    private void PassTheQuestAndGetReward(string questName)
    {
        DirectorController director = FindObjectOfType<DirectorController>();
        Quest quest = director.finishedQuests.Find(x => x.Name == questName);

        if (quest.rewardRep != 0)
            ChangeReputation(quest.rewardRep);
        //Дать деньги и опыт

        RaiseFlags(quest.worldConsequences, quest.charConsequences);

    }
}
