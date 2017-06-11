using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets;

public class EndGame : MonoBehaviour {

    public GameObject panel;
    public Text endText;
    private bool endProcudure;

	// Use this for initialization
	void Start ()
    {
        panel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Flag.FlagCheck(FindObjectOfType<DirectorController>().WorldFlags, new Flag("ExamPassed", 2)) && !endProcudure)
        {
            StartCoroutine(EndTheDemo());
            endProcudure = true;
        }
    }

    IEnumerator EndTheDemo()
    {
        panel.SetActive(true);
        GetComponents<AudioSource>()[1].Play();
        yield return new WaitForSeconds(2f);
        GetComponents<AudioSource>()[0].Play();
        endText.color = new Color(255, 255, 255, 255);
        yield return new WaitForSeconds(3f);
        Application.Quit();
    }
}
