using UnityEngine;
using System.Collections;

public class PlayerTriggerController : MonoBehaviour {

    // Use this for initialization


    private BoxCollider2D trigger;

    void Start ()
    {
        trigger = this.GetComponent<BoxCollider2D>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void VerticalMovement(float side)
    {
        trigger.size = new Vector2(0.5f, 0.3f);

        if (side == 1)
        {
            trigger.offset = new Vector2(0, 0.25f);
        }
        else if (side == -1)
        {
            trigger.offset = new Vector2(0, -0.15f);
        }
    }

    public void HorizontalMovement(float side)
    {
        trigger.size = new Vector2(0.3f, 0.2f);

        if (side == 1)
        {
            trigger.offset = new Vector2(0.3f, 0.1f);
        }
        else if (side == -1)
        {
            trigger.offset = new Vector2(-0.3f, 0.1f);
        }
    }

    
}
