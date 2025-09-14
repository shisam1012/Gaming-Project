using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteractionController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LayerMask stoneLayer;

    private void Awake()
    {
       // _inputHandler = GetComponent<TouchInputHandler>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // check the origin eventData  to the target ScreenToWorldPoint - CHECK IF PLAYER TOUCHED THE STONE ON POINTER DOWN?
        // https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Physics2D.Raycast.html
        // var didHit = Physics.Raycast() 
        RaycastHit hit;
        //var stone = hit.collider.gameObject.GetComponent<Stone>();
        //if (stone != null)
        //{
        //}


    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}