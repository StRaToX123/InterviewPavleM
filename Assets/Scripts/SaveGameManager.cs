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
    private string m_saveGameFileFullPath;

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
        this.m_saveGameFileFullPath = Path.Combine(Application.persistentDataPath, m_fileName);
    }

    public static bool DoesSaveGameExist()
    {
        // Clear any existing save
        try
        {
            // Make sure the file exists    
            if (File.Exists(m_instance.m_saveGameFileFullPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Directory.Delete Failed to delete the SaveGame data " + " at path: " + m_instance.m_saveGameFileFullPath + "\n" + e);
        }

        return false;
    }

    public static void DeleteSaveGame()
    {
        // Clear any existing save
        try
        {
            // Make sure the file exists    
            if (File.Exists(m_instance.m_saveGameFileFullPath))
            {
                Directory.Delete(Path.GetDirectoryName(m_instance.m_saveGameFileFullPath), true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Directory.Delete Failed to delete the SaveGame data " + " at path: " + m_instance.m_saveGameFileFullPath + "\n" + e);
        }

        m_instance.m_saveGameData = null;
    }

    public static void NewGame() 
    {
        DeleteSaveGame();
        // Create a fresh new SaveGameData
        m_instance.m_saveGameData = new SaveGameData();
    }

    public static void LoadGame()
    {
        // Use Path.Combine to account for different OS's having different path separators
        m_instance.m_saveGameData = null;
        if (File.Exists(m_instance.m_saveGameFileFullPath))
        {
            try
            {
                // Load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(m_instance.m_saveGameFileFullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the data from Json back into the C# object
                m_instance.m_saveGameData = JsonUtility.FromJson<SaveGameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load file at path: " + m_instance.m_saveGameFileFullPath + "\n" + e);
            }
        }

        if (m_instance.m_saveGameData == null) 
        {
            return;
        }

        foreach (ISaveGame saveGameObjects in m_instance.m_saveGameObjects) 
        {
            saveGameObjects.LoadData(m_instance.m_saveGameData);
        }
    }

    public static void SaveGame()
    {
        if (m_instance.m_saveGameData == null)
        {
            return;
        }

        foreach (ISaveGame dataPersistenceObj in m_instance.m_saveGameObjects) 
        {
            dataPersistenceObj.SaveData(m_instance.m_saveGameData);
        }

        try
        {
            // Create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(m_instance.m_saveGameFileFullPath));
            string dataToStore = JsonUtility.ToJson(m_instance.m_saveGameData, true);
            using (FileStream stream = new FileStream(m_instance.m_saveGameFileFullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + m_instance.m_saveGameFileFullPath + "\n" + e);
        }
    }

    private void OnApplicationQuit() 
    {
        SaveGame();
    }
}
