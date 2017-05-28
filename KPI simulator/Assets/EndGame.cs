using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets;

public class EndGame : MonoBehaviour {

    public GameObject panel;
    public Text endText;

	// Use this for initialization
	void Start ()
    {
        panel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Flag.FlagCheck(FindObjectOfType<DirectorController>().WorldFlags, new Flag("ExamPassed", 2)))
        {
            StartCoroutine(EndTheDemo());
        }
    }

    IEnumerator EndTheDemo()
    {
        panel.SetActive(true);
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(1.5f);
        GetComponent<AudioSource>().Play();
        endText.color = new Color(255, 255, 255, 255);
        yield return new WaitForSeconds(3f);
        Application.Quit();
    }
}
