using UnityEngine;
using System.Collections.Generic;
using System;

public enum InventoryChangeType
{
    ItemAdded,
    ItemRemoved,
    ItemMoved,
    ItemSwapped
}

public class InventoryChangeEventArgs
{
    public Item Item { get; set; }
    public int Index { get; set; }
    public int FromIndex { get; set; } // For moves and swaps
    public int ToIndex { get; set; } // For moves and swaps
    public InventoryChangeType ChangeType { get; set; }
    
    public InventoryChangeEventArgs(Item item, int index, InventoryChangeType changeType)
    {
        Item = item;
        Index = index;
        ChangeType = changeType;
    }
    
    public InventoryChangeEventArgs(Item item, int fromIndex, int toIndex, InventoryChangeType changeType)
    {
        Item = item;
        FromIndex = fromIndex;
        ToIndex = toIndex;
        ChangeType = changeType;
    }
}

public class Inventory
{
    private EquipmentsInventory equipments;

    // Event that fires when inventory changes
    public static event Action<InventoryChangeEventArgs> OnInventoryChanged;
    public static event Action<EquipmentChangeEventArgs> OnEquipmentChanged;
    public int MaxSize { get; private set; }
    private Item[] items;

    public Inventory(int maxSize)
    {
        MaxSize = maxSize;
        items = new Item[maxSize];
        equipments = new EquipmentsInventory(this);
        equipments.OnEquipmentChanged += HandleEquipmentChanged;
    }
    private void HandleEquipmentChanged(EquipmentChangeEventArgs eventArgs)
    {
        OnEquipmentChanged?.Invoke(eventArgs);
    }

    public bool AddItem(Item item)
    {
        // Encontrar o primeiro slot vazio
        for (int i = 0; i < MaxSize; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(item, i, InventoryChangeType.ItemAdded));
                return true;
            }
        }

        return false;
    }

    public bool AddItemAtIndex(Item item, int index)
    {
        if (index < 0 || index >= MaxSize)
            return false;

        items[index] = item;
        OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(item, index, InventoryChangeType.ItemAdded));
        return true;
    }

    public List<Item> GetItems()
    {
        List<Item> itemList = new List<Item>();
        for (int i = 0; i < MaxSize; i++)
        {
            itemList.Add(items[i]); // Inclui null para slots vazios
        }
        return itemList;
    }

    public Item GetItemAtIndex(int index)
    {
        if (index < 0 || index >= MaxSize)
            return null;

        return items[index];
    }

    public bool RemoveItem(Item item)
    {
        for (int i = 0; i < MaxSize; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(item, i, InventoryChangeType.ItemRemoved));
                return true;
            }
        }

        return false;
    }

    public bool RemoveItemAtIndex(int index)
    {
        if (index < 0 || index >= MaxSize)
            return false;

        if (items[index] != null)
        {
            Item removedItem = items[index];
            items[index] = null;
            OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(removedItem, index, InventoryChangeType.ItemRemoved));
            return true;
        }

        return false;
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= MaxSize || indexB < 0 || indexB >= MaxSize)
            return;

        Item temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        // Fire swap event for both items
        if (temp != null)
        {
            OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(temp, indexA, indexB, InventoryChangeType.ItemSwapped));
        }
        if (items[indexA] != null)
        {
            OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(items[indexA], indexB, indexA, InventoryChangeType.ItemSwapped));
        }
    }

    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= MaxSize || toIndex < 0 || toIndex >= MaxSize)
            return false;

        Item itemToMove = items[fromIndex];
        if (itemToMove == null)
            return false;

        // Se o slot de destino está vazio, fazer movimento simples
        if (items[toIndex] == null)
        {
            items[toIndex] = itemToMove;
            items[fromIndex] = null;
            OnInventoryChanged?.Invoke(new InventoryChangeEventArgs(itemToMove, fromIndex, toIndex, InventoryChangeType.ItemMoved));
        }
        else
        {
            // Se o slot de destino tem item, fazer troca
            SwapItems(fromIndex, toIndex);
        }

        return true;
    }

    public int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < MaxSize; i++)
        {
            if (items[i] != null)
                count++;
        }
        return count;
    }

    public bool IsSlotEmpty(int index)
    {
        if (index < 0 || index >= MaxSize)
            return false;

        return items[index] == null;
    }
    public bool EquipItem(Item item)
    {
        return equipments.EquipItem(item);
    }
    public bool SwapWithEquipment(int inventoryIndex, EquipmentSlotType slotType)
    {
        if (inventoryIndex < 0 || inventoryIndex >= MaxSize)
            return false;

        Item inventoryItem = GetItemAtIndex(inventoryIndex);
        Item equippedItem = equipments.GetEquippedItem(slotType);
        bool result = equipments.SwapWithEquipment(inventoryItem, slotType);
        if (equippedItem == null)
        {
            InventoryChangeEventArgs changeEvent = new InventoryChangeEventArgs(inventoryItem, inventoryIndex, InventoryChangeType.ItemRemoved);
            OnInventoryChanged?.Invoke(changeEvent);
        }
        return result;
    }
    // Sobrecarga para compatibilidade com EquipmentType
    public bool SwapWithEquipment(int inventoryIndex, EquipmentType equipmentType)
    {
        if (equipmentType == EquipmentType.Ring)
        {
            if (SwapWithEquipment(inventoryIndex, EquipmentSlotType.Ring1))
                return true;
            return SwapWithEquipment(inventoryIndex, EquipmentSlotType.Ring2);
        }

        EquipmentSlotType slotType = EquipmentTypeConverter.ToInventorySlotType(equipmentType);
        return SwapWithEquipment(inventoryIndex, slotType);
    }
    public Item GetEquippedItem(EquipmentSlotType slotType)
    {
        return equipments.GetEquippedItem(slotType);
    }
    public bool UnequipItem(EquipmentSlotType slotType, int index = -1)
    {
        return equipments.UnequipItem(slotType, index);
    }
    // Sobrecarga para compatibilidade com EquipmentType
    public bool UnequipItem(EquipmentType equipmentType)
    {
        // Para anéis, desequipar ambos os slots
        if (equipmentType == EquipmentType.Ring)
        {
            bool success1 = UnequipItem(EquipmentSlotType.Ring1);
            bool success2 = UnequipItem(EquipmentSlotType.Ring2);
            return success1 || success2; // Sucesso se pelo menos um foi desequipado
        }
        
        EquipmentSlotType slotType = EquipmentTypeConverter.ToInventorySlotType(equipmentType);
        return UnequipItem(slotType);
    }
}
