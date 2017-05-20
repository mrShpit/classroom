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
    public Text QuestionInfo; 
 
    private StudentData activeStudent;
    enum TargetType { think, speak, act, passive };
    private bool playerMove;
    private TeacherData teacher;

    private enum TeacherReaction { Cheat, Comment, NoComment};
    private TeacherReaction teacherReaction = TeacherReaction.Comment;

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
            QuestionInfo.text = teacher.subjectName + ". Теория " + (i + 1) +". Сложность: " + teacher.GetComponent<CharacterData>().level;
            FullAnswerProgress = 0f; // Начальное состояние общего прогресса ответа
            CurrentPatience = 100f;
            
            while (CurrentPatience > 0 && FullAnswerProgress < 100f) //Пока преподу не надоело или ответ не готов
            {
                teacherReaction = TeacherReaction.Comment; // Дефолтная реакция препода
                ExamChoiceBox.SetActive(true);
                UpdateExamChoiceBox(activeStudent);
                float lastTurnAnswerProgress = FullAnswerProgress;
                playerMove = true;
                while (playerMove)
                    yield return null; //Ход игрока. Поставит ExamChoiceBox в false

                //teacher interaction
                lastTurnAnswerProgress = FullAnswerProgress - lastTurnAnswerProgress;
                DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();

                if (teacherReaction == TeacherReaction.Comment) // Стандартный комментарий
                {
                    string[] teacherComment = GetTeacherComment(lastTurnAnswerProgress).Split('/');
                    yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        teacherComment, teacher.GetComponent<CharacterData>().voice)));
                }
                else if (teacherReaction == TeacherReaction.Cheat) // Был пойман на трюке
                {
                    string[] teacherComment = CheatReaction().Split('/');
                    yield return StartCoroutine((DM.Talk(teacher.GetComponent<CharacterData>().characterName,
                        teacherComment, teacher.GetComponent<CharacterData>().voice)));
                } // Иначе коммента не будет, а терпение препода само не измениться

                DM.HideDialogBox();
            }
            
            sumPatience += CurrentPatience;
            sumSuccess += FullAnswerProgress;
        }
        
        QuestionBar.SetActive(false);
        yield return StartCoroutine( giveResult( GetMark(sumSuccess / 2 , sumPatience / 2) ) ); // 2 - количество вопросов, сделать универсальным
        
        ExamIsRunning = false;
    }

    private string CheatReaction()
    {
        CurrentPatience -= 70;
        activeStudent.GetComponent<StudentData>().currentStress += 40;
        GetComponents<AudioSource>()[1].Play();
        System.Random r = new System.Random();
        int var = r.Next(0, 3);
        switch (var)
        {
            case 0:
                return "Эй, а это еще что такое?!/Я не разрешал этим пользоваться.";
            case 1:
                return  "Быстро спрячь это обратно!/Не помню чтобы я разрешал использовать какие-либо источники информации.";
            default:
                return "Как это понимать?/Ты думаешь что я слепой?";
        }
    }

    private string GetTeacherComment(float answerProgress)
    {
        string comment = string.Empty;
        
        if (FullAnswerProgress == 100)
            return "Отлично, этого мне достаточно.";

        CurrentPatience -= teacher.AnnoySpeed; //Время идет

        if (answerProgress == 0)
        {
            CurrentPatience -= teacher.AnnoySpeed; // дополнительный штраф

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
        else if (answerProgress < 10 && FullAnswerProgress < 25)
        {
            CurrentPatience -= ( teacher.AnnoySpeed / 2); // дополнительный штраф
            comment = "Это было.../Слабовато.";
        }
        else if (answerProgress < 10 && FullAnswerProgress < 100)
        {
            CurrentPatience -= ( teacher.AnnoySpeed / 2); // дополнительный штраф
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
        
        if (CurrentPatience == 0)
        {
            comment += "/Ладно, мне это надоело./Достаточно этой траты времени.";
        }
        else if (CurrentPatience < 20)
        {
            System.Random r = new System.Random();
            int var = r.Next(0, 3);
            switch (var)
            {
                case 0:
                    comment += "/Мое терпение ограничено.";
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

        if (scorePercent >= 90)
            return "A";
        else if (scorePercent >= 80)
            return "B";
        else if (scorePercent >= 70)
            return "C";
        else if (scorePercent >= 60)
            return "D";
        else if (scorePercent >= 50)
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
    }

    IEnumerator askQuestion()
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();

        string text = string.Empty;

        System.Random r = new System.Random();
        switch (r.Next(0, 4))
        {
            case 0:
                text = "Надеюсь, этот вопрос не будет слишком сложным./Я шучу./Мне всё-равно.";
                break;
            case 1:
                text = "Следующий вопрос я точно давал на лекции./Кажеться...";
                break;
            case 2:
                text = "Я хочу чтобы ты детально раскрыл следующий вопрос...";
                break;
            case 3:
                text = "В следующем вопросе меня особо интересуют детали.";
                break;
        }

        yield return StartCoroutine(DM.ShowDialogBox());
        yield return StartCoroutine(DM.Talk(teacher.GetComponent<CharacterData>().characterName , text.Split('/') , teacher.GetComponent<CharacterData>().voice));
        yield return StartCoroutine(DM.Talk("*Мистер Браун задает вопрос по теории. Уровень сложности: " 
            + teacher.GetComponent<CharacterData>().level));
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

    //Действия

    private IEnumerator PerformAction(Skill skill) //Сразу запустит текстбокс. Иначе говоря, комментарий действия обязателен
    {
        this.GetComponents<AudioSource>()[0].Play();
        
        string Comment = string.Empty;

        switch (skill.effectCode)
        {
            case 0:
                Comment = Think_Remember();
                break;
            case 1:
                Comment = Speak_NormalAnswer();
                break;
            case 2:
                Comment = LogicGame();
                break;
            case 3:
                Comment = DeductionGame();
                break;
            case 4:
                Comment = MobileBrowser();
                break;
            case 5:
                Comment = Conviction();
                break;
            case 6:
                Comment = CasualTalking();
                break;
            default:
                Debug.Log("incorrect effect code");
                break;
        }

        ExamChoiceBox.SetActive(false); // Скрыть перед диалогом

        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        yield return StartCoroutine(DM.Talk(Comment.Split('/')));
        playerMove = false;
        //Далее ожидаеться ответ от препода
    }

    private string Think_Remember()
    {
        if (activeStudent.currentAnswer.AnswerProgress < 100)
        {
            float SubjectKnowing = activeStudent.AverageSubjectSkill(teacher.usedSubjects) * 10; // Max = 100
            SubjectKnowing -= ( teacher.GetComponent<CharacterData>().level * 10 );

            System.Random r = new System.Random();

            activeStudent.currentAnswer.AnswerProgress += SubjectKnowing;
            if (SubjectKnowing == 0)
            {
                return "Вам нечего вспоминать./Вы понятия не имеете что от вас хотят.";
            }
            else if (SubjectKnowing < 10)
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        return "Вы едва понимаете что от вас хотят";
                    default:
                        return "Сцепив зубы, вы пытаетесь вспомнить хоть что-то.";
                }     
            }
            else if (SubjectKnowing < 40)
            {
                switch(r.Next(0, 3))
                {
                    case 0:
                        return "Для вас это не очень простой вопрос./Вы пытаетесь вспомнить что можете.";
                    case 1:
                        return "Вы не очень сильны в этой теме, но кое-что помните.";
                    default:
                        return "Сосредоточившись, вы постепенно вспоминаете отрывки лекций";
                }
            }
            else if (SubjectKnowing < 70)
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        return "Вы неплохо разбираетесь в этом вопросе.";
                    case 1:
                        return "В памяти постепенно вслпывают последние лекции.";
                }
            }
            else 
            {
                switch (r.Next(0, 2))
                {
                    case 0:
                        return "Для вас этот вопрос совсем не сложный";
                    case 1:
                        return "Вы знаете ответ и чувствуете уверенность.";
                }
            }
        }
        else
        {
            return "У вас в голове есть все, что нужно для ответа";
        }

        return "";
       
    }

    private string Speak_NormalAnswer()
    {
        float answer = activeStudent.currentAnswer.AnswerProgress;
        if (activeStudent.currentAnswer.AnswerProgress < teacher.GetComponent<CharacterData>().level * 10 )
            answer /= 2;

        FullAnswerProgress += answer;
        activeStudent.currentAnswer = new Answer();

        if(answer == 0)
        {
            return "Вы испуганно шевелите губами. Это не очень эффективно./Прежде чем пытаться ответить, стоит сначала подумать.";
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

    private string LogicGame()
    {
        activeStudent.currentAnswer.AnswerProgress += 10;
        return "Здесь будет мини-игра с головоломкой, сложность которой зависит от уровня знания предмета./" +
            "При успешном решении шкала ответа вырастет";
    }

    private string DeductionGame()
    {
        activeStudent.currentAnswer.AnswerProgress += 10;
        return "Здесь будет мини-игра с головоломкой, сложность которой зависит от уровня шкалы ответа студента./" +
            "При успешном решении загадки шкала ответа может значительно вырасти";
    }

    private string MobileBrowser()
    {
        string comment = string.Empty;
        comment = "Пока " + teacher.GetComponent<CharacterData>().characterName +
            " отвлекаеться на свои записи, вы аккуратно вытаскиваете телефон из кармана и ложите его на колено.";
        comment += "/Вы ненавязчиво опускате глаза на экран, делая вид что сосредоточенно думаете.";

        if (new System.Random().Next(0, 3) == 2)
        {
            comment += "/Стоило вам разблокировать телефон, как преподаватель резко поднял голову...";
            if (new System.Random().Next(0, 2) != 2)
            {
                comment += "/" + (teacher.GetComponent<CharacterData>().characterName + " посмотерел по сторонам и опять опустил глаза на свои бумаги");
            }
            else
            {
                teacherReaction = TeacherReaction.Cheat;
                return comment;
            }
        }

        comment += "/Вы запустили браузер./...";

        switch (new System.Random().Next(0, 3))
        {
            case 0:
                comment += "/Буквально среди первых ссылок есть полезная информация.";
                activeStudent.currentAnswer.AnswerProgress += 50;
                break;
            case 1:
                comment += "/Вам понадобилось некоторое время, но вы нашли кое-что полезное.";
                CurrentPatience -= teacher.AnnoySpeed;
                activeStudent.currentAnswer.AnswerProgress += 30;
                break;
            case 2:
                CurrentPatience -= teacher.AnnoySpeed;
                comment += "/Вам никак не удаеться найти хоть что-то пригодное для ответа.";
                break;
        }

        if (new System.Random().Next(0, 3) != 2)
        {
            comment += "/Вы спрятали телефон обратно в карман.";
        }
        else
        {
            comment += "/Вы собирались было спрятать телефон назад, но он соскользнул с колена и упал вниз с громким глухим звуком."
                + "/Вам пришлось быстро подхватить его.";
            teacherReaction = TeacherReaction.Cheat;
        }
        
        return comment;
    }

    private string Conviction()
    {
        float answer = activeStudent.currentAnswer.AnswerProgress * 1.5f; // Множитель убеждения
        if (activeStudent.currentAnswer.AnswerProgress < teacher.GetComponent<CharacterData>().level * 10)
            answer /= 2;
        if (answer < 40)
            CurrentPatience -= (teacher.AnnoySpeed / 2);
        else
            CurrentPatience -= teacher.AnnoySpeed;
        FullAnswerProgress += answer;
        activeStudent.currentAnswer = new Answer();

        if (answer == 0)
            return "Попытка захватить инициативу не имея какого-либо ответа лишь вызвала у преподователя раздражение.";
        else
            return "Глядя преподавателю в глаза, вы быстро проговорили свой ответ с уверенным тоном./" + 
                "Такая напористость слегка напрягла преподавателя, но зато все прозвучало более убедительно.";
    }

    private string CasualTalking()
    {
        activeStudent.currentStress -= 5;
        CurrentPatience += 20;
        teacherReaction = TeacherReaction.NoComment;
        return "Вы попробовали отвлечь преподавателя посторонней темой./Он охотно рассказал свое мнение по вопросу." + 
            "/Видимо, это ему интереснее чем ваши попытки сдать зачет.";
    }
}

public class Answer
{
    public Answer()
    {
        AnswerProgress = 0;
    }

    public Answer(float p)
    {
        AnswerProgress = p;
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
}