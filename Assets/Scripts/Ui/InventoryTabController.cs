using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public abstract class InventoryTabPanel : MonoBehaviour
{
    [Header("Prefab do Slot")]
    [SerializeField] protected GameObject slotPrefab;

    [Header("Events")]
    [SerializeField] protected UnityEvent onSlotsInitialized;
    [SerializeField] protected UnityEvent onInventoryRefreshed;
    protected List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public virtual void Initialize()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("Slot Prefab não foi atribuído no InventorySlotsController!", this);
            return;
        }
        ClearExistingSlots();
    }
    public virtual void OnCloseMenu()
    {
        ClearExistingSlots();
    }
    public virtual void OnOpenMenu()
    {
        InitializeSlots();
        RefreshInventoryDisplay();
    }
    protected abstract void ClearExistingSlots();
    public abstract void RefreshInventoryDisplay();
    protected abstract void InitializeSlots();
    protected Inventory GetPlayerInventory()
    {
        if (GameStateManager.Instance?.Player?.inventory == null)
        {
            Debug.LogWarning("GameStateManager.Instance.Player.inventory não encontrado!", this);
            return null;
        }

        return GameStateManager.Instance.Player.inventory;
    }
}

public class InventoryTabController : GameplayTabController
{

    [Header("Painéis do Inventário")]
    [SerializeField] private List<InventoryTabPanel> inventoryPanels = new List<InventoryTabPanel>();

    #region Menu Lifecycle
    public override void Initialize()
    {
        ForEachPanel(panel => panel.Initialize());
    }
    public override void OnOpenMenu()
    {
        ForEachPanel(panel => panel.OnOpenMenu());
    }
    public override void OnCloseMenu()
    {
        ForEachPanel(panel => panel.OnCloseMenu());
    }
    #endregion

    private void ForEachPanel(System.Action<InventoryTabPanel> action)
    {
        if (inventoryPanels.Count == 0)
        {
            Debug.LogWarning("Nenhum painel de inventário configurado.");
        }
        foreach (var panel in inventoryPanels)
        {
            action(panel);
        }
    }
    
}