using UnityEngine;
using UnityEngine.EventSystems;

public class PointerProbe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData e) { Debug.Log("DOWN " + e.position); }
    public void OnDrag(PointerEventData e)       { Debug.Log("DRAG " + e.position); }
    public void OnPointerUp(PointerEventData e)  { Debug.Log("UP " + e.position); }
}
