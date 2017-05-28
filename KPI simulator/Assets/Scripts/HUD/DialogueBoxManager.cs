using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DialogueBoxManager : MonoBehaviour
{
    public GameObject dBox;
    public Text dText;
    public Text spaceText;
    public bool talkActive;
    public int currentChoice;
    public bool tutorial;

    public bool ChoiceMode { get; set; }
    private int currentTextLine = 0;
    private List<string> DialogLines;
    private bool dBoxIsMoving = false;
    private bool IsTyping = false;
    private bool SkipTyping = false;
    private Animator dBoxAnim;
    private AudioSource voiceBeep;
    private PhoneController smartphone;
    private string speaker;

    List<string> ChoiceList;
    string choiceComment;

    // Use this for initialization
    void Start ()
    {
        dBoxAnim = GetComponent<Animator>();
        smartphone = FindObjectOfType<PhoneController>();

        dBox.SetActive(false);
        talkActive = false;
        ChoiceMode = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (talkActive && Input.GetKeyDown(KeyCode.Space) && !dBoxIsMoving && !IsTyping && !smartphone.phoneActive) //После окончания печати нажать на пробел
        {
            currentTextLine++;

            if (currentTextLine + 1 > DialogLines.Count) //Если текста не осталось
            {
                talkActive = false;
            }
            else
            {
                StartCoroutine(TypeMessage(DialogLines[currentTextLine]));
            }
        }

        else if (talkActive && Input.GetKeyDown(KeyCode.Space) && !dBoxIsMoving && IsTyping && !smartphone.phoneActive) //Во время печати нажать на пробел
        {
            SkipTyping = true;
        }

        if(ChoiceMode && ( Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ) && !smartphone.phoneActive) //Листать выбор вверх
        {
            if(currentChoice > 0)
            {
                GetComponents<AudioSource>()[0].Play();
                currentChoice--;
                UpdateChoice();
            }
        }
        if (ChoiceMode && ( Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) ) && !smartphone.phoneActive) //Листать выбор вниз
        {
            if(currentChoice < ChoiceList.Count - 1)
            {
                GetComponents<AudioSource>()[0].Play();
                currentChoice++;
                UpdateChoice();
            }
        }
        if (ChoiceMode && Input.GetKeyDown(KeyCode.Return) && !smartphone.phoneActive) //Сделать выбор
        {
            GetComponents<AudioSource>()[1].Play();
            ChoiceMode = false;
            spaceText.text = "Press Space";
        }

    }

    public IEnumerator Talk(string speakerName, List<string> text, AudioSource audio)
    {
        talkActive = true;

        currentTextLine = 0;
        DialogLines = text; //Определить текст для печати
        speaker = speakerName;
        voiceBeep = audio;

        StartCoroutine(TypeMessage(DialogLines[0]));

        while (talkActive)
            yield return null;

    } //Человек говорит несколько фраз

    public IEnumerator Talk(string speakerName, string [] text, AudioSource audio)
    {
        talkActive = true;

        currentTextLine = 0;
        DialogLines = new List<string>();
        DialogLines.AddRange(text);
        speaker = speakerName;
        voiceBeep = audio;

        StartCoroutine(TypeMessage(DialogLines[0]));

        while (talkActive)
            yield return null;
    } 

    public IEnumerator Talk(string[] comment)
    {
        talkActive = true;

        currentTextLine = 0;
        DialogLines = new List<string>();
        foreach (string line in comment)
            DialogLines.Add(line);
        speaker = "";
        voiceBeep = null;

        StartCoroutine(TypeMessage(DialogLines[0]));

        while (talkActive)
            yield return null;
    } //Комментарий на несолько фраз

    public IEnumerator Talk(string comment) //Однострочный комментарийs
    {
        talkActive = true;

        currentTextLine = 0;
        DialogLines = new List<string>() { comment };
        speaker = "";
        voiceBeep = null;

        StartCoroutine(TypeMessage(DialogLines[0]));

        while (talkActive)
            yield return null;
    }

    public IEnumerator MakeChoice(string comment, List<string> choicesList)
    {
        spaceText.text = "";
        ChoiceList = choicesList;
        ChoiceMode = true;
        choiceComment = comment;
        currentChoice = 0;

        UpdateChoice();

        while (ChoiceMode)
            yield return null;
    }

    private IEnumerator TypeMessage(string textLine)
    {
        if (tutorial)
            dText.color = Color.yellow;
        else
            dText.color = Color.white;

        IsTyping = true;
        SkipTyping = false;
        int letter = 0;

        if(speaker != string.Empty)
        {
            dText.text = speaker + ": ";
        }
        else 
            dText.text = string.Empty;

        while(!SkipTyping && (letter < textLine.Length - 1) )
        {
            dText.text += textLine[letter];
            letter += 1;
            if (letter % 3 == 0 && voiceBeep != null)
                voiceBeep.Play();
            
            yield return new WaitForSeconds(0.015f);
        }

        IsTyping = false;

        if (speaker != string.Empty) //In case skip
        {
            dText.text = speaker + ": " + textLine;
        }
        else
            dText.text = textLine;
    }

    void AnimationComplete()
    {
        dBoxIsMoving = false;
    } 

    public IEnumerator ShowDialogBox()
    {
        talkActive = true;
        
        PlayerController PC = FindObjectOfType<PlayerController>();

        dBox.SetActive(true); //Сделать видимым
        dText.text = string.Empty; //Очистить текст

        dBoxIsMoving = true;
        dBoxAnim.SetTrigger("BoxIn");

        while (dBoxIsMoving)
            yield return null;
    } //Начать анимацию появления окна

    public void HideDialogBox()
    {
        PlayerController PC = FindObjectOfType<PlayerController>();
        //PC.canMove = true;
        ChoiceMode = false;
        dBoxAnim.SetTrigger("BoxOut");

    }

    public void DestroyDialogBox()
    {
        dBox.SetActive(false);
        talkActive = false;
    } //Вызываеться по окончанию анимации ухода окна

    private void UpdateChoice()
    {
        string textToShow = choiceComment + "\n";

        for (int i = 0; i < ChoiceList.Count; i++)
        {
            if (i == currentChoice)
                textToShow += "\t> " + ChoiceList[i] + "\n";
            else
                textToShow += "\t" + ChoiceList[i] + "\n";
        }

        textToShow = textToShow.Substring(0, textToShow.Length - 1);
        dText.text = textToShow;
    }
}
