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
    
	public void UpdateSpeed (float currentHSpeed, float currentVSpeed)
    {
        myAnim.SetFloat("XSpeed", currentHSpeed);
        myAnim.SetFloat("YSpeed", currentVSpeed);

        if(currentHSpeed == 0 && currentVSpeed == 0)
        {
            myAnim.SetBool("Moving", false);
        }
        else
        {
            myAnim.SetBool("Moving", true);
            myAnim.SetFloat("XDirection", currentHSpeed);
            myAnim.SetFloat("YDirection", currentVSpeed);
        }
	}
}
