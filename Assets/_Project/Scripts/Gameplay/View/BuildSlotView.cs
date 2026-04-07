using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class BuildSlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int SlotId { get; private set; }
        public bool IsOccupied { get; private set; }

        public event Action<BuildSlotView>? DragStarted;
        public event Action<BuildSlotView, Vector3>? Dragged;
        public event Action<BuildSlotView, Vector3>? DragEnded;

        public void Initialize(int slotId)
        {
            SlotId = slotId;
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsOccupied)
            {
                return;
            }

            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsOccupied)
            {
                return;
            }

            Dragged?.Invoke(this, GetWorldPosition(eventData));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsOccupied)
            {
                return;
            }

            DragEnded?.Invoke(this, GetWorldPosition(eventData));
        }

        private static Vector3 GetWorldPosition(PointerEventData eventData)
        {
            var worldPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;
            return worldPos;
        }
    }
}
