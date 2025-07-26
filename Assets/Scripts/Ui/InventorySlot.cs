using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("Slot Configuration")]
    [SerializeField] protected Image itemImage;
    [SerializeField] protected TooltipTrigger tooltipTrigger;
    [SerializeField] private Sprite equipmentDefaultSprite; // Sprite padrão para slot de equipamento
    
    private Item currentItem;
    protected int slotIndex;
    protected InventoryTabPanel relatedPanel;
    public Item CurrentItem => currentItem;
    public InventoryTabPanel RelatedPanel => relatedPanel;
    public int SlotIndex => slotIndex;
    public bool IsEmpty => currentItem == null;
    
    public void Initialize(int index, InventoryTabPanel panel)
    {
        slotIndex = index;
        relatedPanel = panel;
        // Buscar componentes se não foram atribuídos no inspector
        if (itemImage == null)
        {
            Transform imageChild = transform.Find("Image");
            if (imageChild != null)
                itemImage = imageChild.GetComponent<Image>();
        }
        
        if (tooltipTrigger == null)
            tooltipTrigger = GetComponent<TooltipTrigger>();
    }
    
    public void SetItem(Item item)
    {
        currentItem = item;
        UpdateVisuals();
    }
    
    public void ClearItem()
    {
        currentItem = null;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (itemImage == null) return;

        if (currentItem != null)
        {
            // Slot com item
            itemImage.sprite = currentItem.itemIcon;
            itemImage.color = Color.white;

            // Configurar tooltip
            if (tooltipTrigger != null)
                tooltipTrigger.SetItem(currentItem);
        }
        else
        {
            // Slot base vazio: sem sprite e opacidade 0
            ClearSlot();
        }
    }

    protected virtual void ClearSlot()
    {
        itemImage.sprite = null;
        Color color = itemImage.color;
        color.a = 0f;
        itemImage.color = color;

        // Limpar tooltip
        if (tooltipTrigger != null)
            tooltipTrigger.SetItem(null);
    }
    
    public virtual bool CanAcceptItem(Item item)
    {
        if (item == null)
            return false;
        return true;
    }
}
