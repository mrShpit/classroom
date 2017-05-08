using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    [System.Serializable]
    public class Quest
    {
        public string Name;
        public string Comment;

        public List<Flag> worldFlagsNeededToDone;
        //Добавить neededItemsToDone

        public int rewardRep;
        public int rewardMoney;
        public int rewardXP;
        public List<Flag> worldConsequences;
        public List<Flag> charConsequences;
    }
}
