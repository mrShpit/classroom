using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class IsometricObject : MonoBehaviour
{
    private const int IsometricRangePerYUnit = 100;

    public Transform Target;

    public int TargetOffset = 1;
	
	// Update is called once per frame
	void Update ()
    {
        if (Target == null)
            Target = transform;
        Renderer renderer = GetComponent<Renderer>();
        renderer.sortingOrder = -(int)(Target.position.y * IsometricRangePerYUnit) + 0;
	}
}
