using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSet : Set {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPlayClicked()
    {
        // Set the flag to signal the game to change to the loading state
        Game.Inst.WantsToBeInLoadingState = true;

        // Close this set and open the game set
        CloseSet();
        SetManager.OpenSet<GameSet>();        
    }
}
