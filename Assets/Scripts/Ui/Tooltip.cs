using UnityEngine;

[System.Serializable]
public class Tooltip
{
    public string title;
    public string description;

    public Tooltip(string title, string description)
    {
        this.title = title;
        this.description = description;
    }
}
