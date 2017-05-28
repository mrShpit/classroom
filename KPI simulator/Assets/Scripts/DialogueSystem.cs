using UnityEngine;
using System.Collections;
using Assets;
using System.Collections.Generic;
using System.Linq;

public class DialogueSystem : MonoBehaviour
{
    private NPC_Character speaker;

    [SerializeField]
    public List<Dialogue> allDialogues;

    public bool dialogueEnabled { get; set; }
    public int currentNodeId { get; set; }
    public Dialogue activeDialogue { get; set; }

    void Start()
    {
        speaker = this.GetComponent<NPC_Character>();
    }

    void Update()
    {
        foreach (Dialogue dialogue_variant in allDialogues)
        {
            if (!dialogue_variant.IsAutomatic || dialogue_variant.Used)
                continue;

            bool CharFlagsOK = Flag.FlagCheck(dialogue_variant.conditionCharFlags, speaker.Character_Flags);
            bool WorldFlagsOK = Flag.FlagCheck(dialogue_variant.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags);

            if (CharFlagsOK && WorldFlagsOK)
            {
                StartCoroutine(Auto_Dialogue(dialogue_variant));
                dialogue_variant.Used = true;
            }
        }
    }

    public IEnumerator Auto_Dialogue(Dialogue dialogueVariant)
    {
        dialogueEnabled = true;
        FindObjectOfType<CameraFollow>().Interlocutor = speaker; // Направить камеру к собеседнику
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        int targetNodeID = dialogueVariant.SavedNode;

        if (dialogueVariant.passTheQuest != string.Empty)
        {
            PassTheQuestAndGetReward(dialogueVariant.passTheQuest);
        }

        while (targetNodeID != -1)
        {
            Debug.Log(targetNodeID);
            DialogueNode currentNode = dialogueVariant.dialogueNodes[targetNodeID];
            if (currentNode.nodeText.Count != 0)
                yield return StartCoroutine(DM.Talk(speaker.characterName, currentNode.nodeText, speaker.voice));

            if (currentNode.nodeChoices.Count == 0)
            {
                targetNodeID = currentNode.autoNodeTransition;
            }
            else
            {
                // Исключить использованные варианты и те, которые недоступны по флагам
                List<DialogueChoice> choiceVariants = new List<DialogueChoice>();
                choiceVariants.AddRange(currentNode.nodeChoices);

                choiceVariants.RemoveAll(choice => choice.Used == true
                    || !Flag.FlagCheck(choice.conditionCharFlags, speaker.Character_Flags)
                    || !Flag.FlagCheck(choice.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags));

                List<string> choicesTexts = choiceVariants.Select(x => x.choiceText).ToList();
                string comment = currentNode.nodeText[currentNode.nodeText.Count - 1];
                yield return StartCoroutine(DM.MakeChoice(comment, choicesTexts));
                int chosen = DM.currentChoice;

                ExecuteChoiceConsequences(choiceVariants[chosen]);
                if (choiceVariants[chosen].OnlyOneUse)
                    choiceVariants[chosen].Used = true;

                if (choiceVariants[chosen].answerText.Count != 0)
                {
                    yield return StartCoroutine(DM.Talk(speaker.characterName, choiceVariants[chosen].answerText, speaker.voice));
                }

                targetNodeID = choiceVariants[chosen].targetNode;

                if (targetNodeID != -1)
                    dialogueVariant.SavedNode = targetNodeID;
            }
        }

        DM.HideDialogBox();

        if (dialogueVariant.OnlyOneUse == true)
        {
            dialogueVariant.Used = true;
        }
        
        FindObjectOfType<CameraFollow>().Interlocutor = null; // Сбросить камеру
        dialogueEnabled = false;
        Save();
    }

    public IEnumerator NPC_Dialogue()
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        
        int targetNodeID = activeDialogue.SavedNode;

        if (activeDialogue.passTheQuest != string.Empty)
        {
            PassTheQuestAndGetReward(activeDialogue.passTheQuest);
        }


        while (targetNodeID != -1)
        {
            DialogueNode currentNode = activeDialogue.dialogueNodes[targetNodeID];

            if(currentNode.nodeText.Count != 0)
                yield return StartCoroutine(DM.Talk(speaker.characterName, currentNode.nodeText, speaker.voice));


            if (currentNode.nodeChoices.Count == 0)
            {
                targetNodeID = currentNode.autoNodeTransition;
            }
            else
            {
                // Исключить использованные варианты и те, которые недоступны по флагам
                List<DialogueChoice> choiceVariants = new List<DialogueChoice>();
                choiceVariants.AddRange(currentNode.nodeChoices);

                choiceVariants.RemoveAll(choice => choice.Used == true
                    || !Flag.FlagCheck(choice.conditionCharFlags, speaker.Character_Flags)
                    || !Flag.FlagCheck(choice.conditionWorldFlags, FindObjectOfType<DirectorController>().WorldFlags));

                List<string> choicesTexts = choiceVariants.Select(x => x.choiceText).ToList();
                string comment = currentNode.nodeText[currentNode.nodeText.Count - 1];
                yield return StartCoroutine(DM.MakeChoice(comment, choicesTexts));
                int chosen = DM.currentChoice;

                ExecuteChoiceConsequences(choiceVariants[chosen]);
                if (choiceVariants[chosen].OnlyOneUse)
                    choiceVariants[chosen].Used = true;

                if (choiceVariants[chosen].answerText.Count != 0)
                {
                    yield return StartCoroutine(DM.Talk(speaker.characterName, choiceVariants[chosen].answerText, speaker.voice));
                }

                targetNodeID = choiceVariants[chosen].targetNode;

                if (targetNodeID != -1)
                    activeDialogue.SavedNode = targetNodeID;
            }
        }

        if (activeDialogue.OnlyOneUse == true)
        {
            activeDialogue.Used = true;
        }

        DM.HideDialogBox();
        Save();
    }

    private void ExecuteChoiceConsequences(DialogueChoice choice)
    {
        if (choice.reputationBonus != 0)
        {
            speaker.ChangeReputation(choice.reputationBonus);
        }

        RaiseFlags(choice.consequencesWorldFlags, choice.consequencesCharFlags);

        if (choice.QuestToGet.Name != string.Empty)
        {
            FindObjectOfType<DirectorController>().activeQuests.Add(choice.QuestToGet);
        }
    }

    private void RaiseFlags(List<Flag> worldFlags, List<Flag> charFlags)
    {
        //Поднять флаги персонажа
        foreach (Flag flagCons in charFlags)
        {
            foreach (Flag flagCurrent in speaker.Character_Flags)
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
            speaker.ChangeReputation(quest.rewardRep);
        

        RaiseFlags(quest.worldConsequences, quest.charConsequences);

    }

    public void ChooseActiveDialogue()
    {
        activeDialogue = null;
        foreach (Dialogue dialogue_variant in allDialogues)
        {
            if (dialogue_variant.Used == true)
                continue;

            bool CharFlagsOK = Flag.FlagCheck(dialogue_variant.conditionCharFlags, speaker.Character_Flags);
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

            if (WorldFlagsOK && CharFlagsOK && AllNeededQuestDone && dialogue_variant.RepNeeded <= speaker.Reputation)
            {
                activeDialogue = dialogue_variant; //Сделать много вариантов диалога на выбора
                break;
            }

        }
    }

    private void Save()
    {
        int portraitInd = FindObjectOfType<DirectorController>().savedObjects.FindIndex(x => x.objectName == this.gameObject.name);
        if(portraitInd == -1)
        {
            FindObjectOfType<DirectorController>().savedObjects.Add(
                new ObjectSaveData()
                {
                    objectName = this.gameObject.name,
                    allDialogues = this.allDialogues,
                    charFlags = this.speaker.Character_Flags,
                    reputation = this.speaker.Reputation,
                }
                );
        }
        else
        {
            ObjectSaveData saveData = FindObjectOfType<DirectorController>().savedObjects[portraitInd];
            saveData.allDialogues = this.allDialogues;
            saveData.charFlags = this.speaker.Character_Flags;
            saveData.reputation = this.speaker.Reputation;
        }
    }
}