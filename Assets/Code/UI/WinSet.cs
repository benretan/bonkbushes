using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinSet : Set {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPlayAgainClicked()
    {
        CloseSet();
        SetManager.OpenSet<MainMenuSet>();
    }
}
