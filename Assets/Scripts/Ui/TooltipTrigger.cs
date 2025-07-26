using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Item Item;
    [SerializeField] private float hoverDelay = 0.3f;
    
    private Coroutine hoverCoroutine;
    private Vector3 elementScreenPosition;
    
    public Vector3 ElementScreenPosition => elementScreenPosition;
    public Item CurrentItem => Item;

    public void SetItem(Item item)
    {
        Item = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        elementScreenPosition = eventData.position;
        hoverCoroutine = StartCoroutine(HoverDelayCoroutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        
        OnMouseExitDebug();
    }

    private IEnumerator HoverDelayCoroutine()
    {
        yield return new WaitForSeconds(hoverDelay);
        OnMouseEnterDebug();
    }

    private void OnMouseEnterDebug()
    {
        if (Item != null)
        {
            Tooltip tooltip = new Tooltip(Item.itemName, Item.itemDescription);
            RectTransform triggerRect = GetComponent<RectTransform>();

            Canvas canvas = GetComponentInParent<Canvas>();
            
            Vector3 elementScreenPosition;
            
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                elementScreenPosition = RectTransformUtility.WorldToScreenPoint(null, triggerRect.position);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                elementScreenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, triggerRect.position);
            }
            else
            {
                elementScreenPosition = Camera.main.WorldToScreenPoint(triggerRect.position);
            }
            TooltipController.ShowTooltip(elementScreenPosition, tooltip, triggerRect);
        }
    }
    private void OnMouseExitDebug()
    {
        TooltipController.HideTooltip();
    }
}