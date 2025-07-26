using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }
    
    [Header("Tooltip UI Elements")]
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Tooltip Positioning")]
    [SerializeField] private float offset = 20f;
    [SerializeField] private bool showAbove = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void ShowTooltip(Vector3 screenPosition, Tooltip tooltip, RectTransform triggerRect = null)
    {
        if (Instance != null)
        {
            Instance.DisplayTooltip(screenPosition, tooltip, triggerRect);
        }
    }

    public static void HideTooltip()
    {
        if (Instance != null)
        {
            Instance.HideTooltipInternal();
        }
    }

    private void DisplayTooltip(Vector3 screenPosition, Tooltip tooltip, RectTransform triggerRect = null)
    {
        if (tooltipObject == null || tooltip == null) return;
        
        tooltipObject.SetActive(true);
        
        if (titleText != null)
            titleText.text = tooltip.title;
        if (descriptionText != null)
            descriptionText.text = tooltip.description;
        
        Canvas.ForceUpdateCanvases();
        
        RectTransform tooltipRect = tooltipObject.GetComponent<RectTransform>();
        if (tooltipRect == null) return;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Rect tooltipWorldRect = tooltipRect.rect;
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        RectTransform canvasRect = canvas.transform as RectTransform;                               
        
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPosition
        );
        
        float calculatedOffset = offset;
        if (triggerRect != null)
        {
            Bounds triggerBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, triggerRect);
            float triggerHeight = triggerBounds.size.y;
            
            calculatedOffset = offset + (triggerHeight / 2f);
        }
        
        Vector2 tooltipRealSize = tooltipWorldRect.size;
        Vector2 canvasSize = canvasRect.sizeDelta;
        float halfCanvasWidth = canvasSize.x / 2f;
        float halfCanvasHeight = canvasSize.y / 2f;
        float halfTooltipWidth = tooltipRealSize.x / 2f;
        float halfTooltipHeight = tooltipRealSize.y / 2f;
        
        float triggerTopY = localPosition.y;
        if (triggerRect != null)
        {
            Bounds triggerBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, triggerRect);
            triggerTopY = triggerBounds.max.y;
        }
        
        float tooltipHeight = tooltipRealSize.y;
        float spaceAbove = halfCanvasHeight - triggerTopY;
        float spaceNeededAbove = tooltipHeight + offset;
        
        bool positionAbove = showAbove;
        if (showAbove && spaceAbove < spaceNeededAbove)
        {
            positionAbove = false;
        }
        else if (!showAbove)
        {
            float triggerBottomY = localPosition.y;
            if (triggerRect != null)
            {
                Bounds triggerBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, triggerRect);
                triggerBottomY = triggerBounds.min.y;
            }
            
            float spaceBelow = triggerBottomY - (-halfCanvasHeight);
            float spaceNeededBelow = tooltipHeight + offset;
            
            if (spaceBelow < spaceNeededBelow)
            {
                positionAbove = true;
            }
        }
        
        Vector2 tooltipPosition = localPosition;
        
        if (positionAbove)
        {
            tooltipPosition.y = triggerTopY + halfTooltipHeight + offset;
        }
        else
        {
            float triggerBottomY = localPosition.y;
            float triggerHeight = 0f;
            if (triggerRect != null)
            {
                Bounds triggerBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, triggerRect);
                triggerBottomY = triggerBounds.min.y;
                triggerHeight = triggerBounds.size.y;
            }
            
            float distanceToMove = offset + halfTooltipHeight;
            tooltipPosition.y = triggerBottomY - distanceToMove;
        }
        
        Vector2 adjustedPosition = tooltipPosition;
        
        tooltipRect.anchoredPosition = adjustedPosition;
    }

    private void HideTooltipInternal()
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }
}