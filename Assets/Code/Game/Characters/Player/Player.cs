using UnityEngine;
using System.Collections;

public enum PlayerState
{
    PLAYER_SPAWNING,
    PLAYER_IDLE,
    PLAYER_MOVING,
    PLAYER_ATTACKING,
    PLAYER_HIT,
    PLAYER_DYING,
    PLAYER_DEAD
}

public class Player : MonoBehaviour {

    // The player's current state
    PlayerState CurrentState = PlayerState.PLAYER_IDLE;

    // This is the collider that we will turn on when the player attacks
    [SerializeField] private BoxCollider2D PlayerAttackCollider;

    // The animator component
    public Animator PlayerAnimator;

    // Player score
    public int PlayerScore = 0;

    // Update is called once per frame
    void Update() {
        // Player STATE MACHINE
        switch (CurrentState)
        {
            case PlayerState.PLAYER_IDLE:
                break;

        }
    }

    // Set the animator property to Attack
    public void PlayAttackAnimation()
    {
        // Trigger attack animation on player
        if (PlayerAnimator)
            PlayerAnimator.SetTrigger("Attack");

        PlayerScore += 100;
    }

    // Turn on the player attack collider
    public void AnimEventTurnOnPlayerAttackCollider()
    {
        // Turn on the collider component when we get an animation event
        if (PlayerAttackCollider)
            PlayerAttackCollider.enabled = true;
    }

    // Turn off the player attack collider
    public void AnimEventTurnOffPlayerAttackCollider()
    {
        // Turn off the collider component when we get an animation event
        if (PlayerAttackCollider)
            PlayerAttackCollider.enabled = false;
    }
}
