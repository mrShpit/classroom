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
    public bool used { get; set; }
    public List<Flag> WorldFlagConditions;
    public List<Flag> WorldFlagConsequences; //Флаги поднимуться после комментария или триггера если коммент автоматический и пустой

    void Start()
    {
        int portraitInd = FindObjectOfType<DirectorController>().savedObjects.FindIndex(x => x.objectName == this.gameObject.name);

        if (portraitInd != -1) //Загрузка информации
        {
            ObjectSaveData savedPortrait = FindObjectOfType<DirectorController>().savedObjects[portraitInd];
            CommentController [] thisObjectComments = this.gameObject.GetComponents<CommentController>();
            for (int i = 0; i < thisObjectComments.Length; i++)
            {
                if (this == thisObjectComments[i])
                {
                    this.used = savedPortrait.usedComments[i];
                    break;
                }
            }            
        }
    }

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
            Save();
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

            if (this.GetComponent<AudioSource>() != null)
                this.GetComponent<AudioSource>().Play();

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
            Save();
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

    private void Save()
    {
        int portraitInd = FindObjectOfType<DirectorController>().savedObjects.FindIndex(x => x.objectName == this.gameObject.name);
        if (portraitInd == -1)
        {
            List<bool> usedFactors = new List<bool>();
            CommentController[] thisObjectComments = this.gameObject.GetComponents<CommentController>();
            foreach (CommentController comment in thisObjectComments)
            {
                usedFactors.Add(comment.used);
            }
            FindObjectOfType<DirectorController>().savedObjects.Add(
                new ObjectSaveData()
                {
                    objectName = this.gameObject.name,
                    usedComments = usedFactors
                }
                );
        }
        else
        {
            ObjectSaveData saveData = FindObjectOfType<DirectorController>().savedObjects[portraitInd];
            for(int i = 0; i < saveData.usedComments.Count; i++)
            {
                saveData.usedComments[i] = this.gameObject.GetComponents<CommentController>()[i].used;
            }
        }
    }
}
