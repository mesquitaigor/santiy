using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha o inspetor padrão
        DrawDefaultInspector();
        
        Player player = (Player)target;
        
        // Adiciona espaço
        EditorGUILayout.Space();
        
        // Seção de debug para inventário
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add Item to Inventory"))
        {
            player.AddItemToInventoryInventory();
        }

        if (GUILayout.Button("Add equipament"))
        {
            player.EquipItemDebug();
        }
        
        if (GUILayout.Button("Clear Inventory"))
        {
            player.ClearInventory();
        }
        
        if (GUILayout.Button("Show Inventory Contents"))
        {
            player.ShowInventoryContents();
        }
    }
}
