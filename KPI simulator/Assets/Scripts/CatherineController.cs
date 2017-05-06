using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CatherineController : MonoBehaviour
{
    private AudioSource voice;
    private bool dialogueEnabled;
    private int flag;

    // Use this for initialization
    void Start()
    {
        if (GetComponent<AudioSource>() != null)
            voice = GetComponent<AudioSource>();
        flag = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E) && !dialogueEnabled)
        {
            dialogueEnabled = true;
            DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
            yield return StartCoroutine(DM.ShowDialogBox());
            bool dialogExit = false;
            List<string> speech = new List<string>()
            {
                "Наконец-то! Где ты шлялся все это время?",
                "Твоя очередь уже совсем скоро",
                "Ты идешь?"
            };
           


            //yield return StartCoroutine(DM.Talk(), voice));
            //yield return StartCoroutine(DM.MakeChoice("Ты идешь?", new List<string>() {
            //    "Ты уже сдала зачет?",
            //    "Да, я уже иду",
            //    "С каких пор ты за меня волнуешься?"
            //}));
            Debug.Log(DM.currentChoice);
            DM.HideDialogBox();
            dialogueEnabled = false;
        }
    }




}