using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExamProcess : MonoBehaviour
{
    public GameObject choiceBox;
    public Text CharacterName;
    public Text Level;
    public Text StressLevel;
    public bool ExamIsRunning;
    public GameObject chair;


    // Use this for initialization
    void Start()
    {
        choiceBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator StartExam(TeacherData teacher, List<CharacterData> allyStidents)
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

    public void ShowChoiceBox()
    {
        choiceBox.SetActive(true);
    }

    public void HideChoiceBox()
    {
        choiceBox.SetActive(false);
    }
}
