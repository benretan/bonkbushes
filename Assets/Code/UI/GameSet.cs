using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSet : Set {

    // DEBUG: These are just for testing menu flow
    public void OnWinGameClicked()
    {
        Game.Inst.WantsToBeInWaitState = true;
        Levels.CloseLevel();

        CloseSet();
        SetManager.OpenSet<WinSet>();
    }

    // DEBUG: These are just for testing menu flow
    public void OnLoseGameClicked()
    {
        Game.Inst.WantsToBeInWaitState = true;
        Levels.CloseLevel();

        CloseSet();
        SetManager.OpenSet<LoseSet>();
    }

    // What do we do when we click anywhere on the screen aside from the buttons
    public void OnScreenClicked()
    {
        // Get the player instance and play the attack anim
        //Player player = Levels.CurrentLevel.Player;

        //if(player)
        //    player.PlayAttackAnimation();
    }
}
