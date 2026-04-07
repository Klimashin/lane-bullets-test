using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class DraggableBuildingUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<DraggableBuildingUI>? DragStarted;
        public event Action<DraggableBuildingUI, Vector2>? Dragged;
        public event Action<DraggableBuildingUI, Vector2>? DragEnded;

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(this, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragEnded?.Invoke(this, eventData.position);
        }
    }
}
