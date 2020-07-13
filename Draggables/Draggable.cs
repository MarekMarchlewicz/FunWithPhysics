using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public event System.Action OnDragStart, OnDragEnd;

    public void StartDragging()
    {
        OnDragStart?.Invoke();
    }

    public void StopDragging()
    {
        OnDragEnd?.Invoke();
    }
}
