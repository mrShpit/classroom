using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    [SerializeField]
    private float xMin;
    [SerializeField]
    private float xMax;
    [SerializeField]
    private float yMin;
    [SerializeField]
    private float yMax;

    public Transform targetTrans;

    private static bool ItExists;

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

    void LateUpdate ()
    {
        transform.position = new Vector3(
            Mathf.Clamp(targetTrans.position.x, xMin, xMax),
            Mathf.Clamp(targetTrans.position.y, yMin, yMax),
            transform.position.z);
	}
	
}
