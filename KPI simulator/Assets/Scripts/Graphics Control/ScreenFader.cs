using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    Animator fadeAnim;
    private bool isFading = false;

    // Use this for initialization
    void Start()
    {
        fadeAnim = GetComponent<Animator>();
    }


    public IEnumerator FadeToClear()
    {
        isFading = true;
        fadeAnim.SetTrigger("FadeOut");

        while (isFading)
            yield return null;
    }

    public IEnumerator FadeToBlack()
    {
        isFading = true;
        fadeAnim.SetTrigger("FadeIn");

        while (isFading)
            yield return null;
    }

    void AnimationComplete()
    {
        isFading = false;
    }
}
