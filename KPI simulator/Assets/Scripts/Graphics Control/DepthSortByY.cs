using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class DepthSortByY : MonoBehaviour
{
    private const int IsometricRangePerYUnit = 100;
	
	// Update is called once per frame
	void Update ()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        renderer.sortingOrder = -(int)(transform.position.y * IsometricRangePerYUnit);
	}
}
