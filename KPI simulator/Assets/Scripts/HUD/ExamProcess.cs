using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExamProcess : MonoBehaviour
{
    public GameObject ExamChoiceBox;
    public Text CharacterNameTB;
    public Text LevelTB;
    public Text StressLevelTB;
    public Text CommentTB;
    public bool ExamIsRunning;
    public GameObject chair;
    public GameObject actionGrid;
    public Button actionButton;
    public GameObject AnswerBar;

    
    public float AnswerProgress
    {
        get
        {
            return AnswerProgress;
        }
        set
        {
            if (value > 100)
                value = 100;
            value = value / 100;
            AnswerBar.GetComponentsInChildren<Image>()[2].fillAmount = value;
        }
    }
    private NPC_TeacherBehavior teacher;


    // Use this for initialization
    void Start()
    {
        ExamChoiceBox.SetActive(false);
        AnswerBar.SetActive(false);
    }

    public IEnumerator StartExam(NPC_TeacherBehavior currentTeacher, List<StudentData> allStudents)
    {
        teacher = currentTeacher;

        ExamIsRunning = true;
        FindObjectOfType<PhoneController>().canUse = false;

        PlayerController player = FindObjectOfType<PlayerController>();
        //player.canMove = false;

        ScreenFader sf = GameObject.FindGameObjectWithTag("Fader").GetComponent<ScreenFader>();
        yield return StartCoroutine(sf.FadeToBlack()); //Затемнить экран

        player.transform.position = currentTeacher.GetComponent<TeacherData>().ExamSitPlaces[0];
        GameObject clone = (GameObject)Instantiate(chair, player.transform.position, Quaternion.Euler(Vector3.zero)); //Посадить на стулья

        yield return StartCoroutine(sf.FadeToClear()); //Снова показать экран
        yield return StartCoroutine(askQuestion());
        ExamChoiceBox.SetActive(true);
        AnswerBar.SetActive(true);
        AnswerProgress = 0f;
        FillExamChoiceBox(FindObjectOfType<PlayerController>().GetComponent<StudentData>());


        //ExamIsRunning = false;
        //FindObjectOfType<PhoneController>().canUse = true;
    }

    void FillExamChoiceBox(StudentData student) // Будет перегрузка для любого студента
    {
        CharacterNameTB.text = student.GetComponent<CharacterData>().characterName;
        LevelTB.text = "Уровень " + student.GetComponent<CharacterData>().level.ToString();
        StressLevelTB.text = "Стресс: " + student.GetComponent<StudentData>().currentStress + "/" + student.GetComponent<StudentData>().maxStress;
    }

    IEnumerator askQuestion()
    {
        DialogueBoxManager DM = FindObjectOfType<DialogueBoxManager>();
        yield return StartCoroutine(DM.ShowDialogBox());
        List<string> text = new List<string>() { "Надеюсь, это не будет для тебя слишком сложно", "Я шучу", "Мне плевать" };
        yield return StartCoroutine(DM.Talk(teacher.characterName , text , teacher.voice));
        yield return StartCoroutine(DM.Talk("", "*Сенсей задает вопрос", null));
        DM.HideDialogBox();
    }

    public void LoadActionOptions(int type)
    {
        int childs = actionGrid.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.Destroy(actionGrid.transform.GetChild(i).gameObject);
        }


        Button newButton = (Button)Instantiate(actionButton);
        //Load Skills


        newButton.transform.SetParent(actionGrid.transform);
    }

    public void ShowChoiceBox()
    {
        ExamChoiceBox.SetActive(true);
    }

    public void HideChoiceBox()
    {
        ExamChoiceBox.SetActive(false);
    }
}
