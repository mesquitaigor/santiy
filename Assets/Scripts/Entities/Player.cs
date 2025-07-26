using UnityEngine;
using UnityEngine.Animations;
public class Player : MonoBehaviour
{
    public Inventory inventory = new Inventory(12);
    [SerializeField] private Transform HelmetBoneTransform;
    
    [Header("Debug")]
    [SerializeField] private Item itemToAdd;
    private void Awake()
    {
        Inventory.OnEquipmentChanged += HandleEquipmentChanged;
    }
    private void HandleEquipmentChanged(EquipmentChangeEventArgs eventArg)
    {
        if (eventArg.IsEquipped)
        {
            GameObject equippedObject = Instantiate(eventArg.Item.itemPrefab, gameObject.transform);
            ParentConstraint parentConstraint = equippedObject.AddComponent<ParentConstraint>();
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = HelmetBoneTransform,
                weight = 1f
            };
            parentConstraint.SetSource(0, source);
        }
    }
    public void AddItemToInventoryInventory()
    {
        if (itemToAdd != null)
        {
            inventory.AddItem(itemToAdd);
        }
        else
        {
            Debug.LogError("Nenhum item foi atribuído!");
        }
    }
    public void EquipItemDebug()
    {
        if (itemToAdd != null)
        {
            inventory.EquipItem(itemToAdd);
        }
        else
        {
            Debug.LogError("Nenhum item foi atribuído!");
        }
    }
    
    public void ClearInventory()
    {
        inventory = new Inventory(12);
    }
    
    public void ShowInventoryContents()
    {
        var items = inventory.GetItems();
        Debug.Log($"Inventário contém {items.Count} itens:");
        
        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log($"  {i + 1}. {items[i].itemName}");
        }
    }
}