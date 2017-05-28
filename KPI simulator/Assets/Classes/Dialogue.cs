using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    [System.Serializable]
    public class Dialogue
    {
        public string DialogueName;
        public bool IsAutomatic;
        public List<Flag> conditionCharFlags;
        public List<Flag> conditionWorldFlags;
        public List<string> questsPassedNeeded;
        public string passTheQuest;
        public int RepNeeded; 
        public List<DialogueNode> dialogueNodes;
        public bool OnlyOneUse;
        public int SavedNode { get; set; }

        private bool used;

        public bool Used
        {
            get
            {
                return used;
            }

            set
            {
                used = value;
            }
        }
    }

    [System.Serializable]
    public class Flag
    {
        public string flagName;
        public int flagStatus;
        public Flag(string name, int status)
        {
            flagName = name;
            flagStatus = status;
        }

        public static bool FlagCheck(List<Flag> givenFlags, Flag flagToComare)
        {

            Flag flag = null;
            flag = givenFlags.Find(x => x.flagName == flagToComare.flagName);
            if (flag != null && flag.flagStatus == flagToComare.flagStatus)
                return true;
            else
                return false;
        }



        public static bool FlagCheck(List<Flag> conditionFlags, List<Flag> givenFlags)
        {
            foreach(Flag condition in conditionFlags)
            {
                if (!Flag.FlagCheck(givenFlags, condition))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [System.Serializable]
    public class DialogueNode
    {
        public int nodeId;
        public List<string> nodeText;
        public int autoNodeTransition; //Если нету вариантов ответа: -1 чтобы выйти из диалога, >=0 чтобы перейти к другому узлу диалога
        public List<DialogueChoice> nodeChoices = new List<DialogueChoice>();
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int targetNode;
        public List<string> answerText;
        public List<Flag> conditionCharFlags;
        public List<Flag> conditionWorldFlags;
        public int reputationBonus;
        public List<Flag> consequencesWorldFlags;
        public List<Flag> consequencesCharFlags;
        public bool OnlyOneUse;
        public Quest QuestToGet;

        private bool used;

        public bool Used
        {
            get
            {
                return used;
            }

            set
            {
                used = value;
            }
        }
    }

    [System.Serializable]
    public class ObjectSaveData
    {
        public string objectName;
        public int reputation;
        public List<Flag> charFlags;
        public List<Dialogue> allDialogues;
        public List<bool> usedComments;
    }
}