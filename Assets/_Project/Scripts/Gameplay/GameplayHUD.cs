using System.Collections.Generic;
using _Project.Scripts.Gameplay.View;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas = null!;
        [SerializeField] private Camera _camera = null!;
        [SerializeField] private GameplayController _gameplayController = null!;
        [SerializeField] private Transform _buildingListParent = null!;
        [SerializeField] private List<BuildingDefinition> _availableBuildings = new();

        private readonly Dictionary<DraggableBuildingUI, BuildingDefinition> _buildingByDraggable = new();

        private GameObject? _dragCopy;

        private void Start()
        {
            foreach (var definition in _availableBuildings)
            {
                if (definition == null || definition.UIPrefab == null)
                {
                    continue;
                }

                var instance = Instantiate(definition.UIPrefab, _buildingListParent);
                var draggable = instance.GetComponent<DraggableBuildingUI>();

                if (draggable == null)
                {
                    continue;
                }

                _buildingByDraggable[draggable] = definition;

                draggable.DragStarted += OnDragStarted;
                draggable.Dragged += OnDragged;
                draggable.DragEnded += OnDragEnded;
            }
        }

        private void OnDragStarted(DraggableBuildingUI source)
        {
            _dragCopy = Instantiate(source.gameObject, _canvas.transform);
            _dragCopy.GetComponent<DraggableBuildingUI>().enabled = false;
        }

        private void OnDragged(DraggableBuildingUI source, Vector2 screenPosition)
        {
            if (_dragCopy == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPosition,
                null,
                out var localPoint);

            _dragCopy.GetComponent<RectTransform>().localPosition = localPoint;
        }

        private void OnDragEnded(DraggableBuildingUI source, Vector2 screenPosition)
        {
            if (_dragCopy != null)
            {
                Destroy(_dragCopy);
                _dragCopy = null;
            }

            var ray = _camera.ScreenPointToRay(screenPosition);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.TryGetComponent<BuildSlotView>(out var slotView))
            {
                if (_buildingByDraggable.TryGetValue(source, out var definition))
                {
                    _gameplayController.TryPlaceBuilding(slotView.SlotId, definition);
                }
            }
        }
    }
}
