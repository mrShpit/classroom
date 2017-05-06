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
    public Flag[] Character_Flags;

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
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled && activeDialogue != null)
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

                Debug.Log(choiceVariants.Count);
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

    private void ChooseActiveDialogue()
    {
        activeDialogue = null;
        foreach (Dialogue dialogue_variant in allDialogues)
        {
            if (dialogue_variant.Used == true)
                continue;

            bool CharFlagsOK = Flag.FlagCheck(dialogue_variant.conditionCharFlags, Character_Flags);
            bool WorldFlagsOK = Flag.FlagCheck(dialogue_variant.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags);

            if (WorldFlagsOK && CharFlagsOK)
            {
                activeDialogue = dialogue_variant;
                break;
            }

        }
    }

    private void ExecuteChoiceConsequences(DialogueChoice choice) 
    {
        if (choice.reputationBonus != 0)
        {
            this.Reputation += choice.reputationBonus;
            var clone = (GameObject)Instantiate(floatingText, this.transform.position, Quaternion.Euler(Vector3.zero));
            clone.GetComponent<FloatingText>().Clone = true;

            if (choice.reputationBonus > 0)
            {
                clone.GetComponent<FloatingText>().textColor = Color.cyan;
                clone.GetComponent<FloatingText>().text = "+" + choice.reputationBonus + " REP";
            }
            else  
            {
                clone.GetComponent<FloatingText>().textColor = Color.red;
                clone.GetComponent<FloatingText>().text = choice.reputationBonus + " REP";
            }

            
        }

        //Поднять флаги персонажа
        foreach(Flag flagCons in choice.consequencesCharFlags)
        {
            foreach(Flag flagCurrent in this.Character_Flags)
            {
                if(flagCurrent.flagName == flagCons.flagName)
                {
                    flagCurrent.flagStatus = flagCons.flagStatus;
                    break;
                }
            }
        }

        //Поднять флаги мира
        foreach (Flag flagCons in choice.conditionWorldFlags)
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


}
