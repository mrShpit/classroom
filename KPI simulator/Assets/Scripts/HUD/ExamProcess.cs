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
    public bool ExamIsRunning;
    public GameObject chair;


    // Use this for initialization
    void Start()
    {
        ExamChoiceBox.SetActive(false);
    }

    public IEnumerator StartExam(TeacherData teacher, List<NPC_CharacterData> allyStidents)
    {
        ExamIsRunning = true;

        PlayerController player = FindObjectOfType<PlayerController>();
        player.canMove = false;

        ScreenFader sf = GameObject.FindGameObjectWithTag("Fader").GetComponent<ScreenFader>();
        yield return StartCoroutine(sf.FadeToBlack()); //Затемнить экран

        player.transform.position = teacher.ExamSitPlaces[0];
        GameObject clone = (GameObject)Instantiate(chair, player.transform.position, Quaternion.Euler(Vector3.zero)); //Посадить на стулья

        yield return StartCoroutine(sf.FadeToClear()); //Снова показать экран




        ExamIsRunning = false;
    }

    void FillExamChoiceBox(PlayerController player) // Будет перегрузка для любого студента
    {
        CharacterNameTB.text = player.characterName;
        LevelTB.text = player.level.ToString();
        StressLevelTB.text = player.currentStress + "/" + player.maxStress;

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
