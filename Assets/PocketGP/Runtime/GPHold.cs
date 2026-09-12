using UnityEngine;
using UnityEngine.EventSystems;
namespace PocketGP {
public sealed class GPHold : MonoBehaviour,IPointerDownHandler,IPointerUpHandler {
    public System.Action<bool> Changed; bool held;
    public void OnPointerDown(PointerEventData e){held=true;Changed?.Invoke(true);}
    public void OnPointerUp(PointerEventData e){held=false;Changed?.Invoke(false);}
    void OnDisable(){if(held)Changed?.Invoke(false);held=false;}
}
}
