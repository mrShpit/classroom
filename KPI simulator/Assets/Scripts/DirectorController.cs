using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;

public class DirectorController : MonoBehaviour
{
    private static bool ItExists;
    public Flag[] WorldFlags;

    // Use this for initialization
    void Start ()
    {
        if (!ItExists)
        {
            ItExists = true;
            DontDestroyOnLoad(transform.gameObject);
            //Тут выполняеться загрузка при старте игры
        }
        else
        {
            Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
