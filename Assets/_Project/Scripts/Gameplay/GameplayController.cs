using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Simulation;
using _Project.Scripts.Gameplay.View;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayController : MonoBehaviour
    {
        [SerializeField] private Transform _projectileRoot = null!;
        [SerializeField] private Transform _targetRoot = null!;
        [SerializeField] private Transform _buildingRoot = null!;

        [SerializeField] private ProjectileView _projectilePrefab = null!;
        [SerializeField] private TargetView _targetPrefab = null!;
        [SerializeField] private BuildSlotView _buildSlotViewPrefab = null!;

        [SerializeField] private int _buildSlotCount = 5;
        [SerializeField] private float _buildSlotStartX = 2f;
        [SerializeField] private float _buildSlotSpacing = 3f;
        
        [SerializeField] private List<TargetPlacement> _targetPlacements = new();

        [SerializeField] private float _simulationTick = 0.02f;
        [SerializeField] private float _laneLength = 20f;
        [SerializeField] private float _unitsPerLaneX = 1f;

        private LaneState? _laneState;
        private LaneSimulator? _simulator;

        private readonly Dictionary<int, ProjectileView> _projectileViews = new();
        private readonly Dictionary<int, TargetView> _targetViews = new();
        private readonly Dictionary<int, BuildingView> _buildingViews = new();
        private readonly Dictionary<int, BuildSlotView> _slotViews = new();

        private float _tickAccumulator;

        private void Start()
        {
            _simulator = new LaneSimulator(20f);
            _laneState = CreateLane();

            CreateViews(_laneState);
        }

        private void Update()
        {
            if (_simulator == null || _laneState == null)
            {
                return;
            }

            _tickAccumulator += Time.deltaTime;

            while (_tickAccumulator >= _simulationTick)
            {
                _tickAccumulator -= _simulationTick;

                var stepResult = _simulator.Step(_laneState, _simulationTick);
                ApplyStepVisuals(stepResult);
            }

            SyncViews(_laneState);
        }

        private LaneState CreateLane()
        {
            var lane = new LaneState();

            for (int i = 0; i < _buildSlotCount; i++)
            {
                float posX = _buildSlotStartX + i * _buildSlotSpacing;
                lane.BuildSlots.Add(new BuildSlotState(i, posX));
            }

            foreach (var placement in _targetPlacements)
            {
                lane.Targets.Add(new TargetState(
                    placement.Id,
                    placement.PositionX,
                    placement.Health));
            }

            return lane;
        }

        private void CreateViews(LaneState lane)
        {
            foreach (var slot in lane.BuildSlots)
            {
                var slotView = Instantiate(_buildSlotViewPrefab, _buildingRoot);
                slotView.Initialize(slot.Id);
                slotView.transform.position = LaneToWorld(slot.PositionX);
                slotView.DragStarted += OnSlotDragStarted;
                slotView.Dragged += OnSlotDragged;
                slotView.DragEnded += OnSlotDragEnded;
                _slotViews.Add(slot.Id, slotView);
            }

            foreach (var target in lane.Targets)
            {
                var view = Instantiate(_targetPrefab, _targetRoot);
                view.Initialize(target.Id);
                view.SetWorldPosition(LaneToWorld(target.PositionX));
                _targetViews.Add(target.Id, view);
            }
        }

        public bool TryPlaceBuilding(int slotId, BuildingDefinition definition)
        {
            if (_laneState == null)
            {
                return false;
            }

            if (slotId < 0 || slotId >= _laneState.BuildSlots.Count)
            {
                return false;
            }

            var slot = _laneState.BuildSlots[slotId];

            if (slot.IsOccupied)
            {
                return false;
            }

            BuildingState building;

            if (definition is GunBuildingDefinition gun)
            {
                building = new BuildingState(slotId, slot.PositionX, gun);
            }
            else
            {
                building = new BuildingState(slotId, slot.PositionX, definition);
            }

            slot.PlaceBuilding(building);
            SpawnBuildingView(building, definition);

            if (_slotViews.TryGetValue(slotId, out var slotView))
            {
                slotView.SetOccupied(true);
            }

            return true;
        }

        private const float RemoveDistanceThreshold = 0.5f;

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
            if (_laneState == null)
            {
                return;
            }

            var slot = _laneState.BuildSlots[slotView.SlotId];
            var slotWorldPosition = LaneToWorld(slot.PositionX);

            if (Vector3.Distance(worldPosition, slotWorldPosition) > RemoveDistanceThreshold)
            {
                slot.RemoveBuilding();
                slotView.SetOccupied(false);

                if (_buildingViews.TryGetValue(slotView.SlotId, out var buildingView))
                {
                    Destroy(buildingView.gameObject);
                    _buildingViews.Remove(slotView.SlotId);
                }
            }
            else
            {
                if (_buildingViews.TryGetValue(slotView.SlotId, out var buildingView))
                {
                    buildingView.SetWorldPosition(slotWorldPosition);
                }
            }
        }

        private void SpawnBuildingView(BuildingState building, BuildingDefinition definition)
        {
            var view = Instantiate(definition.Prefab, _buildingRoot);
            view.Initialize(building.Id, building.Type);
            view.SetWorldPosition(LaneToWorld(building.PositionX));
            _buildingViews.Add(building.Id, view);
        }

        private Vector3 LaneToWorld(float laneX, float y = 0f)
        {
            return new Vector3(laneX * _unitsPerLaneX, y, 0f);
        }

        private void SyncViews(LaneState lane)
        {
            SyncProjectileViews(lane);
            SyncTargetViews(lane);
        }

        private void SyncProjectileViews(LaneState lane)
        {
            var aliveProjectileIds = new HashSet<int>();

            foreach (var projectile in lane.Projectiles)
            {
                aliveProjectileIds.Add(projectile.Id);

                if (!_projectileViews.TryGetValue(projectile.Id, out var view))
                {
                    view = Instantiate(_projectilePrefab, _projectileRoot);
                    view.Initialize(projectile.Id);
                    _projectileViews.Add(projectile.Id, view);
                }

                view.SetWorldPosition(LaneToWorld(projectile.PositionX));
            }

            var idsToRemove = new List<int>();

            foreach (var pair in _projectileViews)
            {
                if (!aliveProjectileIds.Contains(pair.Key))
                {
                    Destroy(pair.Value.gameObject);
                    idsToRemove.Add(pair.Key);
                }
            }

            foreach (var id in idsToRemove)
            {
                _projectileViews.Remove(id);
            }
        }

        private void SyncTargetViews(LaneState lane)
        {
            var aliveTargetIds = new HashSet<int>();

            foreach (var target in lane.Targets)
            {
                aliveTargetIds.Add(target.Id);

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
                if (!aliveTargetIds.Contains(pair.Key))
                {
                    Destroy(pair.Value.gameObject);
                    idsToRemove.Add(pair.Key);
                }
            }

            foreach (var id in idsToRemove)
            {
                _targetViews.Remove(id);
            }
        }

        private void ApplyStepVisuals(SimulationStepResult stepResult)
        {
            foreach (var hit in stepResult.TargetHits)
            {
                Debug.Log($"Projectile {hit.ProjectileId} hit Target {hit.TargetId} for {hit.Damage}");
            }

            foreach (var spawn in stepResult.SpawnedProjectiles)
            {
                Debug.Log($"Spawned projectile {spawn.ProjectileId} at X={spawn.PositionX}");
            }

            foreach (var fire in stepResult.GunFires)
            {
                if (_buildingViews.TryGetValue(fire.GunId, out var buildingView))
                {
                    buildingView.PlayFireFeedback();
                }
            }
        }
    }

    [Serializable]
    public sealed class TargetPlacement
    {
        public int Id;
        public float PositionX;
        public float Health = 10f;
    }
}
