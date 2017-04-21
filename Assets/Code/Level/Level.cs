using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is the base class for all levels
public class Level : MonoBehaviour {
    
    // The starting location for the player
    [SerializeField] private Transform PlayerStartTransform;

    // The enemy spawn points
    [SerializeField] private List<Transform> EnemySpawnTransforms;

    // Spawned characters
    private List<GameObject> SpawnedCharacters = new List<GameObject>();

    // The player spawned in the level
    public GameObject PlayerGameObject;
    public Player Player;

    // Spawn the player and set his location based on the player start
    public void SpawnPlayer()
    {
        // Create the player using the ResourceManager
        PlayerGameObject = ResourceManager.Create("Characters/Player/Player");

        // Make sure we successfully spawned (this could fail if we moved something)
        if (PlayerGameObject)
        {
            // Add to spawned characters list so we can clean up later
            SpawnedCharacters.Add(PlayerGameObject);

            // Set the player to the start position 
            if (PlayerStartTransform)
                PlayerGameObject.transform.position = PlayerStartTransform.position;

            // Store the player component in the level
            Player = PlayerGameObject.GetComponent<Player>();
        }
    }

    // Spawn the enemy and set their location to a specific spawn point
    public void SpawnEnemy(int transformIndex = 0) // set the default to be the first spawn point if one is not specified
    {
        // Make sure we have a valid transformIndex
        if (transformIndex < EnemySpawnTransforms.Count)
        {
            // Spawn in the an enemy using the ResourceManager
            GameObject enemyGameObject = ResourceManager.Create("Characters/Enemies/Archer/Enemy");

            // Make sure we successfully spawned (this could fail if we moved something)
            if (enemyGameObject)
            {
                // Add to spawned characters list so we can clean up later
                SpawnedCharacters.Add(enemyGameObject);

                // Set the enemy to the spawn transform index position
                enemyGameObject.transform.position = EnemySpawnTransforms[transformIndex].position;
            }
        }
        else
            Debug.LogWarning("Enemy spawn index outside of valid range.");

    }

    // Spawn an enemy and set their location to an random spawn point
    public void SpawnEnemyAtRandom()
    {
        // Spawn in the an enemy using the ResourceManager
        GameObject enemyGameObject = ResourceManager.Create("Characters/Enemies/Archer/Enemy");

        // Make sure we successfully spawned (this could fail if we moved something)
        if (enemyGameObject)
        { 
            // Add to spawned characters list so we can clean up later
            SpawnedCharacters.Add(enemyGameObject);

            // Get a random valid index
            int randomSpawnIndex = Random.Range(0, EnemySpawnTransforms.Count);

            // Set the enemy to the spawn transform index position
            enemyGameObject.transform.position = EnemySpawnTransforms[randomSpawnIndex].position;
        }
    }

    // Clean up all of the spawned in characters
    public void CleanUpCharacters()
    {
        // Iterates through the list of spawned characters and destroy them
        foreach (GameObject character in SpawnedCharacters)
            Destroy(character);
    }

    // Close the current level
    public void Close()
    {
        // Clean up characters
        CleanUpCharacters();

        // Destroy the game object the level component is attached to
        Destroy(this.gameObject);
    }
}
