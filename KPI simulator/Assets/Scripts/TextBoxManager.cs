using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

public class TextBoxManager : MonoBehaviour {

    public GameObject textPanel;
    public bool isActive;
    public Text TextBox;
    public float TypeSpeed;
    public string[] textLines;

    public int currentLine;
    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        textPanel.SetActive(false);
    }
    


    void Update()
    {
        if (!isActive)
            return;

        //TextBox.text = textLines[currentLine];

        if(Input.GetKeyDown(KeyCode.Space) )
        {
            currentLine += 1;

            if (currentLine == textLines.Length)
            {
                DisableTextPanel();
            }
            else
            {
                StartCoroutine(TextScroll(textLines[currentLine]));
            }
        }


    }

    private IEnumerator TextScroll (string line)
    {
        AudioSource audio = GetComponent<AudioSource>();
        int letter = 0;
        TextBox.text = "";
        while (letter < line.Length - 1)
        {
            TextBox.text += line[letter];
            letter += 1;
            audio.Play();
            yield return new WaitForSeconds(TypeSpeed);
        }
        TextBox.text = line;
    }

    public void EnableTextPanel(TextAsset textFile)
    {
        textLines = textFile.text.Split('\n');
        currentLine = 0;
        textPanel.SetActive(true);
        player.canMove = false;
        isActive = true;
        StartCoroutine(TextScroll(textLines[currentLine]));
    }

    public void DisableTextPanel()
    {
        textLines = null;

        textPanel.SetActive(false);
        player.canMove = true;
        isActive = false;
    }
}
