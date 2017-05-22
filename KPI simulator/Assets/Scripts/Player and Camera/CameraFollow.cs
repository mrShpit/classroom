using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour {

    //[SerializeField]
    //private float xMin; -> создаст поле в инспекторе юнити

    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;
    private float half_width;
    private float half_height;

    public Transform targetTrans;
    public NPC_Character Interlocutor;

    private static bool ItExists;
    public float speedCoef = 0.1f;

    // Use this for initialization

    void Start()
    {
        if (!ItExists)
        {
            ItExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
            Destroy(gameObject);
    }

    void LateUpdate () //Блокирование выхода камеры за границы карты
    {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, xMin + 0.25f, xMax + 0.25f),
                Mathf.Clamp(transform.position.y, yMin + 0.25f, yMax + 0.25f),
                transform.position.z);
    }

    void Update()
    {
        if (Interlocutor == null)
        {
            transform.position = Vector3.Lerp(transform.position, targetTrans.position, speedCoef) + new Vector3(0, 0, -1);
        }
        else
        {
            Vector3 dialogueFocus = new Vector3((targetTrans.position.x + Interlocutor.transform.position.x) / 2,
                (targetTrans.position.y + Interlocutor.transform.position.y) / 2, -1);
            transform.position = Vector3.Lerp(transform.position, dialogueFocus, speedCoef);
        }
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        UpdateCameraEdges();
    }

    void UpdateCameraEdges()
    {
        GameObject locationEdge = GameObject.FindGameObjectWithTag("LocationEdge");
        MeshRenderer meshRendrer = locationEdge.GetComponent<MeshRenderer>();
        Mesh mesh = locationEdge.GetComponent<MeshFilter>().mesh;
        Vector3 vertice = mesh.vertices[0];
      
        half_height = GetComponent<Camera>().orthographicSize;
        half_width = half_height * GetComponent<Camera>().aspect;

        xMin = vertice.x / 100 - meshRendrer.bounds.size.x + half_width;
        yMin = vertice.y / 100 - meshRendrer.bounds.size.y + half_height;
        xMax = vertice.x / 100 - half_width;
        yMax = vertice.y / 100 - half_height;

        if (xMin  > xMax)
        {
            xMin = (vertice.x / 50 - meshRendrer.bounds.size.x) / 2;
            xMax = xMin;
        }

        if (yMin > yMax)
        {
            yMin = (vertice.y / 50 - meshRendrer.bounds.size.y) / 2;
            yMax = yMin;
        }
    }

}