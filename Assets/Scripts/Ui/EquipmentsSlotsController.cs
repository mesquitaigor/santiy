using System.Collections.Generic;
using UnityEngine;
public enum SlotSide
{
    Left,
    Right,
    Top,
    Buttom
}
[System.Serializable]
public class ArmorSlotConfig
{
    [Header("Configuração do Slot")]
    public EquipmentSlotType slotType;
    public Sprite slotIcon;
    public SlotSide side = SlotSide.Top;
    public EquipmentSlot equipmentSlot;
    
    // Propriedade para compatibilidade com código existente
    public EquipmentType equipmentType => EquipmentTypeConverter.ToEquipmentType(slotType);
}
public class EquipmentsSlotsController : InventoryTabPanel
{
    [Header("Painéis do Inventário")]
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private GameObject rightPanel;
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject bottomPanel;

    [Header("Configuração dos Slots de Armadura")]
    [SerializeField] private List<ArmorSlotConfig> armorSlots = new List<ArmorSlotConfig>();

    public override void RefreshInventoryDisplay()
    {
        Inventory inventory = GetPlayerInventory();
        foreach (var slot in armorSlots)
        {
            Item item = inventory.GetEquippedItem(slot.slotType);
            slot.equipmentSlot.SetItem(item);
        }
    }

    protected override void InitializeSlots()
    {
        for (int i = 0; i < armorSlots.Count; i++)
        {
            ArmorSlotConfig slot = armorSlots[i];
            GameObject targetPanel = GetPanelBySide(slot.side);
            if (slotPrefab != null && targetPanel != null)
            {
                GameObject slotInstance = Instantiate(slotPrefab, targetPanel.transform);
                ConfigureSlotInstance(i, slotInstance, slot);
            }
            else
            {
                Debug.LogWarning($"SlotPrefab ou targetPanel não configurado para o slot {slot.slotType}");
            }
        }
    }
    protected override void ClearExistingSlots()
    {
        inventorySlots.Clear();
        ClearPanel(leftPanel);
        ClearPanel(rightPanel);
        ClearPanel(topPanel);
        ClearPanel(bottomPanel);
    }
    private void ClearPanel(GameObject panel)
    {
        if (panel != null)
        {
            for (int i = panel.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(panel.transform.GetChild(i).gameObject);
            }
        }
    }
    private void ConfigureSlotInstance(int index, GameObject slotInstance, ArmorSlotConfig config)
    {
        slotInstance.name = $"Slot_{config.slotType}";
        EquipmentSlot equipmentSlot = slotInstance.GetComponent<EquipmentSlot>();
        if (equipmentSlot != null)
        {
            equipmentSlot.RestrictedTo(config.slotType);
            equipmentSlot.InitializeSlot(index, config.slotIcon, this);
            inventorySlots.Add(equipmentSlot);
            config.equipmentSlot = equipmentSlot;
        }
    }
    
    private GameObject GetPanelBySide(SlotSide side)
    {
        return side switch
        {
            SlotSide.Left => leftPanel,
            SlotSide.Right => rightPanel,
            SlotSide.Top => topPanel,
            SlotSide.Buttom => bottomPanel,
            _ => topPanel
        };
    }

    private void OnEnable()
    {
        Inventory.OnEquipmentChanged += OnEquipmentChanged;
    }

    private void OnDisable()
    {
        Inventory.OnEquipmentChanged -= OnEquipmentChanged;
    }

    private void OnEquipmentChanged(EquipmentChangeEventArgs eventArgs)
    {
        RefreshInventoryDisplay();
    }
}