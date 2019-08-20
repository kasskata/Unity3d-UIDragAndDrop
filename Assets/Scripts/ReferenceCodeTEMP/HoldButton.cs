using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float holdTime = 0.5f;
    public UnityEvent onHold;

    private Coroutine holdCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        this.holdCoroutine = StartCoroutine(Hold());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopCoroutine(this.holdCoroutine);
    }

    private IEnumerator Hold()
    {
        yield return new WaitForSeconds(this.holdTime);
        this.onHold.Invoke();
    }
}
