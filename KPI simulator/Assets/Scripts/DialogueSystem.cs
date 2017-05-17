using UnityEngine;
using System.Collections;
using Assets;
using System.Collections.Generic;
using System.Linq;

public class DialogueSystem : MonoBehaviour
{
    NPC_CharacterData speaker;
    public GameObject floatingText;

    public IEnumerator NPD_Dialogue(NPC_CharacterData SpeakingCharacter)
    {
        speaker = SpeakingCharacter;
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        int targetNodeID = 0;

        if (speaker.activeDialogue.passTheQuest != string.Empty)
        {
            PassTheQuestAndGetReward(speaker.activeDialogue.passTheQuest);
        }

        while (targetNodeID != -1)
        {
            DialogueNode currentNode = speaker.activeDialogue.dialogueNodes[targetNodeID];
            yield return StartCoroutine(DM.Talk(speaker.Name, currentNode.nodeText, speaker.voice));

            if (currentNode.nodeChoices.Count == 0)
            {
                targetNodeID = -1;
            }
            else
            {
                // Исключить использованные варианты и те, которые недоступны по флагам
                List<DialogueChoice> choiceVariants = new List<DialogueChoice>();
                choiceVariants = currentNode.nodeChoices;

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
                    yield return StartCoroutine(DM.Talk(speaker.Name, choiceVariants[chosen].answerText, speaker.voice));
                }

                targetNodeID = choiceVariants[chosen].targetNode;
            }
        }

        if (speaker.activeDialogue.OnlyOneUse == true)
        {
            speaker.activeDialogue.Used = true;
        }

        DM.HideDialogBox();
    }

    private void ChangeReputation(int rep)
    {
        speaker.Reputation += rep;
        GameObject clone = (GameObject)Instantiate(this.floatingText, new Vector2(transform.position.x, transform.position.y + 0.8f), Quaternion.Euler(Vector3.zero));
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

    private void ExecuteChoiceConsequences(DialogueChoice choice)
    {
        if (choice.reputationBonus != 0)
        {
            ChangeReputation(choice.reputationBonus);
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
            ChangeReputation(quest.rewardRep);
        //Дать деньги и опыт

        RaiseFlags(quest.worldConsequences, quest.charConsequences);

    }
}
