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

    void OnTriggerStay2D(Collider2D otherObject)
    {
        if (otherObject.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.E))
        {
            FindObjectOfType<PlayerController>().StartPoint = destinationIndex;
            SceneManager.LoadScene(destinationLevelName); 
        }
    }
}
