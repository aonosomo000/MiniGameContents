using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform yPosT;
    [SerializeField] private CircleDropMachine machine;
    [SerializeField] private LineRenderer lineRenderer;

    private bool isEnter = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isEnter = false;
        machine.MoveCircleOnMouse(machine.spawnPos.position);
        var zero = new Vector3(0f, 0f, 0f);
        lineRenderer.SetPosition(0, zero);
        lineRenderer.SetPosition(1, zero);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if(isEnter)
        {
            var topPos = new Vector2(eventData.pointerCurrentRaycast.worldPosition.x, yPosT.position.y);
            var mousePos = eventData.pointerCurrentRaycast.worldPosition;

            lineRenderer.SetPosition(0, topPos);
            lineRenderer.SetPosition(1, mousePos);

            machine.MoveCircleOnMouse(topPos);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        machine.PlayDrop(new Vector2(eventData.pointerCurrentRaycast.worldPosition.x, yPosT.position.y));
    }

}
