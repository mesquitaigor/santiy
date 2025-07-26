using UnityEngine;
using UnityEngine.EventSystems;

public enum DropOperation
{
    EquipItem,      // Equipar item do inventário em slot de equipamento
    UnequipItem,    // Desequipar item para o inventário
    SwapEquipment,  // Trocar equipamentos entre slots
    MoveInInventory // Mover/trocar itens dentro do inventário
}

public class DropHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Drop Visual Feedback")]
    [SerializeField] private GameObject dropHighlight;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    private InventorySlot inventorySlot;
    private UnityEngine.UI.Image backgroundImage;
    private Color originalColor;
    private bool hasCustomHighlight;
    
    private void Awake()
    {
        inventorySlot = GetComponent<InventorySlot>();
        
        // Configurar highlight visual
        if (dropHighlight != null)
        {
            hasCustomHighlight = true;
            dropHighlight.SetActive(false);
        }
        else
        {
            // Usar mudança de cor no background como fallback
            backgroundImage = GetComponent<UnityEngine.UI.Image>();
            if (backgroundImage != null)
            {
                originalColor = backgroundImage.color;
                hasCustomHighlight = false;
            }
        }
    }
    
    public void HandleDrop()
    {
        // Validações iniciais
        if (!IsValidDrop())
        {
            HideDropHighlight();
            return;
        }
        
        // Determinar o tipo de operação baseado nos slots de origem e destino
        DropOperation operation = DetermineDropOperation();
        Debug.Log($"Operação de drop determinada: {operation}");
        // Executar a operação apropriada
        switch (operation)
        {
            case DropOperation.EquipItem:
                HandleEquipItem();
                break;
            case DropOperation.UnequipItem:
                HandleUnequipItem();
                break;
            case DropOperation.SwapEquipment:
                HandleSwapEquipment();
                break;
            case DropOperation.MoveInInventory:
                HandleInventoryMove();
                break;
            default:
                Debug.LogWarning($"Operação de drop não suportada: {operation}");
                break;
        }
        
        HideDropHighlight();
    }
    
    // ===== MÉTODOS DE VALIDAÇÃO =====
    
    private bool IsValidDrop()
    {
        // Verificar se há um item sendo arrastado
        if (DragHandler.CurrentDraggedItem == null || DragHandler.DraggedItem == null)
        {
            Debug.LogWarning("Tentativa de drop sem item sendo arrastado");
            return false;
        }
        
        // Não permitir drop no mesmo slot
        if (IsSameSlot())
        {
            Debug.Log("Tentativa de drop no mesmo slot");
            return false;
        }
        
        // Verificar se este slot pode aceitar o item
        if (!inventorySlot.CanAcceptItem(DragHandler.DraggedItem))
        {
            Debug.Log("Slot não pode aceitar este item");
            return false;
        }
        
        return true;
    }
    
    private bool IsSameSlot()
    {
        return DragHandler.DraggedSlot.SlotIndex == inventorySlot.SlotIndex && 
               DragHandler.DraggedSlot.RelatedPanel.GetType() == inventorySlot.RelatedPanel.GetType();
    }
    
    // ===== DETERMINAÇÃO DA OPERAÇÃO =====
    
    private DropOperation DetermineDropOperation()
    {
        bool sourceIsEquipment = DragHandler.DraggedSlot is EquipmentSlot;
        bool targetIsEquipment = inventorySlot is EquipmentSlot;
        
        if (sourceIsEquipment && targetIsEquipment)
        {
            return DropOperation.SwapEquipment;
        }
        else if (!sourceIsEquipment && targetIsEquipment)
        {
            return DropOperation.EquipItem;
        }
        else if (sourceIsEquipment && !targetIsEquipment)
        {
            return DropOperation.UnequipItem;
        }
        else
        {
            return DropOperation.MoveInInventory;
        }
    }
    
    // ===== OPERAÇÕES DE DROP =====
    
    private void HandleEquipItem()
    {
        if (!ValidateInventoryAccess()) return;
        
        var inventory = GameStateManager.Instance.Player.inventory;
        int sourceSlotIndex = DragHandler.DraggedSlot.SlotIndex;
        
        // Equipar item do inventário
        inventory.SwapWithEquipment(sourceSlotIndex, DragHandler.DraggedItem.equipmentType);
    }
    
    private void HandleUnequipItem()
    {
        if (!ValidateInventoryAccess()) return;
        
        var inventory = GameStateManager.Instance.Player.inventory;
        var equipmentSlot = DragHandler.DraggedSlot as EquipmentSlot;
        EquipmentSlotType restrictedSlotType = equipmentSlot.GetRestrictedSlotType();
        int targetSlotIndex = inventorySlot.SlotIndex;
        
        // Desequipar item para o inventário
        inventory.UnequipItem(restrictedSlotType, targetSlotIndex);
        Debug.Log($"Item desequipado para slot {targetSlotIndex}");
    }
    
    private void HandleSwapEquipment()
    {
        if (!ValidateInventoryAccess()) return;
        
        var inventory = GameStateManager.Instance.Player.inventory;
        var sourceEquipmentSlot = DragHandler.DraggedSlot as EquipmentSlot;
        int sourceSlotIndex = sourceEquipmentSlot.SlotIndex;
        EquipmentSlotType restrictedSlotType = sourceEquipmentSlot.GetRestrictedSlotType();
        
        // Trocar equipamentos entre slots
        inventory.SwapWithEquipment(sourceSlotIndex, restrictedSlotType);
        Debug.Log($"Equipamentos trocados entre slots");
    }
    
    private void HandleInventoryMove()
    {
        if (!ValidateInventoryAccess()) return;
        
        var inventory = GameStateManager.Instance.Player.inventory;
        int sourceSlotIndex = DragHandler.DraggedSlot.SlotIndex;
        int targetSlotIndex = inventorySlot.SlotIndex;
        
        // Mover/trocar item no inventário
        inventory.MoveItem(sourceSlotIndex, targetSlotIndex);
        Debug.Log($"Item movido do slot {sourceSlotIndex} para {targetSlotIndex}");
    }
    
    // ===== MÉTODO AUXILIAR =====
    
    private bool ValidateInventoryAccess()
    {
        if (GameStateManager.Instance?.Player?.inventory == null)
        {
            Debug.LogError("Inventory não encontrado!");
            return false;
        }
        return true;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mostrar highlight apenas se há um item sendo arrastado
        if (DragHandler.CurrentDraggedItem != null && DragHandler.DraggedItem != null)
        {
            // Não destacar o slot de origem
            if (DragHandler.DraggedSlot.SlotIndex != inventorySlot.SlotIndex)
            {
                ShowDropHighlight();
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        HideDropHighlight();
    }
    
    private void ShowDropHighlight()
    {
        if (hasCustomHighlight && dropHighlight != null)
        {
            dropHighlight.SetActive(true);
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
    }
    
    private void HideDropHighlight()
    {
        if (hasCustomHighlight && dropHighlight != null)
        {
            dropHighlight.SetActive(false);
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = originalColor;
        }
    }
    
    private void OnDisable()
    {
        HideDropHighlight();
    }
}
