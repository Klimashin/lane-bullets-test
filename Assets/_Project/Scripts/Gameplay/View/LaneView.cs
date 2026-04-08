using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Data;
using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;
using UnityEngine.Pool;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class LaneView : MonoBehaviour
    {
        [SerializeField] private Transform _projectileRoot = null!;
        [SerializeField] private Transform _targetRoot = null!;
        [SerializeField] private Transform _buildingRoot = null!;

        [SerializeField] private ProjectileView _projectilePrefab = null!;
        [SerializeField] private TargetView _targetPrefab = null!;
        [SerializeField] private BuildSlotView _buildSlotViewPrefab = null!;

        public int LaneId { get; private set; }

        public event Action<int, int>? BuildingRemoveRequested; // <laneId, slotId>

        private readonly Dictionary<int, ProjectileView> _projectileViews = new();
        private readonly Dictionary<int, TargetView> _targetViews = new();
        private readonly Dictionary<int, BuildingView> _buildingViews = new();
        private readonly Dictionary<int, BuildSlotView> _slotViews = new();
        private readonly Dictionary<int, Vector3> _slotWorldPositions = new();

        private float _worldY;
        private ObjectPool<ProjectileView>? _projectilePool;
        private ObjectPool<ProjectileView> ProjectilePool
        {
            get
            {
                _projectilePool ??= new ObjectPool<ProjectileView>(
                    createFunc: () => Instantiate(_projectilePrefab, _projectileRoot),
                    actionOnGet: v => v.gameObject.SetActive(true),
                    actionOnRelease: v => v.gameObject.SetActive(false),
                    actionOnDestroy: v => Destroy(v.gameObject));

                return _projectilePool;
            }
        }

        private const float REMOVE_DISTANCE_THRESHOLD = 0.5f;

        public void Initialize(LaneState lane, int laneId, float worldY)
        {
            LaneId = laneId;
            _worldY = worldY;

            foreach (var slot in lane.BuildSlots)
            {
                var worldPos = LaneToWorld(slot.PositionX);
                var slotView = Instantiate(_buildSlotViewPrefab, _buildingRoot);
                slotView.Initialize(slot.Id, laneId);
                slotView.transform.position = worldPos;
                slotView.DragStarted += OnSlotDragStarted;
                slotView.Dragged += OnSlotDragged;
                slotView.DragEnded += OnSlotDragEnded;
                _slotViews.Add(slot.Id, slotView);
                _slotWorldPositions.Add(slot.Id, worldPos);
            }

            foreach (var target in lane.Targets)
            {
                var view = Instantiate(_targetPrefab, _targetRoot);
                view.Initialize(target.Id);
                view.SetWorldPosition(LaneToWorld(target.PositionX));
                _targetViews.Add(target.Id, view);
            }
        }

        public void SyncViews(LaneState lane)
        {
            SyncProjectileViews(lane);
            SyncTargetViews(lane);
        }

        public void ApplyStepVisuals(SimulationStepResult stepResult)
        {
            foreach (var hit in stepResult.TargetHits)
            {
                if (_targetViews.TryGetValue(hit.TargetId, out var targetView))
                {
                    targetView.PlayHitFeedback();
                }
            }

            foreach (var fire in stepResult.GunFires)
            {
                if (_buildingViews.TryGetValue(fire.GunId, out var buildingView))
                {
                    buildingView.PlayFeedback();
                }
            }

            foreach (var trigger in stepResult.TriggeredBuildings)
            {
                if (_projectileViews.TryGetValue(trigger.ProjectileId, out var projectileView))
                {
                    projectileView.PlayFeedback();
                }
            }
        }

        public void SpawnBuildingView(BuildingState building, BuildingDefinition definition)
        {
            var view = Instantiate(definition.Prefab, _buildingRoot);
            view.Initialize(building.Id, definition);
            view.SetWorldPosition(LaneToWorld(building.PositionX));
            _buildingViews.Add(building.Id, view);
        }

        public void RemoveBuildingView(int slotId)
        {
            if (_buildingViews.TryGetValue(slotId, out var buildingView))
            {
                Destroy(buildingView.gameObject);
                _buildingViews.Remove(slotId);
            }
        }

        public void SetSlotOccupied(int slotId, bool occupied)
        {
            if (_slotViews.TryGetValue(slotId, out var slotView))
            {
                slotView.SetOccupied(occupied);
            }
        }

        private void SyncProjectileViews(LaneState lane)
        {
            var aliveIds = new HashSet<int>();

            foreach (var projectile in lane.Projectiles)
            {
                aliveIds.Add(projectile.Id);

                if (!_projectileViews.TryGetValue(projectile.Id, out var view))
                {
                    view = ProjectilePool.Get();
                    view.Initialize(projectile.Id);
                    _projectileViews.Add(projectile.Id, view);
                }

                view.SetWorldPosition(LaneToWorld(projectile.PositionX));
                view.SetDamage(projectile.Damage);
                view.SetIsCopy(!projectile.CanBeCopied);
            }

            var idsToRemove = new List<int>();
            foreach (var pair in _projectileViews)
            {
                if (!aliveIds.Contains(pair.Key))
                {
                    ProjectilePool.Release(pair.Value);
                    idsToRemove.Add(pair.Key);
                }
            }

            foreach (var id in idsToRemove)
                _projectileViews.Remove(id);
        }

        private void SyncTargetViews(LaneState lane)
        {
            var aliveIds = new HashSet<int>();

            foreach (var target in lane.Targets)
            {
                aliveIds.Add(target.Id);

                if (!_targetViews.TryGetValue(target.Id, out var view))
                {
                    view = Instantiate(_targetPrefab, _targetRoot);
                    view.Initialize(target.Id);
                    _targetViews.Add(target.Id, view);
                }

                view.SetWorldPosition(LaneToWorld(target.PositionX));
                view.SetHealth(target.Health);
            }

            var idsToRemove = new List<int>();
            foreach (var pair in _targetViews)
            {
                if (!aliveIds.Contains(pair.Key))
                {
                    Destroy(pair.Value.gameObject);
                    idsToRemove.Add(pair.Key);
                }
            }

            foreach (var id in idsToRemove)
                _targetViews.Remove(id);
        }

        private void OnSlotDragStarted(BuildSlotView slotView)
        {
        }

        private void OnSlotDragged(BuildSlotView slotView, Vector3 worldPosition)
        {
            if (_buildingViews.TryGetValue(slotView.SlotId, out var buildingView))
            {
                buildingView.SetWorldPosition(worldPosition);
            }
        }

        private void OnSlotDragEnded(BuildSlotView slotView, Vector3 worldPosition)
        {
            var slotWorldPosition = _slotWorldPositions[slotView.SlotId];

            if (Vector3.Distance(worldPosition, slotWorldPosition) > REMOVE_DISTANCE_THRESHOLD)
            {
                SetSlotOccupied(slotView.SlotId, false);
                RemoveBuildingView(slotView.SlotId);
                BuildingRemoveRequested?.Invoke(LaneId, slotView.SlotId);
            }
            else
            {
                if (_buildingViews.TryGetValue(slotView.SlotId, out var buildingView))
                {
                    buildingView.SetWorldPosition(slotWorldPosition);
                }
            }
        }

        private Vector3 LaneToWorld(float laneX)
        {
            return new Vector3(laneX, _worldY, 0f);
        }
        
        private void OnDestroy()
        {
            _projectilePool?.Dispose();
        }
    }
}
