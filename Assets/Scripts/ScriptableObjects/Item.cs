using UnityEngine;

public enum EquipmentType
{
    None,
    Helmet,
    Face,
    Boots,
    Chest,
    Gloves,
    Belt,
    Leggings,
    Ring,
    Necklace,
}

[CreateAssetMenu(fileName = "Items", menuName = "Scriptable Objects/Items")]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public bool equipment;
    public EquipmentType equipmentType;
    public GameObject itemPrefab;
}

public static class EquipmentTypeConverter
{
    /// <summary>
    /// Converte EquipmentType para InventorySlotType
    /// Para anéis, retorna Ring1 por padrão
    /// </summary>
    public static EquipmentSlotType ToInventorySlotType(EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Ring:
                return EquipmentSlotType.Ring1; // Por padrão, anéis vão para Ring1
            case EquipmentType.None:
                return EquipmentSlotType.None;
            case EquipmentType.Helmet:
                return EquipmentSlotType.Helmet;
            case EquipmentType.Face:
                return EquipmentSlotType.Face;
            case EquipmentType.Boots:
                return EquipmentSlotType.Boots;
            case EquipmentType.Chest:
                return EquipmentSlotType.Chest;
            case EquipmentType.Gloves:
                return EquipmentSlotType.Gloves;
            case EquipmentType.Belt:
                return EquipmentSlotType.Belt;
            case EquipmentType.Leggings:
                return EquipmentSlotType.Leggings;
            case EquipmentType.Necklace:
                return EquipmentSlotType.Necklace;
            default:
                return EquipmentSlotType.None;
        }
    }

    /// <summary>
    /// Converte InventorySlotType para EquipmentType
    /// Ring1 e Ring2 se tornam Ring
    /// </summary>
    public static EquipmentType ToEquipmentType(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Ring1:
            case EquipmentSlotType.Ring2:
                return EquipmentType.Ring;
            case EquipmentSlotType.None:
                return EquipmentType.None;
            case EquipmentSlotType.Helmet:
                return EquipmentType.Helmet;
            case EquipmentSlotType.Face:
                return EquipmentType.Face;
            case EquipmentSlotType.Boots:
                return EquipmentType.Boots;
            case EquipmentSlotType.Chest:
                return EquipmentType.Chest;
            case EquipmentSlotType.Gloves:
                return EquipmentType.Gloves;
            case EquipmentSlotType.Belt:
                return EquipmentType.Belt;
            case EquipmentSlotType.Leggings:
                return EquipmentType.Leggings;
            case EquipmentSlotType.Necklace:
                return EquipmentType.Necklace;
            default:
                return EquipmentType.None;
        }
    }

    /// <summary>
    /// Verifica se um item pode ser equipado em um slot específico
    /// </summary>
    public static bool CanEquipInSlot(EquipmentType itemType, EquipmentSlotType slotType)
    {
        if (itemType == EquipmentType.Ring)
        {
            return slotType == EquipmentSlotType.Ring1 || slotType == EquipmentSlotType.Ring2;
        }
        
        return ToInventorySlotType(itemType) == slotType;
    }
}
