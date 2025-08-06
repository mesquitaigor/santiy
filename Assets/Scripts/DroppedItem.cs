using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private DroppedItemContactArea contactSphere;
    [SerializeField] private List<Item> items;
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float viewAngleThreshold = 45f; // Ângulo em graus para detectar se está olhando
    private PlayerInputManager playerInputManager;
    private bool isPlayerInRange = false;
    private bool isPlayerLookingAt = false;

    void Start()
    {
        playerInputManager = InputActionsController.GetPlayerInputManager();
        contactSphere.OnPlayerEnter += HandlePlayerEnter;
        contactSphere.OnPlayerExit += HandlePlayerExit;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        ChangeCanvasState(false);
    }
    void Update()
    {
        if (worldSpaceCanvas != null && mainCamera != null && isPlayerInRange)
        {
            // Verifica se o jogador está olhando para o objeto
            bool wasLookingAt = isPlayerLookingAt;
            isPlayerLookingAt = IsPlayerLookingAtObject();
            
            // Atualiza o canvas apenas se o estado mudou
            if (wasLookingAt != isPlayerLookingAt)
            {
                ChangeCanvasState(isPlayerLookingAt);
            }
            
            // Rotaciona o canvas para ficar de frente para a câmera (apenas se visível)
            if (isPlayerLookingAt)
            {
                worldSpaceCanvas.transform.LookAt(mainCamera.transform);
                worldSpaceCanvas.transform.Rotate(0, 180, 0);
            }
        }
    }
    void OnDestroy()
    {
        contactSphere.OnPlayerEnter -= HandlePlayerEnter;
        contactSphere.OnPlayerExit -= HandlePlayerExit;
    }
    private void OnInteract()
    {
        if (isPlayerInRange)
        {
            AddDroppedItemsToPlayerInventory();
        }
    }
    private void ChangeCanvasState(bool state)
    {
        if (worldSpaceCanvas != null)
        {
            Debug.Log($"Changing canvas state to: {state}");
            worldSpaceCanvas.gameObject.SetActive(state);
        }
    }

    private bool IsPlayerLookingAtObject()
    {
        if (mainCamera == null) return false;
        
        Vector3 directionToObject = (transform.position - mainCamera.transform.position).normalized;
        Vector3 cameraForward = mainCamera.transform.forward;
        
        float dot = Vector3.Dot(cameraForward, directionToObject);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        
        return angle <= viewAngleThreshold;
    }

    private void HandlePlayerEnter()
    {
        if (items.Count > 0)
        {
            isPlayerInRange = true;
            // Canvas será mostrado apenas se estiver olhando (controlado no Update)
            playerInputManager.OnInteractStarted += OnInteract;
        }
    }
    private void AddDroppedItemsToPlayerInventory()
    {
        Player player = GameStateManager.Instance.Player;
        foreach (Item item in items)
        {
            player.AddItemToInventory(item);
        }
        items.Clear();
        Destroy(gameObject);
    }

    private void HandlePlayerExit()
    {
        if (items.Count > 0)
        {
            isPlayerInRange = false;
            isPlayerLookingAt = false;
            ChangeCanvasState(false);
            playerInputManager.OnInteractStarted -= OnInteract;
        }
    }
}
