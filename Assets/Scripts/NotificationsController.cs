using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class NotificationsController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float NotificationDuration = 5;
    [SerializeField] private GameObject NotificationPanel;
    public static NotificationsController Instance { get; private set; }
    
    public static event Action<string> OnNotificationTriggered;

    private Coroutine hideCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        NotificationPanel.SetActive(false);
    }

    void Start()
    {
        OnNotificationTriggered += HandleNotification;
    }

    void OnDestroy()
    {
        OnNotificationTriggered -= HandleNotification;
    }

    public static void TriggerNotification(string message)
    {
        OnNotificationTriggered?.Invoke(message);
    }

    private void HandleNotification(string message)
    {
        notificationText.text = message;
        NotificationPanel.SetActive(true);
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        hideCoroutine = StartCoroutine(HideNotificationAfterDelay());
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(NotificationDuration);
        HideNotification();
    }

    private void HideNotification()
    {
        NotificationPanel.SetActive(false);
    }
}
