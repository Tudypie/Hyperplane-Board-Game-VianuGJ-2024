using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    public UnityEvent startEvent;
    void Start()
    {
        startEvent?.Invoke();
    }
}
