using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Assets;

public class CommentController : MonoBehaviour
{
    public List<string> MonologueLines = new List<string>();
    public bool highlight;
    public bool autoComment;
    public bool oneUse;
    private bool used;
    public List<Flag> WorldFlagConditions;
    public List<Flag> WorldFlagConsequences; //Флаги поднимуться после комментария или триггера если коммент автоматический и пустой

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (!FindObjectOfType<PlayerController>().changingLocation && otherObject.gameObject.tag == "Player" 
                && (Input.GetKeyDown(KeyCode.E) && !autoComment) && !FindObjectOfType<DialogueBoxManager>().talkActive 
                && !used && Flag.FlagCheck(WorldFlagConditions, FindObjectOfType<DirectorController>().WorldFlags))
        {
            if (oneUse)
                used = true;

            if (this.GetComponent<AudioSource>() != null)
                this.GetComponent<AudioSource>().Play();

            DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
            yield return StartCoroutine(DM.ShowDialogBox());
            yield return StartCoroutine(DM.Talk(string.Empty, MonologueLines, null));
            DM.HideDialogBox();

            RaiseWorldFlags();
        }
    } //Игрок нажал Е. Должен быть текст

    IEnumerator OnTriggerEnter2D(Collider2D otherObject) //Автоматический. Текст быть не обязан
    {
        if (!FindObjectOfType<PlayerController>().changingLocation && otherObject.gameObject.tag == "Player"
            && autoComment && !FindObjectOfType<DialogueBoxManager>().talkActive && !used
             && Flag.FlagCheck(WorldFlagConditions, FindObjectOfType<DirectorController>().WorldFlags))
        {
            if (oneUse)
                used = true;

            if (MonologueLines.Count != 0)
            {
                DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
                if (highlight)
                    DM.tutorial = true;

                yield return StartCoroutine(DM.ShowDialogBox());
                yield return StartCoroutine(DM.Talk(string.Empty, MonologueLines, null));
                DM.HideDialogBox();
                DM.tutorial = false;
            }

            RaiseWorldFlags();
        }
    }

    private void RaiseWorldFlags()
    {
        foreach (Flag flagCons in WorldFlagConsequences)
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
