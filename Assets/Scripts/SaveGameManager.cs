using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using UnityEngine.Profiling;

public class SaveGameManager : MonoBehaviour
{
    private SaveGameData m_saveGameData;
    private List<ISaveGame> m_saveGameObjects;
    private string m_fileName = "SaveGame.data";

    public static SaveGameManager m_instance { get; private set; }

    private void Awake() 
    {
        if (m_instance != null) 
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Find all the objects which implement the ISaveGame interface
        IEnumerable<ISaveGame> saveGameObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ISaveGame>();

        this.m_saveGameObjects = new List<ISaveGame>(saveGameObjects);
    }

    public void NewGame() 
    {
        // Clear any existing save
        string fullPath = Path.Combine(Application.persistentDataPath, m_fileName);
        try
        {
            // Make sure the file exists
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning("Error during deletion of SaveGame data. File was not found at path: " + fullPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Directory.Delete Failed to delete the SaveGame data " + " at path: " + fullPath + "\n" + e);
        }

        // Create a fresh new SaveGameData
        this.m_saveGameData = new SaveGameData();
    }

    public void LoadGame()
    {
        // Use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(Application.persistentDataPath, m_fileName);
        this.m_saveGameData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the data from Json back into the C# object
                this.m_saveGameData = JsonUtility.FromJson<SaveGameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load file at path: " + fullPath + "\n" + e);
            }
        }

        if (this.m_saveGameData == null) 
        {
            this.m_saveGameData = new SaveGameData();
        }

        foreach (ISaveGame saveGameObjects in m_saveGameObjects) 
        {
            saveGameObjects.LoadData(m_saveGameData);
        }
    }

    public void SaveGame()
    {
        foreach (ISaveGame dataPersistenceObj in m_saveGameObjects) 
        {
            dataPersistenceObj.SaveData(m_saveGameData);
        }

        // Use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(Application.persistentDataPath, m_fileName);
        try
        {
            // Create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonUtility.ToJson(m_saveGameData, true);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    private void OnApplicationQuit() 
    {
        SaveGame();
    }
}
