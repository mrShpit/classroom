using UnityEngine;
using System.Collections;

public class HUD_controller : MonoBehaviour
{

    private static bool ItExists;


    // Use this for initialization
    void Start ()
    {
        if (!ItExists)
        {
            ItExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
            Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
    
}
