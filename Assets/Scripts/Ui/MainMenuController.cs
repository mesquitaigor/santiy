using System.IO;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private string savePath = Path.Combine(Application.persistentDataPath, "saves");
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNewSave()
    {
        GameSave saveData = new GameSave
        {
            saveName = "New Game"
        };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(savePath, saveData.saveName + ".json"), json);
    }
}
