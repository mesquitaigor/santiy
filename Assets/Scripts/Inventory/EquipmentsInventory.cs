using System;
using System.Collections.Generic;

public enum EquipmentSlotType
{
    None,
    Helmet,
    Face,
    Boots,
    Chest,
    Gloves,
    Belt,
    Leggings,
    Ring1,
    Ring2,
    Necklace,
}
public class EquipmentChangeEventArgs
{
    public Item Item { get; set; }
    public EquipmentSlotType SlotType { get; set; }
    public bool IsEquipped { get; set; } // true for equipped, false for unequipped
    
    public EquipmentChangeEventArgs(Item item, EquipmentSlotType slotType, bool isEquipped)
    {
        Item = item;
        SlotType = slotType;
        IsEquipped = isEquipped;
    }
}
public class EquipmentsInventory
{
    // Event that fires when equipment changes
    public event Action<EquipmentChangeEventArgs> OnEquipmentChanged;
    private Inventory inventory;
    // Slots de equipamento - usando InventorySlotType para suportar Ring1 e Ring2
    private Dictionary<EquipmentSlotType, Item> equippedItems = new Dictionary<EquipmentSlotType, Item>();
    public EquipmentsInventory(Inventory inventory)
    {
        this.inventory = inventory;

        // Inicializar slots de equipamento
        InitializeEquipmentSlots();
    }
    private void InitializeEquipmentSlots()
    {
        equippedItems[EquipmentSlotType.Helmet] = null;
        equippedItems[EquipmentSlotType.Chest] = null;
        equippedItems[EquipmentSlotType.Gloves] = null;
        equippedItems[EquipmentSlotType.Boots] = null;
        equippedItems[EquipmentSlotType.Ring1] = null;
        equippedItems[EquipmentSlotType.Ring2] = null;
        equippedItems[EquipmentSlotType.Face] = null;
        equippedItems[EquipmentSlotType.Belt] = null;
        equippedItems[EquipmentSlotType.Leggings] = null;
        equippedItems[EquipmentSlotType.Necklace] = null;
    }
    // Métodos para equipamentos
    public bool EquipItem(Item item)
    {
        if (item == null || !item.equipment)
            return false;
            
        // Para anéis, tenta equipar no primeiro slot vazio (Ring1 ou Ring2)
        if (item.equipmentType == EquipmentType.Ring)
        {
            if (GetEquippedItem(EquipmentSlotType.Ring1) == null)
            {
                return EquipItemInSlot(item, EquipmentSlotType.Ring1);
            }
            else if (GetEquippedItem(EquipmentSlotType.Ring2) == null)
            {
                return EquipItemInSlot(item, EquipmentSlotType.Ring2);
            }
            else
            {
                // Ambos os slots estão ocupados, substituir Ring1
                return EquipItemInSlot(item, EquipmentSlotType.Ring1);
            }
        }
        // Para outros equipamentos, usar conversão direta
        EquipmentSlotType slotType = EquipmentTypeConverter.ToInventorySlotType(item.equipmentType);
        return EquipItemInSlot(item, slotType);
    }
    
    private bool EquipItemInSlot(Item item, EquipmentSlotType slotType)
    {
        // Se já há algo equipado neste slot, tentar adicionar ao inventário
        Item currentlyEquipped = GetEquippedItem(slotType);
        if (currentlyEquipped != null)
        {
            if (!inventory.AddItem(currentlyEquipped))
            {
                return false;
            }
            // Fire unequip event for the previous item
            OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(currentlyEquipped, slotType, false));
        }
        
        // Equipar o novo item
        equippedItems[slotType] = item;
        
        // Fire equip event for the new item
        OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(item, slotType, true));
        
        return true;
    }
    
    public bool UnequipItem(EquipmentSlotType slotType, int index = -1)
    {
        Item equippedItem = GetEquippedItem(slotType);
        if (equippedItem == null)
            return false;

        if (index >= 0)
        {
            if (inventory.AddItemAtIndex(equippedItem, index))
            {
                equippedItems[slotType] = null;
                // Fire unequip event
                OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(equippedItem, slotType, false));
                return true;
            }
        }
        else
        {
            if (inventory.AddItem(equippedItem))
            {
                equippedItems[slotType] = null;
                // Fire unequip event
                OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(equippedItem, slotType, false));
                return true;
            }
        }
        
        return false; // Inventário cheio
    }
    
    public Item GetEquippedItem(EquipmentSlotType slotType)
    {
        return equippedItems.ContainsKey(slotType) ? equippedItems[slotType] : null;
    }
    
    public bool SwapWithEquipment(Item inventoryItem, EquipmentSlotType slotType)
    {
        Item equippedItem = GetEquippedItem(slotType);
        // Se o item do inventário é equipamento e pode ser equipado neste slot
        if (inventoryItem != null && inventoryItem.equipment &&
            EquipmentTypeConverter.CanEquipInSlot(inventoryItem.equipmentType, slotType))
        {
            // Fazer a troca
            equippedItems[slotType] = inventoryItem;
            if (equippedItem != null)
            {
                // Se já havia um item equipado, adicioná-lo de volta ao inventário
                if (!inventory.AddItem(equippedItem))
                {
                    return false; // Falha ao adicionar o item de volta ao inventário
                }

            }
            else
            {
                // Se não havia item equipado, apenas remover do inventário
                inventory.RemoveItem(inventoryItem);
            }
            
            // Fire events for the swap
            if (equippedItem != null)
            {
                OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(equippedItem, slotType, false));
            }
            OnEquipmentChanged?.Invoke(new EquipmentChangeEventArgs(inventoryItem, slotType, true));

            return true;
        }

        return false;
    }
}