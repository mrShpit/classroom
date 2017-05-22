using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed;
    public float timeToDestroy;
    public string text { get; set; }
    public Text displayText;
    public Color textColor;
    public bool Clone;
	

    void Start()
    {
        displayText.color = textColor;
    }

	// Update is called once per frame


	void Update ()
    {
        displayText.text = text;

        Color color = displayText.color;
        color.a -= 0.015f;
        displayText.color = color;

        transform.position = new Vector3(transform.position.x, transform.position.y + (moveSpeed * Time.deltaTime), transform.position.z);

        timeToDestroy -= Time.deltaTime;
        if (timeToDestroy <= 0)
        {
            Destroy(gameObject);
        }
    }
}
