using UnityEngine;
using System.Collections;

public class AnimatorController : MonoBehaviour
{

    public static AnimatorController instance;
    Transform myTrans;
    Animator myAnim;
    Vector3 ScaleCache;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        myTrans = this.transform;
        myAnim = this.gameObject.GetComponent<Animator>();
        ScaleCache = myTrans.localScale;
    }

    void Flip(float currentSpeed)
    {
        if ((currentSpeed < 0 && ScaleCache.x == 1) ||
            (currentSpeed > 0 && ScaleCache.x == -1))
        {
            ScaleCache.x *= -1;
            myTrans.localScale = ScaleCache;
        }
    }
    
	public void UpdateSpeed (float currentHSpeed, float currentVSpeed)
    {
        myAnim.SetFloat("XSpeed", currentHSpeed);
        myAnim.SetFloat("YSpeed", currentVSpeed);
        if (currentHSpeed != 0)
            Flip(currentHSpeed);
	}
}
