using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : InventorySlot
{
    [Header("Configurações de Tamanho")]
    [SerializeField] private float slotIconScale = 0.7f; // Tamanho do sprite padrão do slot (70% do tamanho original)
    [SerializeField] private float itemIconScale = 1f;   // Tamanho do sprite do item (100% do tamanho original)
    [SerializeField] private EquipmentSlotType restrictedToSlotType = EquipmentSlotType.None;

    private RectTransform itemImageRectTransform;
    private Vector2 originalItemImageSize;
    private Sprite slotIcon;

    public void RestrictedTo(EquipmentSlotType slotType)
    {
        restrictedToSlotType = slotType;
    }
    public EquipmentSlotType GetRestrictedSlotType()
    {
        return restrictedToSlotType;
    }

    public void InitializeSlot(int index, Sprite slotIcon, EquipmentsSlotsController panel)
    {
        Initialize(index, panel);
        this.slotIcon = slotIcon;
        CacheItemImageReferences();
        ApplyImageScale();
    }

    protected override void ClearSlot()
    {
        base.ClearSlot();
        itemImage.sprite = slotIcon;
        Color color = itemImage.color;
        color.a = 1f;
        itemImage.color = color;
    }

    // Sobrescrever método SetItem para aplicar escala quando item muda
    public new void SetItem(Item item)
    {
        base.SetItem(item);
        ApplyImageScale();
    }

    // Sobrescrever método ClearItem para aplicar escala quando item é removido
    public new void ClearItem()
    {
        base.ClearItem();
        ApplyImageScale();
    }
    
    public override bool CanAcceptItem(Item item)
    {
        if (item == null)
            return false;

        if (!EquipmentTypeConverter.CanEquipInSlot(item.equipmentType, restrictedToSlotType))
            return false;

        return true;
    }

    private void CacheItemImageReferences()
    {
        if (itemImage != null)
        {
            itemImageRectTransform = itemImage.rectTransform;
            originalItemImageSize = itemImageRectTransform.sizeDelta;

            if (originalItemImageSize == Vector2.zero)
            {
                originalItemImageSize = itemImageRectTransform.rect.size;
            }
        }
    }

    private void ApplyImageScale()
    {
        if (itemImageRectTransform == null)
        {
            CacheItemImageReferences();
            if (itemImageRectTransform == null) return;
        }
        if (CurrentItem != null)
        {
            SetImageScale(itemIconScale);
        }
        else
        {
            SetImageScale(slotIconScale);
        }
    }

    private void SetImageScale(float scale)
    {
        if (itemImageRectTransform != null)
        {
            Vector3 newScale = Vector3.one * scale;
            itemImageRectTransform.localScale = newScale;
        }
    }
}