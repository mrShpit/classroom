using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransferPointController : MonoBehaviour
{

    public string destinationLevelName;
    public int Index;
    public int destinationIndex;

    // Use this for initialization
    void Start()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (this.Index == player.StartPoint)
        {
            player.transform.position = this.transform.position;
        }
    }

    IEnumerator OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E))
        {
            CameraFollow camera = FindObjectOfType<CameraFollow>();
            camera.speedCoef = 1.0f;

            ScreenFader sf = GameObject.FindGameObjectWithTag("Fader").GetComponent<ScreenFader>();

            yield return StartCoroutine(sf.FadeToBlack());

            FindObjectOfType<PlayerController>().StartPoint = destinationIndex; // Указать точку входа в локацию
            SceneManager.LoadScene(destinationLevelName); // Загрузить локацию

            yield return StartCoroutine(sf.FadeToClear());
            camera.speedCoef = 0.1f;
        }
    }
}
