using UnityEngine; 
using Unity.Cinemachine;
using System.Collections.Generic;

public abstract class GameplayTabController : MonoBehaviour
{
    public abstract void Initialize();
    public abstract void OnOpenMenu();
    public abstract void OnCloseMenu();
}

/// <summary>
/// Controla a abertura e fechamento do menu de gameplay (inventário).
/// Gerencia a interação com o sistema de input e atualiza o estado global do jogo.
/// </summary>
public class GameplayMenuController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private InputActionsController inputActionsController;

    private PlayerInputManager playerInputManager;
    private GameplayMenuInputManager gamePlayerInputManager;
    [Header("Menu State")]
    [SerializeField] private List<GameplayTabController> gameplayTabs = new List<GameplayTabController>();

    public System.Action OnMenuOpened;
    private bool isMenuOpen = false;

    /// <summary>
    /// Propriedade pública para verificar se o menu está aberto.
    /// </summary>
    public bool IsMenuOpen => isMenuOpen;

    #region Unity Lifecycle

    void Awake()
    {
        if (inputActionsController == null)
        {
            Debug.LogError("InputActionsController is null in GameplayMenuController.Awake()", this);
        }
        inputActionsController.OnInputActionsInitialized += GetInputManagers;
    }
    private void GetInputManagers()
    {
        playerInputManager = InputActionsController.GetPlayerInputManager();
        gamePlayerInputManager = InputActionsController.GetGameplayMenuInputManager();
        
        if (playerInputManager == null)
        {
            Debug.LogError("PlayerInputManager is null in GameplayMenuController.Awake()", this);
        }
        
        if (gamePlayerInputManager == null)
        {
            Debug.LogError("GameplayMenuInputManager is null in GameplayMenuController.Awake()", this);
        }
        InitializeController();
        EnsureMenuStartsClosed();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Inicializa o controlador e subscreve aos eventos de input.
    /// </summary>
    private void InitializeController()
    {
        InitializeGameplayTabs();
        SubscribeToInputEvents();
    }

    private void InitializeGameplayTabs()
    {
        foreach (var tab in gameplayTabs)
        {
            if (tab != null)
            {
                tab.Initialize();
            }
        }
    }
    private void SubscribeToInputEvents()
    {
        if (playerInputManager != null)
        {
            playerInputManager.OnOpenInventory += OpenGameplayMenu;
        }
        else
        {
            Debug.LogError("PlayerInputManager is null, cannot subscribe to OnOpenInventory", this);
        }
        
        if (gamePlayerInputManager != null)
        {
            gamePlayerInputManager.OnInventoryKeyPressed += CloseGameplayMenu;
            gamePlayerInputManager.OnClosePressed += CloseGameplayMenu;
        }
        else
        {
            Debug.LogError("GameplayMenuInputManager is null, cannot subscribe to its events", this);
        }
    }
    private void UnsubscribeFromEvents()
    {
        if (playerInputManager != null)
        {
            playerInputManager.OnOpenInventory -= OpenGameplayMenu;
        }
        
        if (gamePlayerInputManager != null)
        {
            gamePlayerInputManager.OnInventoryKeyPressed -= CloseGameplayMenu;
            gamePlayerInputManager.OnClosePressed -= CloseGameplayMenu;
        }
    }
    private void EnsureMenuStartsClosed()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            isMenuOpen = false;
        }
    }

    #endregion

    #region Menu Control

    /// <summary>
    /// Abre o menu de gameplay se ele estiver fechado.
    /// </summary>
    private void OpenGameplayMenu()
    {
        if (!CanOpenMenu()) return;

        SetMenuState(true);
        OnOpenCallbacksGameplayTabs();
    }

    /// <summary>
    /// Fecha o menu de gameplay se ele estiver aberto.
    /// </summary>
    private void CloseGameplayMenu()
    {
        if (!CanCloseMenu()) return;

        SetMenuState(false);
        OnCloseCallbacksGameplayTabs();
    }

    /// <summary>
    /// Verifica se o menu pode ser aberto.
    /// </summary>
    /// <returns>True se pode abrir, false caso contrário.</returns>
    private bool CanOpenMenu()
    {
        return menuPanel != null && !isMenuOpen;
    }

    /// <summary>
    /// Verifica se o menu pode ser fechado.
    /// </summary>
    /// <returns>True se pode fechar, false caso contrário.</returns>
    private bool CanCloseMenu()
    {
        return menuPanel != null && isMenuOpen;
    }

    /// <summary>
    /// Define o estado do menu (aberto/fechado) e atualiza todos os sistemas relacionados.
    /// </summary>
    /// <param name="menuOpen">True para abrir, false para fechar.</param>
    private void SetMenuState(bool menuOpen)
    {
        isMenuOpen = menuOpen;
        UpdateMenuPanelVisibility();
        UpdateGlobalGameState();
    }

    /// <summary>
    /// Atualiza a visibilidade do painel do menu.
    /// </summary>
    private void UpdateMenuPanelVisibility()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuOpen);
            OnMenuOpened?.Invoke();
        }
    }

    /// <summary>
    /// Atualiza o estado global do jogo através do GameStateManager.
    /// </summary>
    private void UpdateGlobalGameState()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetMenuOpen(isMenuOpen);
        }
        else
        {
            Debug.LogWarning("GameStateManager.Instance não encontrado. " +
                           "O estado global do jogo não será atualizado.", this);
        }
    }

    private void OnOpenCallbacksGameplayTabs()
    {
        foreach (var tab in gameplayTabs)
        {
            if (tab != null)
            {
                tab.OnOpenMenu();
            }
        }
    }
    
    private void OnCloseCallbacksGameplayTabs()
    {
        foreach (var tab in gameplayTabs)
        {
            if (tab != null)
            {
                tab.OnCloseMenu();
            }
        }
    }

    #endregion
}
