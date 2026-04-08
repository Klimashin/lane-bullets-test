using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class BuildingUIView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text _statsText = null!;

        public event Action<BuildingUIView>? DragStarted;
        public event Action<BuildingUIView, Vector2>? Dragged;
        public event Action<BuildingUIView, Vector2>? DragEnded;

        public void Initialize(BuildingDefinition definition)
        {
            _statsText.text = definition.GetStatsText();
        }

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
