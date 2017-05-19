using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;

public class DirectorController : MonoBehaviour
{
    private static bool ItExists;
    public List<Flag> WorldFlags;
    public List<Quest> activeQuests;
    public List<Quest> finishedQuests;
    public List<Quest> passedQuests;

    // Use this for initialization
    void Start ()
    {
        if (!ItExists)
        {
            ItExists = true;
            DontDestroyOnLoad(transform.gameObject);
            //Тут выполняеться загрузка при старте игры
        }
        else
        {
            Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        CheckQuestFinished();
        if (Input.GetKeyDown(KeyCode.Backspace))
            Application.Quit();
    }

    public bool CheckQuestPassed(string name)
    {
        var quest = this.passedQuests.FirstOrDefault(x => x.Name == name);
        if (quest != null)
            return true;
        else
            return false;
    }

    void CheckQuestFinished()
    {
        Quest questFinished = null;
        foreach (Quest quest in this.activeQuests)
        {
            if (Flag.FlagCheck(quest.worldFlagsNeededToDone, this.WorldFlags))
            {
                questFinished = quest;
                break;
            }
        }

        if (questFinished != null)
        {
            this.activeQuests.Remove(questFinished);
            this.finishedQuests.Add(questFinished);
        }
    }



}
