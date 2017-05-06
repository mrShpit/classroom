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
        public Flag[] conditionCharFlags;
        public Flag[] conditionWorldFlags;
        public List<DialogueNode> dialogueNodes;
        public bool OnlyOneUse;

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

        public static bool FlagCheck(Flag [] list, Flag flagToComare)
        {

            foreach (Flag f in list)
            {
                if (f.flagName == flagToComare.flagName)
                {
                    if (f.flagStatus == flagToComare.flagStatus)
                        return true;
                    else
                        return false;
                }
            }

            return false;
        }

        public static bool FlagCheck(Flag[] conditionFlags, Flag[] givenFlags)
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
        public Flag[] conditionCharFlags;
        public Flag[] conditionWorldFlags;
        public int reputationBonus;
        public Flag[] consequencesWorldFlags;
        public Flag[] consequencesCharFlags;
        public bool OnlyOneUse;

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


}
