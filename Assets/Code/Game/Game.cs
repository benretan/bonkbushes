using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
    GAME_INITIALIZING,
    GAME_WAITING,
    GAME_LOADING,
    GAME_RUNNING,
}

public class Game : MonoBehaviour {

    // Singleton pattern (used to access the single instance of the class from anywhere)
    public static Game Inst { get { return m_Inst; } }
    static Game m_Inst;

    // Set the initial game state to initializing
    private GameState CurrentState = GameState.GAME_INITIALIZING;

    // Variables used for state transitions
    [NonSerialized] public bool WantsToBeInWaitState = false;
    [NonSerialized] public bool WantsToBeInLoadingState = false;
    [NonSerialized] public bool WantsToBeInRunningState = false;

    void Awake()
    {
        // Set the instance for the singleton
        if (m_Inst == null)
            m_Inst = this;
    }

    public void Update()
    {
        // Don't update the game if the app is not running
        if (!App.Inst.GetIsRunning())
            return;

        // Main game STATE MACHINE
        switch (CurrentState)
        {
            case GameState.GAME_INITIALIZING:
                // TODO: Add all of your initialization logic here                

                // Load main menu set
                SetManager.OpenSet<MainMenuSet>((mms) => WantsToBeInWaitState = true);

                // If we want to be in the wait state, do the state transition
                if (WantsToBeInWaitState)
                    DoStateTransition(GameState.GAME_WAITING);

                break;
            case GameState.GAME_WAITING:
                // TODO: Go into this state and do nothing until the game is ready to run (for instance on the main menu or on the win/lose screens)

                // If we want to be in the loading state, do the state transition
                if (WantsToBeInLoadingState)
                    DoStateTransition(GameState.GAME_LOADING);

                break;
            case GameState.GAME_LOADING:
                // TODO: Load the level
                Levels.LoadLevel("TestLevel");
                WantsToBeInRunningState = true;

                // If we want to be in the running state, do the state transition
                if (WantsToBeInRunningState)
                    DoStateTransition(GameState.GAME_RUNNING);

                break;
            case GameState.GAME_RUNNING:
                // TODO: This is wher ethe majority of the game logic will happen and there might even be sub-states within this main game state

                // If we want to pause the game, go into the pause game state
                if (WantsToBeInWaitState)
                    DoStateTransition(GameState.GAME_WAITING);

                break;
            default:
                Debug.Assert(false, "Invalid CurrentState for GameState in Game.cs");
                break;


        }
    }

    // Use this to do state transitions in case we later need to keep track of the previous state
    private void DoStateTransition(GameState newState)
    {
        // Set the new state
        CurrentState = newState;

        // Clear all of our flags
        WantsToBeInWaitState = false;
        WantsToBeInLoadingState = false;
        WantsToBeInRunningState = false;
    }
}
