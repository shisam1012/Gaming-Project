using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerProbe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Action<PointerEventData> OnSelect;
    public Action<PointerEventData> OnMove;
    public Action<PointerEventData> OnRelease;

    public void OnPointerDown(PointerEventData e)
    {
        Debug.Log("pointer is down");
        OnSelect?.Invoke(e);
    }

    public void OnDrag(PointerEventData e)
    {
        OnMove?.Invoke(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        OnRelease?.Invoke(e);
    }
}
