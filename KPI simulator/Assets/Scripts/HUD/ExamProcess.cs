using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Threading;
using System.IO;

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
    public Text QuestionInfo;

    public List<int[]> actionHistory;
    public int summaryFailCount;
    public int summaryQuestionsCount;
    public int averageMark;

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
            if (value < 0)
                value = 0;
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
    
    //Интерфейс

    public void LoadActionOptions(int type)
    {
        this.GetComponents<AudioSource>()[0].Play();
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

        foreach (Skill currentSkill in FindObjectOfType<SkillController>().defaultSkills) //Добавляет дефолтные скилы
        {
            if (currentSkill.skillType.ToString() == target.ToString())
                CreateActionButton(currentSkill);
        }

        for (int discIndex = 0; discIndex < sudentDisciplinesLevels.Length; discIndex++) //Добавляет прокачанные скилы
        {
            int discLevel = sudentDisciplinesLevels[discIndex];
            for (int i = 0; i < discLevel; i++)
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

    //Экзамен

    private void SaveStats()
    {
        string path = @"C:\ExamData.txt";
        StreamWriter tw = new StreamWriter(path, false);

        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
        }
        
        tw.WriteLine(summaryQuestionsCount + "." + summaryFailCount + "." + averageMark);
        string textData = string.Empty;
        foreach (int[] stat in actionHistory)
            textData += stat[0] + "." + stat[1] + "/";

        textData = textData.Substring(0, textData.Length - 1);
        tw.WriteLine(textData);
        tw.Close();
    }

    private void LoadStats()
    {
        actionHistory = new List<int[]>();
        string path = @"C:\ExamData.txt";
        if (!File.Exists(path))
        {
            averageMark = 25;
            summaryFailCount = 0;
            summaryQuestionsCount = 0;
            return;
        }

        string[] allData = File.ReadAllLines(path);

        if (allData.Length == 0)
        {
            averageMark = 25;
            summaryFailCount = 0;
            summaryQuestionsCount = 0;
            return;
        }

        summaryQuestionsCount = Convert.ToInt32(allData[0].Split('.')[0]);
        summaryFailCount = Convert.ToInt32(allData[0].Split('.')[1]);
        averageMark = Convert.ToInt32(allData[0].Split('.')[2]);

        string[] actionsLog = allData[1].Split('/');

        foreach(string actionLog in actionsLog)
        {
            int[] actionData = new int[2];
            actionData[0] = Convert.ToInt32(actionLog.Split('.')[0]);
            actionData[1] = Convert.ToInt32(actionLog.Split('.')[1]);

            actionHistory.Add(actionData);
        }
    }

    private void LogAction(int skillCode, Skill.RegressType regressType, bool fail)
    {
        if (regressType == Skill.RegressType.noRegress)
            return;

        int actionIndex = actionHistory.FindIndex(x => x[0] == skillCode);

        if (actionIndex == -1)
        {
            if ( (regressType == Skill.RegressType.onlyFail && fail) || regressType == Skill.RegressType.anyUse)
                actionHistory.Add(new int[] { skillCode, 1});
            else
                actionHistory.Add(new int[] { skillCode, 0});
        }
        else
        {
            if ( (regressType == Skill.RegressType.onlyFail && fail) || regressType == Skill.RegressType.anyUse)
                actionHistory[actionIndex][1]++;
        }
    }

    private int CheckSuccess(int effectCode, double chanceMod) // 0 - success, 1 - fail, 2 - repeated fail
    {
        float summary = 0;
        foreach (int [] stats in actionHistory)
        {
            summary += stats[1];
        }

        float regressLevel = 0;
        if(actionHistory.FindIndex(x => x[0] == effectCode) != -1)
        {
            regressLevel = actionHistory.Find(x => x[0] == effectCode)[1];
        }

        double teacherLevelPenalty = teacher.GetComponent<CharacterData>().level * 0.05;
        double failProbability = regressLevel * (summaryFailCount + 1) / ((summary + 1) * ( summaryQuestionsCount + 1 )) + teacherLevelPenalty;
        failProbability *= chanceMod;
        
        double successCheck = new System.Random().NextDouble();
        Debug.Log("fail: " + failProbability + " random: " + successCheck);

        if (successCheck >= failProbability)
            return 0;
        else
        {
            if (regressLevel == 0)
                return 1;
            else
                return 2;
        }
    }

    private string GetTeacherComment(float answerProgress) //Вызывать только если ответ на вопрос не окончен
    {
        string comment = "???";

        if (answerProgress == 0)
        {
            System.Random r = new System.Random();
            int var = r.Next(0, 6);
            switch (var)
            {
                case 0:
                    comment = "Ты долго собираешься молчать?";
                    break;
                case 1:
                    comment = "Знаешь, это же не квантовая механика./Хотя если бы я ее преподавал, то тоже был бы недоволен молчанием.";
                    break;
                case 2:
                    comment = "Я надеюсь, твой план не в том, чтобы молчать и выглядеть умным?/ Это не подходящий момент.";
                    break;
                case 3:
                    comment = "Еще пару лет назад на такие вопросы отвечали моментально.";
                    break;
                case 4:
                    comment = "Если тебе надо подумать - не спеши./Только не весь день, пожалуйста.";
                    break;
                case 5:
                    comment = "Я не подгоняю тебя.../Но если ты не знаешь, то так и скажи.";
                    break;
            }
        }
        else if (answerProgress < 10 && FullAnswerProgress < 60)
        {
            comment = "Это было.../Слабовато.";
        }
        else if (answerProgress < 10 && FullAnswerProgress < 100)
        {
            comment = "Хватит тянуть. Не так много осталось сказать.";
        }
        else if (answerProgress < 50 && FullAnswerProgress < 60)
        {
            comment = "Да-да, все верно./На самом деле не все, но сойдет и так.";
        }
        else if (answerProgress < 50 && FullAnswerProgress < 100)
        {
            comment = "Абсолютно верно./Остались лишь детали.";
        }
        else if (answerProgress < 100 && FullAnswerProgress < 100)
        {
            comment = "Отлично. Вопрос почти целиком раскрыт./Почти.";
        }
        

        if (CurrentPatience < 20)
        {
            System.Random r = new System.Random();
            int var = r.Next(0, 3);
            switch (var)
            {
                case 0:
                    comment += "/Ну?/Мое терпение ограничено.";
                    break;
                case 1:
                    comment += "/Можешь считать что мне почти надоело ждать пока ты что-то выдавишь из себя.";
                    break;
                case 2:
                    comment += "/У тебя уходит слишком много времени на такой элементарный вопрос.";
                    break;
            }
        }
        else if (answerProgress != 0)
        {
            System.Random r = new System.Random();
            int var = r.Next(0, 9);
            switch (var)
            {
                case 0:
                    comment += "/Я жду окончательного ответа.";
                    break;
                case 1:
                    comment += "/Ну же, это не так уж сложно.";
                    break;
                case 2:
                    comment += "/Я хочу услышать полный ответ.";
                    break;
                case 3:
                    comment += "/Я бы намекнул на правильный ответ, но так ведь будет неинтересно.";
                    break;
                case 4:
                    comment += "/Хм... Кстати, Я вообще рассказывал эту тему на лекциях?";
                    break;
                case 5:
                    comment += "/Отвечай, пока мне не стало скучно.";
                    break;
                case 6:
                    comment += "/Я жду.";
                    break;
                case 7:
                    comment += "/Я все еще жду.";
                    break;
                case 8:
                    comment += "/К слову, по этой теме у вас была практика.";
                    break;
            }
        }

        return comment;
    }

    private string GetMark(float avSuccessPercent, float avAnnoyPersent)
    {
        float scorePercent = (avSuccessPercent + avAnnoyPersent) / 2;
        Debug.Log(scorePercent);

        if (scorePercent >= 85)
            return "A";
        else if (scorePercent >= 70)
            return "B";
        else if (scorePercent >= 55)
            return "C";
        else if (scorePercent >= 40)
            return "D";
        else if (scorePercent >= 25)
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
        StudentAnswerProgressTB.text = "Ответ: " + student.СurrentAnswer + "%";
    }

    IEnumerator askQuestion()
    {
        int difficulty = averageMark / 10;
        if (difficulty > teacher.GetComponent<CharacterData>().level * 2)
            difficulty = teacher.GetComponent<CharacterData>().level * 2;
        
        teacher.questionDifficulty = difficulty;

        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();

        string text = string.Empty;

        System.Random r = new System.Random();
        switch (r.Next(0, 4))
        {
            case 0:
                text = "Надеюсь, этот вопрос не будет слишком сложным./Я шучу./Мне всё-равно.";
                break;
            case 1:
                text = "Этот вопрос может быть не простым.";
                break;
            case 2:
                text = "Я хочу чтобы ты детально раскрыл вопрос, который я сейчас задам.";
                break;
            case 3:
                text = "В этой теме меня особенно интересуют детали.";
                break;
        }

        yield return StartCoroutine(DM.ShowDialogBox());
        yield return StartCoroutine(DM.Talk(teacher.GetComponent<CharacterData>().characterName , text.Split('/') , teacher.GetComponent<CharacterData>().voice));
        yield return StartCoroutine(DM.Talk("*Мистер Браун задает вопрос по теории. Уровень сложности: " 
            + teacher.questionDifficulty));
        DM.HideDialogBox();
    } //Тут будут разные виды вопросов

    IEnumerator giveResult(string mark)
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        List<string> text = new List<string>() { "Зачет окончен.", "Твой результат: " + mark , "Можешь быть свободен."};
        yield return StartCoroutine(DM.Talk(teacher.GetComponent<CharacterData>().characterName, text, teacher.GetComponent<CharacterData>().voice));
        DM.HideDialogBox();
    }

    private void ChangePatience(float change, string comment)
    {
        CurrentPatience += change;
        if (change > 0)
            teacher.GetComponent<NPC_Character>().FloatingTextReaction(comment, true);
        else if (change <0)
            teacher.GetComponent<NPC_Character>().FloatingTextReaction(comment, false);
    }

    public IEnumerator StartExam(TeacherData currentTeacher, List<StudentData> allStudents)
    {
        if (allStudents.Count == 0)
        {
            Debug.Log("Must be at least one student");
            yield break;
        }

        this.GetComponents<AudioSource>()[2].Play();
        this.GetComponents<AudioSource>()[2].volume = 0.2f;

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

        activeStudent = allStudents[0]; //Тут повтор кода, исправить потом  

        float sumPatience = 0f;
        float sumSuccess = 0f;
        LoadStats();

        for (int i = 0; i < 2; i++) // Количество вопросов
        {
            yield return StartCoroutine(askQuestion()); //Задать вопрос                                                   

            QuestionBar.SetActive(true);
            QuestionInfo.text = teacher.subjectName + ". Теория " + (i + 1) + ". Сложность: " + teacher.questionDifficulty;
            FullAnswerProgress = 0f; // Начальное состояние общего прогресса ответа
            CurrentPatience = 100f;
            for (int s = 0; s < allStudents.Count; s++)
                allStudents[s].СurrentAnswer = 0;

            DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();

            while (CurrentPatience > 0 && FullAnswerProgress < 100f) //Пока преподу не надоело или ответ не готов
            {
                ExamChoiceBox.SetActive(true);
                GameObject myEventSystem = GameObject.Find("EventSystem");
                myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
                UpdateExamChoiceBox(activeStudent);

                float lastTurnAnswerProgress = FullAnswerProgress;
                playerMove = true;
                while (playerMove)
                    yield return null; //Ход игрока. Поставит ExamChoiceBox в false

                if (CurrentPatience != 0 && FullAnswerProgress < 100f)
                    DM.HideDialogBox(); // До сюда
            }

            if (FullAnswerProgress < 100f)
            {
                string comment = "Ладно, это лишь трата времени./Достаточно.";
                activeStudent.currentStress += 20;
                yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                       comment.Split('/'), teacher.GetComponent<CharacterData>().voice)));
                teacher.GetComponent<NPC_Character>().FloatingTextReaction("Провал вопроса", false);
            }
            else
            {
                string comment = "Этого мне достаточно.";
                yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        comment.Split('/'), teacher.GetComponent<CharacterData>().voice)));
                teacher.GetComponent<NPC_Character>().FloatingTextReaction("Вопрос отвечен", true);
            }

            

            //Take all actionData from list and add line to file
            summaryQuestionsCount++;
            sumPatience += CurrentPatience;
            sumSuccess += FullAnswerProgress;
            int newAverageMark = (int)(averageMark + (CurrentPatience + FullAnswerProgress) / 2) / 2;

            if(newAverageMark + 20 < averageMark)
            {
                string comment = "Должен признать, я в тебе разочарован.";
                yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        comment.Split('/'), teacher.GetComponent<CharacterData>().voice)));
            }
            else if (newAverageMark > averageMark + 20)
            {
                string comment = "Отличный ответ./Думаю, мне стоит задавать тебе вопросы посложнее.";
                yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        comment.Split('/'), teacher.GetComponent<CharacterData>().voice)));
            }

            DM.HideDialogBox();
            averageMark = newAverageMark;
        }

        this.GetComponents<AudioSource>()[2].Stop();
        QuestionBar.SetActive(false);
        
        yield return StartCoroutine(giveResult(GetMark(sumSuccess / 2, sumPatience / 2))); // 2 - количество вопросов, сделать универсальным

        SaveStats();
        ExamIsRunning = false;
    }

    //Действия

    private IEnumerator PerformAction(Skill skill)
    {
        this.GetComponents<AudioSource>()[0].Play();
        
        ActionData actionData = new ActionData();

        switch (skill.effectCode)
        {
            case 0:
                actionData = Think_Remember();
                break;
            case 1:
                actionData = Speak_NormalAnswer();
                break;
            case 2:
                actionData = LogicGame();
                break;
            //case 3:
            //    actionData = DeductionGame();
            //    break;
            case 4:
                actionData = MobileBrowser(skill.effectCode);
                break;
            case 5:
                actionData = Conviction(skill.effectCode);
                break;
            case 6:
                actionData = CasualTalking(skill.effectCode);
                break;
            default:
                Debug.Log("incorrect effect code");
                break;
        }

        ExamChoiceBox.SetActive(false); // Скрыть перед диалогом

        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        yield return StartCoroutine(DM.Talk(actionData.actionText.Split('/')));
        activeStudent.СurrentAnswer += (int)actionData.studentAnswerBonus;
        FullAnswerProgress += actionData.fullAnswerBonus;

        if (actionData.fail)
        {
            GetComponents<AudioSource>()[1].Play();
            summaryFailCount++;
        }

        activeStudent.currentStress += actionData.stressChange;

        if (actionData.patienceChange != 0)
            ChangePatience(actionData.patienceChange, actionData.patienceChangeComment);


        if(actionData.teacherReactionText == string.Empty && FullAnswerProgress < 100) //Если текст не был определен, вызываеться коммент по умолчанию
            yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        GetTeacherComment(actionData.fullAnswerBonus).Split('/'), teacher.GetComponent<CharacterData>().voice)));
        else if (actionData.teacherReactionText != string.Empty) //Если есть текст, он выдаеться
            yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                           actionData.teacherReactionText.Split('/'), teacher.GetComponent<CharacterData>().voice)));

        if (!actionData.fail && FullAnswerProgress < 100 && CurrentPatience > 0)
            ChangePatience(-teacher.AnnoySpeed, "Время идет");

        //saveActionData to List

        LogAction(skill.effectCode, skill.regressType, actionData.fail);
        
        playerMove = false;
    }

    private ActionData Think_Remember()
    {
        ActionData dataToReturn = new ActionData();
        dataToReturn.patienceChange = -teacher.AnnoySpeed;
        dataToReturn.patienceChangeComment = "Молчание";

        if (activeStudent.СurrentAnswer < 100)
        {
            float SubjectKnowing = activeStudent.AverageSubjectSkill(teacher.usedSubjects); // Max = 10
            SubjectKnowing = SubjectKnowing / teacher.questionDifficulty;
            dataToReturn.studentAnswerBonus = SubjectKnowing * 50;
            Debug.Log(dataToReturn.studentAnswerBonus);
            
            System.Random r = new System.Random();

            if (dataToReturn.studentAnswerBonus == 0)
            {
               dataToReturn.actionText =  "Вам нечего вспоминать./Вы понятия не имеете что от вас хотят.";
            }
            else if (dataToReturn.studentAnswerBonus < 10)
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        dataToReturn.actionText = "Вы едва понимаете что от вас хотят";
                        break;
                    default:
                        dataToReturn.actionText = "Сцепив зубы, вы пытаетесь вспомнить хоть что-то.";
                        break;
                }     
            }
            else if (dataToReturn.studentAnswerBonus < 40)
            {
                switch(r.Next(0, 3))
                {
                    case 0:
                        dataToReturn.actionText = "Для вас это не очень простой вопрос./Вы пытаетесь вспомнить что можете.";
                        break;
                    case 1:
                        dataToReturn.actionText = "Вы не очень сильны в этой теме, но кое-что помните.";
                        break;
                    default:
                        dataToReturn.actionText = "Сосредоточившись, вы постепенно вспоминаете отрывки лекций";
                        break;
                }
            }
            else if (dataToReturn.studentAnswerBonus < 70)
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        dataToReturn.actionText = "Вы неплохо разбираетесь в этом вопросе.";
                        break;
                    case 1:
                        dataToReturn.actionText = "В памяти постепенно вслпывают последние лекции.";
                        break;
                }
            }
            else 
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        dataToReturn.actionText = "Для вас этот вопрос совсем не сложный";
                        break;
                    case 1:
                        dataToReturn.actionText = "Вы знаете ответ и чувствуете уверенность.";
                        break;
                }
            }
        }
        else
        {
            dataToReturn.actionText = "У вас в голове есть все, что нужно для ответа";
        }

        return dataToReturn;
    }

    private ActionData Speak_NormalAnswer()
    {
        ActionData dataToReturn = new ActionData();
        float answer = activeStudent.СurrentAnswer;
        if(activeStudent.currentStress > 50)
            answer = answer * (1.5f - activeStudent.currentStress / 100);
        dataToReturn.fullAnswerBonus = answer;
        activeStudent.СurrentAnswer = 0;

        if(answer == 0)
        {
            if (activeStudent.currentStress == 0)
                dataToReturn.actionText = "Вы настолько испуганы, что с трудом издаете звуки./Преподавателя это раздражает.";
            else
                dataToReturn.actionText = "Вы задумчиво шевелите губами. Это не очень эффективно./Прежде чем пытаться ответить, стоит сначала подумать.";
        }
        else if(answer < 25)
        {
            dataToReturn.actionText = "Вы неуверенно проговариваете вслух пришедший на ум вариант ответа.";
        }
        else if(answer < 50)
        {
            dataToReturn.actionText = "Вы слегка скомканно рассказываете все, что смогли вспомнить.";
        }
        else if (answer < 75)
        {
            dataToReturn.actionText = "Вы даете немного витиеватый, но уверенный ответ.";
        }
        else
        {
            dataToReturn.actionText = "Вы убедительно излагаете все, что пришло вам на ум.";
        }

        if(activeStudent.currentStress > 50)
        {
            dataToReturn.actionText += "/Сильный стресс мешает вам свободно говорить.";
        }

        return dataToReturn;
    }

    private ActionData LogicGame()
    {
        ActionData dataToReturn = new ActionData();
        dataToReturn.patienceChange = -teacher.AnnoySpeed;
        dataToReturn.patienceChangeComment = "Молчание";
        dataToReturn.studentAnswerBonus = 10;
        dataToReturn.actionText = "Здесь будет мини-пазл, решение которого повышает очки готовности";

        return dataToReturn;
    }

    private ActionData MobileBrowser(int skillCode)
    {
        ActionData dataToReturn = new ActionData();
        string comment = string.Empty;

        comment = "Пока " + teacher.GetComponent<CharacterData>().characterName +
            " отвлекаеться на свои записи, вы аккуратно вытаскиваете телефон из кармана и ложите его на колено.";
        comment += "/Вы ненавязчиво опускате глаза на экран, делая вид что сосредоточенно думаете.";

        int result = CheckSuccess(skillCode , 1);

        if (result == 1 || result == 2)
        {
            dataToReturn.fail = true;
            dataToReturn.actionText = comment + "/Внезапно вы осознаете что преподаватель смотрит на вас.";
            dataToReturn.patienceChange = -50;
            dataToReturn.patienceChangeComment = "Пойман на мухлеже";
            dataToReturn.stressChange = 30;

            if (result == 1)
                dataToReturn.teacherReactionText = "И как это понимать?/Я не разрешал пользоваться техникой.";
            else if (result == 2)
                dataToReturn.teacherReactionText = "Ты что, опять за свое?/Я уже говорил тебе что не разрешаю пользоваться телефоном.";
            
            return dataToReturn;
        }

        comment += "/Вы запустили браузер./...";

        int r = new System.Random().Next(0, 3);
        switch (r % 3)
        {
            case 0:
                comment += "/Буквально среди первых ссылок есть полезная информация.";
                dataToReturn.studentAnswerBonus += 50;
                break;
            case 1:
                comment += "/Вам понадобилось некоторое время, но вы нашли кое-что полезное.";
                dataToReturn.studentAnswerBonus += 30;
                break;
            case 2:
                comment += "/Вам никак не удаеться найти хоть что-то пригодное для ответа.";
                break;
        }
        
        dataToReturn.actionText = comment;

        return dataToReturn;
    }

    private ActionData Conviction(int skillCode)
    {
        ActionData dataToReturn = new ActionData();
        float answer = activeStudent.СurrentAnswer * 1.5f; // Множитель убеждения

        dataToReturn.patienceChange = -teacher.AnnoySpeed;
        dataToReturn.patienceChangeComment = "Наглость";

        if(CheckSuccess(skillCode, 0.5) == 2)
        {
            dataToReturn.fail = true;
            dataToReturn.teacherReactionText = "Нету необходимости устраивать тут спектакль./Говори нормально.";
        }

        dataToReturn.fullAnswerBonus = answer;
        
        if (answer == 0)
            dataToReturn.actionText = "Попытка захватить инициативу не имея какого-либо ответа лишь вызвала у преподователя раздражение.";
        else
            dataToReturn.actionText = "Глядя преподавателю в глаза, вы быстро проговорили свой ответ с уверенным тоном./" + 
                "Такая напористость слегка напрягла преподавателя, но зато все прозвучало более убедительно.";

        activeStudent.СurrentAnswer = 0;

        return dataToReturn;
    }

    private ActionData CasualTalking(int skillCode)
    {
        ActionData dataToReturn = new ActionData();
        dataToReturn.patienceChange = 20;
        dataToReturn.patienceChangeComment = "Отвлеченность";
        dataToReturn.stressChange = -5;
        dataToReturn.actionText = "Вы попробовали отвлечь преподавателя посторонней темой./Он охотно рассказал свое мнение по вопросу." +
            "/Видимо, это ему интереснее чем ваши попытки сдать зачет.";

        return dataToReturn;
    }
}

public class ActionData
{
    public ActionData()
    {
        studentAnswerBonus = 0;
        fullAnswerBonus = 0;
        patienceChange = 0;
        patienceChangeComment = string.Empty;
        actionText = string.Empty;
        teacherReactionText = string.Empty;
        stressChange = 0;
        fail = false;
    }
    
    public float studentAnswerBonus;
    public float fullAnswerBonus;
    public float patienceChange;
    public string patienceChangeComment;
    public string actionText;
    public string teacherReactionText;
    public float stressChange;
    public bool fail;
}

