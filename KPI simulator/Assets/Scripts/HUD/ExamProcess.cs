using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class ExamProcess : MonoBehaviour
{
    public bool ExamIsRunning;
    public GameObject ExamChoiceBox;
    public GameObject chair;
    public GameObject actionGrid;
    public Button actionButton;
    public GameObject QuestionBar;

    public Text CharacterNameTB;
    public Text LevelTB;
    public Text StressLevelTB;
    public Text SubjectSkillTB;
    public Text CommentTB;
    public Text StudentAnswerProgressTB;
    public Text StudentAnswerQualityTB;
    public Text QuestionInfo; 
 
    private StudentData activeStudent;
    enum TargetType { think, speak, act, passive };
    private bool playerMove;
    private TeacherData teacher;

    private float fullAnswerProgress;
    public float FullAnswerProgress
    {
        get
        {
            return fullAnswerProgress;
        }
        set
        {
            if (value > 100)
                value = 100;
            fullAnswerProgress = value;
            QuestionBar.GetComponentsInChildren<Image>()[3].fillAmount = value / 100;
        }
    }

    private float currentPatience;
    public float CurrentPatience
    {
        get
        {
            return currentPatience;
        }
        set
        {
            currentPatience = value;
            QuestionBar.GetComponentsInChildren<Image>()[6].fillAmount = value / 100;
        }
    }

    // Use this for initialization
    void Start()
    {
        ExamChoiceBox.SetActive(false);
        QuestionBar.SetActive(false);
    }

    public IEnumerator StartExam(TeacherData currentTeacher, List<StudentData> allStudents)
    {
        if(allStudents.Count == 0)
        {
            Debug.Log("Must be at least one student");
            yield break;
        }

        teacher = currentTeacher;

        ExamIsRunning = true;
        FindObjectOfType<PhoneController>().canUse = false;
        
        GameObject studentObject = allStudents[0].gameObject; //Позже сделать это для всего списка
        ScreenFader sf = GameObject.FindGameObjectWithTag("Fader").GetComponent<ScreenFader>();
        yield return StartCoroutine(sf.FadeToBlack()); //Затемнить экран
        studentObject.transform.position = currentTeacher.GetComponent<TeacherData>().ExamSitPlaces[0];
        GameObject clone = (GameObject)Instantiate(chair, studentObject.transform.position, Quaternion.Euler(Vector3.zero)); //Посадить на стулья
        yield return StartCoroutine(sf.FadeToClear()); //Снова показать экран

        this.gameObject.transform.SetAsLastSibling(); //Сделать панель активной для EventSystem

        for (int i = 0; i < allStudents.Count; i++)
            allStudents[i].currentAnswer = new Answer();

        activeStudent = allStudents[0]; //Тут повтор кода, исправить потом  

        float sumPatience = 0f;
        float sumSuccess = 0f;

        for (int i = 0; i < 2; i++) // Количество вопросов
        {
            yield return StartCoroutine(askQuestion()); //Задать вопрос
            //Указать вопрос на QuestionBar

            QuestionBar.SetActive(true);
            ExamChoiceBox.SetActive(true);
            QuestionInfo.text = teacher.subjectName + ". Теория. Сложность: " + teacher.GetComponent<CharacterData>().level;
            FullAnswerProgress = 0f; // Начальное состояние общего прогресса ответа
            CurrentPatience = 100f;
            
            while (CurrentPatience > 0 && FullAnswerProgress < 100f) //Пока преподу не надоело или ответ не готов
            {
                UpdateExamChoiceBox(activeStudent);
                playerMove = true;
                while (playerMove)
                    yield return null; //Ход игрока

                //teacher interaction
                CurrentPatience -= teacher.AnnoySpeed;
            }

            ExamChoiceBox.SetActive(false);

            sumPatience += CurrentPatience;
            sumSuccess += FullAnswerProgress;
        }
        
        QuestionBar.SetActive(false);
        yield return StartCoroutine( giveResult( GetMark(sumSuccess / 2 , sumPatience / 2) ) ); // 2 - количество вопросов, сделать универсальным
        
        ExamIsRunning = false;
    }

    private string GetMark(float avSuccessPercent, float avAnnoyPersent)
    {
        float scorePercent = (avSuccessPercent + avAnnoyPersent) / 2;

        if (scorePercent >= 95)
            return "A";
        else if (scorePercent >= 90)
            return "B";
        else if (scorePercent >= 80)
            return "C";
        else if (scorePercent >= 70)
            return "D";
        else if (scorePercent >= 60)
            return "E";
        else
            return "F";
    }

    void UpdateExamChoiceBox(StudentData student) // Будет перегрузка для любого студента
    {
        CharacterNameTB.text = student.GetComponent<CharacterData>().characterName;
        LevelTB.text = "Уровень " + student.GetComponent<CharacterData>().level.ToString();
        StressLevelTB.text = "Стресс: " + student.GetComponent<StudentData>().currentStress + "%";
        SubjectSkillTB.text = "Знание предмета: " + student.AverageSubjectSkill(teacher.usedSubjects) + "/10";
        StudentAnswerProgressTB.text = "Ответ: " + student.currentAnswer.AnswerProgress + "%";
        StudentAnswerQualityTB.text = "Качество: " + student.currentAnswer.AnswerQuality + "%";
    }

    IEnumerator askQuestion()
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        List<string> text = new List<string>() { "Надеюсь, это не будет для тебя слишком сложно", "Я шучу", "Мне плевать" };
        yield return StartCoroutine(DM.Talk(teacher.GetComponent<CharacterData>().characterName , text , teacher.GetComponent<CharacterData>().voice));
        yield return StartCoroutine(DM.Talk("*Сенсей задает вопрос"));
        DM.HideDialogBox();
    }

    IEnumerator giveResult(string mark)
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        List<string> text = new List<string>() { "Зачет окончен", "Твой результат: " + mark , "Можешь быть свободен"};
        yield return StartCoroutine(DM.Talk(teacher.GetComponent<CharacterData>().characterName, text, teacher.GetComponent<CharacterData>().voice));
        DM.HideDialogBox();
    }

    public void LoadActionOptions(int type)
    {
        this.GetComponent<AudioSource>().Play();
        int childs = actionGrid.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.Destroy(actionGrid.transform.GetChild(i).gameObject); //Очистить поле
        }

        TargetType target = TargetType.passive;

        switch (type) //Фильтр выбранного типа навыка
        {
            case 0:
                target = TargetType.think;
                break;
            case 1:
                target = TargetType.speak;
                break;
            case 2:
                target = TargetType.act;
                break;
        }

        int[] sudentDisciplinesLevels = activeStudent.discipLevels;
        List<Discipline> allDisciplines = FindObjectOfType<SkillController>().allDisciplines;

        foreach(Skill currentSkill in FindObjectOfType<SkillController>().defaultSkills) //Добавляет дефолтные скилы
        {
            if (currentSkill.skillType.ToString() == target.ToString())
                CreateActionButton(currentSkill);
        }

        for(int discIndex = 0; discIndex < sudentDisciplinesLevels.Length; discIndex++) //Добавляет прокачанные скилы
        {
            int discLevel = sudentDisciplinesLevels[discIndex];
            for(int i = 0; i < discLevel; i++)
            {
                Skill currentSkill = allDisciplines[discIndex].disciplineSkills[i];
                if (currentSkill.skillType.ToString() != target.ToString())
                    continue;

                CreateActionButton(currentSkill);  
            }    
        }  
    }

    public void ShowChoiceBox()
    {
        ExamChoiceBox.SetActive(true);
    }

    public void HideChoiceBox()
    {
        ExamChoiceBox.SetActive(false);
    }

    private void CreateActionButton(Skill currentSkill)
    {
        Button newButton = (Button)Instantiate(actionButton);
        Text buttText = newButton.GetComponentInChildren<Text>();
        buttText.text = currentSkill.Name;
        newButton.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(PerformAction(currentSkill)); });
        EventTrigger buttonTrigger = newButton.GetComponent<EventTrigger>();

        EventTrigger.Entry moveOn = new EventTrigger.Entry();
        moveOn.eventID = EventTriggerType.PointerEnter;
        moveOn.callback = new EventTrigger.TriggerEvent(); ;
        moveOn.callback.AddListener(delegate { CommentTB.text = currentSkill.comment; });
        buttonTrigger.triggers.Add(moveOn);

        EventTrigger.Entry moveOut = new EventTrigger.Entry();
        moveOut.eventID = EventTriggerType.PointerExit;
        moveOut.callback = new EventTrigger.TriggerEvent(); ;
        moveOut.callback.AddListener(delegate { CommentTB.text = ""; });
        buttonTrigger.triggers.Add(moveOut);

        newButton.transform.SetParent(actionGrid.transform);
    }

    private IEnumerator PerformAction(Skill skill) //Сразу запустит текстбокс. Иначе говоря, комментарий действия обязателен
    {
        this.GetComponent<AudioSource>().Play();
        
        string Comment = string.Empty;

        switch (skill.effectCode)
        {
            case 0:
                Comment = Think_Remember();
                break;
            case 1:
                Comment = Speak_NormalAnswer();
                break;
            default:
                Debug.Log("incorrect effect code");
                break;
        }

        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();

        if (Comment != string.Empty)
        {
            ExamChoiceBox.SetActive(false);
            yield return StartCoroutine(DM.ShowDialogBox());
            yield return StartCoroutine(DM.Talk(Comment));
            DM.HideDialogBox();
            ExamChoiceBox.SetActive(true);
            UpdateExamChoiceBox(activeStudent);
        }

        playerMove = false;
    }

    private string Think_Remember()
    {
        if (activeStudent.currentAnswer.AnswerProgress < 100)
        {
            float SubjectKnowing = activeStudent.AverageSubjectSkill(teacher.usedSubjects) * 10;
            SubjectKnowing = SubjectKnowing / teacher.GetComponent<CharacterData>().level;

            activeStudent.currentAnswer.AnswerProgress += SubjectKnowing;
            if (SubjectKnowing < 10)
            {
                return "Вы практически не понимаете что от вас хотят";
            }
            else if (SubjectKnowing < 40)
            {
                return "У вас явно не хватает фактических знаний в этой области";
            }
            else if (SubjectKnowing < 70)
            {
                return "Кажеться, вы кое-что понимаете, но пока этого мало";
            }
            else 
            {
                return "Вы хорошо разбираетесь в этом вопросе";
            }
        }
        else
        {
            return "У вас в голове есть все, что нужно для ответа";
        }
       
    }

    private string Speak_NormalAnswer()
    {
        float answer = activeStudent.currentAnswer.AnswerProgress + activeStudent.currentAnswer.AnswerQuality / 2;
        if (activeStudent.currentAnswer.AnswerProgress < 10 || activeStudent.currentAnswer.AnswerQuality < 10)
            answer /= 2;

        FullAnswerProgress += answer;
        activeStudent.currentAnswer = new Answer();

        if(answer == 0)
        {
            return "Вы испуганно шевелите губами. Это не очень эффективно";
        }
        else if(answer < 25)
        {
            return "Вы неуверенно проговариваете вслух пришедший на ум вариант ответа";
        }
        else if(answer < 50)
        {
            return  "Вы слегка скомканно рассказываете все, что смогли вспомнить";
        }
        else if (answer < 75)
        {
            return "Вы даете немного витиеватый, но уверенный ответ";
        }
        else
        {
            return "Вы убедительно излагаете все, что пришло вам на ум";
        }


    }
}


public class Answer
{
    public Answer()
    {
        AnswerProgress = 0;
        AnswerQuality = 0;
    }

    public Answer(float p, float q)
    {
        AnswerProgress = p;
        AnswerQuality = q;
    }

    private float answerProgress ;
    public float AnswerProgress
    {
        get
        {
            return answerProgress;
        }
        set
        {
            if (value > 100)
                value = 100;
            answerProgress = value;
        }
    }

    private float answerQuality ;
    public float AnswerQuality
    {
        get
        {
            return answerQuality;
        }
        set
        {
            if (value > 100)
                value = 100;
            answerQuality = value;
        }
    }

}