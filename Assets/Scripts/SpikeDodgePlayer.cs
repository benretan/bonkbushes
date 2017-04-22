using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDodgePlayer : MonoBehaviour
{
    [SerializeField]
    private RectTransform PlayerTransform;

    [SerializeField]
    private float MoveSpeed;
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetMouseButton(0))
		{
            PlayerTransform.position += (Input.mousePosition - PlayerTransform.position) * MoveSpeed * Time.deltaTime;
		}
	}
}
