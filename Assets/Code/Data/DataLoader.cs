using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataLoader : MonoBehaviour {

    public void Update()
    {
        // Save the player data
        if (Input.GetKeyDown(KeyCode.S))
            SaveData();

        // Load the player data
        if (Input.GetKeyDown(KeyCode.L))
            LoadData();

    }

    public static void SaveData()
    {
        // Store the data you want to save in the player data
        PlayerData playerSaveData = new PlayerData();
        playerSaveData.PlayerScore = Levels.CurrentLevel.Player.PlayerScore;

        // Save the PlayerData data structure
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/score.dat");
        bf.Serialize(file, playerSaveData);
        file.Close();

    }

    public static void LoadData()
    {
        // First check if the file exists
        if (File.Exists(Application.persistentDataPath + "/score.dat"))
        {
            // Load in the binary save data
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open((Application.persistentDataPath + "/score.dat"), FileMode.Open);
            PlayerData playerSaveData = (PlayerData)bf.Deserialize(file);
            file.Close();

            // Set the player score based on the save data
            Levels.CurrentLevel.Player.PlayerScore = playerSaveData.PlayerScore;
        }
    }
}
