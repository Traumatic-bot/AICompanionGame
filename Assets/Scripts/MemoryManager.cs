using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MemoryManager : MonoBehaviour
{
    [System.Serializable]
    public class MemoryData
    {
        public List<string> memories =
            new List<string>();
    }

    private MemoryData memoryData =
        new MemoryData();

    private string savePath;


    void Awake()
    {
        savePath =
            Path.Combine(
                Application.persistentDataPath,
                "memories.json"
            );

        LoadMemories();
    }


    public void AddMemory(string memory)
    {
        if (string.IsNullOrWhiteSpace(memory))
            return;

        if (memoryData.memories.Contains(memory))
            return;

        memoryData.memories.Add(memory);

        SaveMemories();

        Debug.Log(
            "Memory added: " + memory
        );
    }


    public string GetMemoriesAsText()
    {
        if (memoryData.memories.Count == 0)
        {
            return "No important memories yet.";
        }

        string result = "";

        foreach (string memory in memoryData.memories)
        {
            result += "- " + memory + "\n";
        }

        return result;
    }


    private void SaveMemories()
    {
        string json =
            JsonUtility.ToJson(
                memoryData,
                true
            );

        File.WriteAllText(
            savePath,
            json
        );
    }


    private void LoadMemories()
    {
        if (!File.Exists(savePath))
            return;

        string json =
            File.ReadAllText(savePath);

        MemoryData loadedData =
            JsonUtility.FromJson<MemoryData>(
                json
            );

        if (loadedData != null)
        {
            memoryData = loadedData;
        }
    }
}