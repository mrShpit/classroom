using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets;

public class PhoneController : MonoBehaviour
{
    public GameObject phoneBox;
    public bool phoneActive;
    public Image pointer;
    public Vector3 position;

    private Animator phoneAnim;
    private bool phoneIsMoving;

    // Use this for initialization
    void Start ()
    {
        phoneAnim = GetComponent<Animator>();
        phoneBox.SetActive(false);
        phoneActive = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Tab) && phoneActive == false && !phoneIsMoving)
        {
            StartCoroutine(TakePhone());
        }

        if (Input.GetKeyDown(KeyCode.Tab) && phoneActive == true && !phoneIsMoving)
        {
            StartCoroutine(HidePhone());
        }

        if(phoneActive == true && !phoneIsMoving)
        {
            

            if(Input.GetKeyDown(KeyCode.UpArrow) && position.y <  70)
            {
                position.y += 70;
                pointer.transform.position = position;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && position.y > -70)
            {
                position.y -= 70;
                pointer.transform.position = position;
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow) && position.x > -40)
            {
                position.x -= 80;
                pointer.transform.position = position;
            }
            if(Input.GetKeyDown(KeyCode.RightArrow) && position.x < 40)
            {
                position.x += 80;
                pointer.transform.position = position;
            }

        }
    }

    void UpdatePointer()
    {
    }

    IEnumerator TakePhone()
    {
        UpdatePointer();
        phoneIsMoving = true;
        phoneBox.SetActive(true);

        DirectorController director = FindObjectOfType<DirectorController>();

        //foreach(Quest quest in director.activeQuests)
        //{
        //    GameObject message = Instantiate(messagePrefab);
        //    message.GetComponentInChildren<Text>().text = quest.Name;
        //    message.transform.parent = content.transform;
        //}

        phoneAnim.SetTrigger("TakePhone");

        yield return StartCoroutine(WaitUntilAnimationIsDone());

        position = pointer.transform.position;
        phoneActive = true;
    }

    IEnumerator HidePhone()
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
}
