using System;
using UnityEngine;

public class DroppedItemContactArea : MonoBehaviour
{
    public Action OnPlayerEnter;
    public Action OnPlayerExit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExit?.Invoke();
        }
    }
}
