using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    
    private InventorySlot inventorySlot;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject dragPreview;
    private Image dragImage;
    
    // Static para rastrear o item sendo arrastado
    public static DragHandler CurrentDraggedItem { get; private set; }
    public static Item DraggedItem { get; private set; }
    public static InventorySlot DraggedSlot { get; private set; }
    
    private void Awake()
    {
        inventorySlot = GetComponent<InventorySlot>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Se não tem CanvasGroup, adicionar um
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        // Buscar canvas se não foi atribuído
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
            
        // Buscar GraphicRaycaster se não foi atribuído
        if (graphicRaycaster == null)
            graphicRaycaster = canvas?.GetComponent<GraphicRaycaster>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Só permite drag se há um item no slot
        if (inventorySlot == null || inventorySlot.IsEmpty)
            return;
        
        // Configurar variáveis estáticas
        CurrentDraggedItem = this;
        DraggedItem = inventorySlot.CurrentItem;
        DraggedSlot = inventorySlot;
        
        // Criar preview do drag
        CreateDragPreview(eventData);
        
        // Tornar o slot original semitransparente
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreview == null) return;
        
        // Mover o preview do drag com o mouse
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            dragPreview.transform.localPosition = localPointerPosition;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreview == null) return;
        
        // Limpar o preview
        DestroyDragPreview();
        
        // Restaurar visibilidade do slot original
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        
        // Verificar se foi dropado em um slot válido
        CheckForValidDrop(eventData);

        
        // Limpar variáveis estáticas
        CurrentDraggedItem = null;
        DraggedItem = null;
        DraggedSlot = null;
    }
    
    private void CreateDragPreview(PointerEventData eventData)
    {
        if (inventorySlot.IsEmpty) return;
        
        // Criar objeto para preview
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);
        
        // Adicionar Image component
        dragImage = dragPreview.AddComponent<Image>();
        dragImage.sprite = inventorySlot.CurrentItem.itemIcon;
        dragImage.color = new Color(1, 1, 1, 0.8f); // Ligeiramente transparente
        
        // Configurar RectTransform
        RectTransform dragRect = dragPreview.GetComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;
        
        // Posicionar no mouse usando a posição do PointerEventData
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            dragPreview.transform.localPosition = localPointerPosition;
        }
        
        // Garantir que está na frente de tudo
        dragPreview.transform.SetAsLastSibling();
        
        // Adicionar CanvasGroup para não bloquear raycasts
        CanvasGroup previewCanvasGroup = dragPreview.AddComponent<CanvasGroup>();
        previewCanvasGroup.blocksRaycasts = false;
    }
    
    private void DestroyDragPreview()
    {
        if (dragPreview != null)
        {
            Destroy(dragPreview);
            dragPreview = null;
            dragImage = null;
        }
    }
    
    private bool CheckForValidDrop(PointerEventData eventData)
    {
        // Usar GraphicRaycaster para encontrar o que está sob o mouse
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, raycastResults);
        
        foreach (var result in raycastResults)
        {
            // Procurar por DropHandler no objeto ou nos seus pais
            DropHandler dropHandler = result.gameObject.GetComponent<DropHandler>();
            if (dropHandler == null)
                dropHandler = result.gameObject.GetComponentInParent<DropHandler>();
                
            if (dropHandler != null)
            {
                dropHandler.HandleDrop();
                return true;
            }
        }
        
        return false;
    }
}
