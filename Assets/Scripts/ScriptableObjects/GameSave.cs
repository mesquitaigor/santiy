using UnityEngine;

[CreateAssetMenu(fileName = "GameSave", menuName = "Scriptable Objects/GameSave")]
[System.Serializable]
public class GameSave : ScriptableObject
{
    public string saveName;
}
