using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets;

public class PhoneController : MonoBehaviour
{
    public GameObject phoneBox;
    public bool phoneActive;
    public Image pointer;
    public bool canUse;

    public Vector3 startPointerPos;
    private Vector2 currentPointerPos;

    private Animator phoneAnim;
    private bool phoneIsMoving;

    // Use this for initialization
    void Start ()
    {
        currentPointerPos = new Vector2(0, 0);
        phoneAnim = GetComponent<Animator>();

        canUse = true;
        startPointerPos = pointer.rectTransform.position;
        phoneBox.SetActive(false);
        phoneActive = false;
	}

	// Update is called once per frame
	void Update ()
    {
        if (phoneIsMoving)
            return;

	    if(Input.GetKeyDown(KeyCode.Tab) && phoneActive == false && canUse)
        {
            StartCoroutine(TakePhone());
        }

        if(phoneActive == true)
        {
            if(Input.GetKeyDown(KeyCode.DownArrow) && currentPointerPos.y < 2)
            {
                currentPointerPos.y++;
                pointer.rectTransform.position = new Vector3(startPointerPos.x + currentPointerPos.x * 80, startPointerPos.y - currentPointerPos.y * 70);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && currentPointerPos.y > 0)
            {
                currentPointerPos.y--;
                pointer.rectTransform.position = new Vector3(startPointerPos.x + currentPointerPos.x * 80, startPointerPos.y - currentPointerPos.y * 70);
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow) && currentPointerPos.x > 0)
            {
                currentPointerPos.x --;
                pointer.rectTransform.position = new Vector3(startPointerPos.x + currentPointerPos.x * 80, startPointerPos.y - currentPointerPos.y * 70);
            }
            if(Input.GetKeyDown(KeyCode.RightArrow) && currentPointerPos.x < 1)
            {
                currentPointerPos.x ++;
                pointer.rectTransform.position = new Vector3(startPointerPos.x + currentPointerPos.x * 80, startPointerPos.y - currentPointerPos.y * 70);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                StartCoroutine(HidePhone());
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartApp(0, 0);
            }
        }
    }

    IEnumerator TakePhone()
    {
        phoneIsMoving = true;
        phoneBox.SetActive(true);

        DirectorController director = FindObjectOfType<DirectorController>();

        phoneAnim.SetTrigger("TakePhone");

        yield return StartCoroutine(WaitUntilAnimationIsDone());

        phoneActive = true;
    }

    public IEnumerator HidePhone()
    {
        phoneActive = false;
        phoneIsMoving = true;

        phoneAnim.SetTrigger("HidePhone");

        yield return StartCoroutine(WaitUntilAnimationIsDone());
        phoneBox.SetActive(false);
    }

    IEnumerator WaitUntilAnimationIsDone()
    {
        while (phoneIsMoving)
            yield return null;
    }

    public void AnimationComplete()
    {
        phoneIsMoving = false;
    }

    private void StartApp(int xPos, int yPos)
    {
        if(xPos == 0 && yPos == 0)
        {
            int[] disciplinesLevels = FindObjectOfType<PlayerController>().discLevels; // Сделать вариант для NPC
            FindObjectOfType<SkillController>().ShowSkillTree(disciplinesLevels);
        }

    }
}
