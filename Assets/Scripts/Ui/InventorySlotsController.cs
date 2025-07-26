using UnityEngine;

public class InventorySlotsController : InventoryTabPanel
{
    [SerializeField] private Transform slotsParent;

    [Header("Layout Settings")]
    [SerializeField] private int maxSlotsToShow = -1;

    private void OnEnable()
    {
        Inventory.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        Inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged(InventoryChangeEventArgs eventArgs)
    {
        RefreshInventoryDisplay();
    }

    #region InventoryTabPanel Implementation
    public override void Initialize()
    {
        base.Initialize();
        if (slotsParent == null)
        {
            Debug.LogError("Slots Parent não foi atribuído no InventorySlotsController!", this);
            return;
        }
    }
    #endregion
    protected override void InitializeSlots()
    {
        Inventory playerInventory = GetPlayerInventory();
        if (playerInventory == null)
        {
            Debug.LogWarning("Inventário do player não encontrado. Slots não serão criados.", this);
            return;
        }

        int slotsToCreate = maxSlotsToShow == -1 ? playerInventory.MaxSize : Mathf.Min(maxSlotsToShow, playerInventory.MaxSize);

        for (int i = 0; i < slotsToCreate; i++)
        {
            CreateSlot(i);
        }
        onSlotsInitialized?.Invoke();
    }
    private void CreateSlot(int slotIndex)
    {
        GameObject slotObject = Instantiate(slotPrefab, slotsParent);
        slotObject.name = $"InventorySlot_{slotIndex}";

        InventorySlot inventorySlot = slotObject.GetComponent<InventorySlot>();
        if (inventorySlot == null)
        {
            Debug.LogError($"O prefab do slot não contém o componente InventorySlot! Slot {slotIndex} não funcionará corretamente.", this);
            return;
        }

        inventorySlot.Initialize(slotIndex, this);
        inventorySlots.Add(inventorySlot);
    }
    protected override void ClearExistingSlots()
    {
        inventorySlots.Clear();

        foreach (Transform child in slotsParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
    public override void RefreshInventoryDisplay()
    {
        Inventory playerInventory = GetPlayerInventory();
        if (playerInventory == null)
        {
            Debug.LogWarning("Inventário do player não encontrado para atualização.", this);
            return;
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Item itemAtIndex = playerInventory.GetItemAtIndex(i);
            inventorySlots[i].SetItem(itemAtIndex);
        }
        onInventoryRefreshed?.Invoke();
    }
}