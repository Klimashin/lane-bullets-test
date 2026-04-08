using System.Collections.Generic;
using _Project.Scripts.Gameplay.View;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas = null!;
        [SerializeField] private Camera _camera = null!;
        [SerializeField] private GameplayController _gameplayController = null!;
        [SerializeField] private Button _fireButton = null!;
        [SerializeField] private Slider _speedSlider = null!;
        [SerializeField] private Transform _buildingListParent = null!;
        [SerializeField] private List<BuildingDefinition> _availableBuildings = new();

        private readonly Dictionary<BuildingUIView, BuildingDefinition> _buildingByView = new();

        private GameObject? _dragCopy;

        private void Start()
        {
            _fireButton.onClick.AddListener(_gameplayController.TriggerAllGuns);
            _speedSlider.onValueChanged.AddListener(v => _gameplayController.SetSimulationSpeed((int)v));

            foreach (var definition in _availableBuildings)
            {
                if (definition == null || definition.UIPrefab == null)
                {
                    continue;
                }

                var instance = Instantiate(definition.UIPrefab, _buildingListParent);
                var uiView = instance.GetComponent<BuildingUIView>();

                if (uiView == null)
                {
                    continue;
                }

                uiView.Initialize(definition);
                _buildingByView[uiView] = definition;

                uiView.DragStarted += OnDragStarted;
                uiView.Dragged += OnDragged;
                uiView.DragEnded += OnDragEnded;
            }
        }

        private void OnDragStarted(BuildingUIView source)
        {
            _dragCopy = Instantiate(source.gameObject, _canvas.transform);
            _dragCopy.GetComponent<BuildingUIView>().enabled = false;
        }

        private void OnDragged(BuildingUIView source, Vector2 screenPosition)
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

        private void OnDragEnded(BuildingUIView source, Vector2 screenPosition)
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
                if (_buildingByView.TryGetValue(source, out var definition))
                {
                    _gameplayController.TryPlaceBuilding(slotView.LaneId, slotView.SlotId, definition);
                }
            }
        }
    }
}
