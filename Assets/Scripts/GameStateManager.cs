using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gerenciador central do estado do jogo.
/// Controla quando menus estão abertos e determina se o player pode se mover ou a câmera pode funcionar.
/// Gerencia a alternância entre action maps do Player e GameplayMenu.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Header("Estado do Gameplay")]
    [SerializeField] private bool isAnyMenuOpen = false;
    [SerializeField] private Player player;
    
    [Header("Configurações do Cursor")]
    [SerializeField] private bool lockCursorInGameplay = true;
    
    // Propriedade pública para acessar o player
    public Player Player => player;
    
    // Singleton para fácil acesso global
    public static GameStateManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Inicializa os action maps
        InitializeActionMaps();
        
        // Inicia com nenhum menu aberto
        isAnyMenuOpen = false;
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    /// <summary>
    /// Inicializa e configura os action maps
    /// </summary>
    private void InitializeActionMaps()
    {
        // Habilita o action map do player por padrão
        SetPlayerActionMapActive(true);
        
        // Configura o cursor inicial
        SetCursorState(false);
    }
    
    /// <summary>
    /// Ativa ou desativa o action map do player
    /// </summary>
    /// <param name="active">True para ativar, False para desativar</param>
    private void SetPlayerActionMapActive(bool active)
    {
        if (InputActionsController.Instance != null)
        {
            InputActionsController.Instance.SetInputActionState(InputActionType.Player, active);
        }
        else
        {
            Debug.LogWarning("InputActionsController.Instance is null when trying to set Player action map active state.");
        }
    }
    
    /// <summary>
    /// Ativa ou desativa o action map do gameplay menu
    /// </summary>
    /// <param name="active">True para ativar, False para desativar</param>
    private void SetGameplayMenuActionMapActive(bool active)
    {
        if (InputActionsController.Instance != null)
        {
            InputActionsController.Instance.SetInputActionState(InputActionType.GameplayMenu, active);
        }
        else
        {
            Debug.LogWarning("InputActionsController.Instance is null when trying to set GameplayMenu action map active state.");
        }
    }
    
    /// <summary>
    /// Configura o estado do cursor baseado se menus estão abertos
    /// </summary>
    /// <param name="menuOpen">True se algum menu está aberto</param>
    private void SetCursorState(bool menuOpen)
    {
        if (lockCursorInGameplay)
        {
            if (menuOpen)
            {
                // Menu aberto: libera e mostra o cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Gameplay: trava e esconde o cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    /// <summary>
    /// Alterna entre os action maps baseado no estado do menu
    /// </summary>
    /// <param name="menuOpen">True se algum menu está aberto</param>
    private void SwitchActionMaps(bool menuOpen)
    {
        if (menuOpen)
        {
            SetPlayerActionMapActive(false);
            SetGameplayMenuActionMapActive(true);
        }
        else
        {
            SetPlayerActionMapActive(true);
            SetGameplayMenuActionMapActive(false);
        }
        
        // Atualiza o estado do cursor
        SetCursorState(menuOpen);
    }
    
    /// <summary>
    /// Define se algum menu está aberto no jogo
    /// </summary>
    /// <param name="menuOpen">True se algum menu está aberto, False se todos estão fechados</param>
    public void SetMenuOpen(bool menuOpen)
    {
        if (isAnyMenuOpen != menuOpen)
        {
            isAnyMenuOpen = menuOpen;
            SwitchActionMaps(menuOpen);
        }
    }
}
