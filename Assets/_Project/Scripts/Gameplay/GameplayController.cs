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
        [SerializeField] private BuildingView _buildingPrefab = null!;
        [SerializeField] private GunView _gunPrefab = null!;

        [SerializeField] private List<BuildingPlacement> _buildingPlacements = new ();
        [SerializeField] private List<TargetPlacement> _targetPlacements = new ();
        [SerializeField] private List<GunPlacement> _gunPlacements = new ();

        [SerializeField] private float _simulationTick = 0.02f;
        [SerializeField] private float _laneLength = 20f;
        [SerializeField] private float _unitsPerLaneX = 1f;

        private LaneState? _laneState;
        private LaneSimulator? _simulator;

        private readonly Dictionary<int, ProjectileView> _projectileViews = new();
        private readonly Dictionary<int, TargetView> _targetViews = new();
        private readonly Dictionary<int, BuildingView> _buildingViews = new();
        private readonly Dictionary<int, GunView> _gunViews = new();

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

            foreach (var placement in _buildingPlacements)
            {
                lane.Buildings.Add(new ModifierBuildingState(
                    placement.Id,
                    placement.PositionX,
                    placement.Type));
            }

            foreach (var placement in _targetPlacements)
            {
                lane.Targets.Add(new TargetState(
                    placement.Id,
                    placement.PositionX,
                    placement.Health));
            }
            
            foreach (var placement in _gunPlacements)
            {
                lane.Guns.Add(new GunState(
                    id: placement.Id,
                    positionX: placement.PositionX,
                    projectileSpeed: placement.ProjectileSpeed,
                    projectileDamage: placement.ProjectileDamage,
                    projectileHits: placement.ProjectileHits,
                    fireInterval: placement.FireInterval,
                    canProjectileBeCopied: placement.CanProjectileBeCopied,
                    initialCooldown: placement.InitialCooldown));
            }

            return lane;
        }
        
        private void CreateViews(LaneState lane)
        {
            foreach (var building in lane.Buildings)
            {
                var view = Instantiate(_buildingPrefab, _buildingRoot);
                view.Initialize(building.Id, building.Type);
                view.SetWorldPosition(LaneToWorld(building.PositionX));
                _buildingViews.Add(building.Id, view);
            }

            foreach (var target in lane.Targets)
            {
                var view = Instantiate(_targetPrefab, _targetRoot);
                view.Initialize(target.Id);
                view.SetWorldPosition(LaneToWorld(target.PositionX));
                _targetViews.Add(target.Id, view);
            }
            
            foreach (var gun in lane.Guns)
            {
                var view = Instantiate(_gunPrefab, _buildingRoot);
                view.Initialize(gun.Id);
                view.SetWorldPosition(LaneToWorld(gun.PositionX));
                _gunViews.Add(gun.Id, view);
            }
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
                if (_gunViews.TryGetValue(fire.GunId, out var gunView))
                {
                    gunView.PlayFireFeedback();
                }
            }
        }
    }
    
    [Serializable]
    public sealed class BuildingPlacement
    {
        public int Id;
        public float PositionX;
        public ModifierBuildingType Type;
    }

    [Serializable]
    public sealed class TargetPlacement
    {
        public int Id;
        public float PositionX;
        public float Health = 10f;
    }
    
    [Serializable]
    public sealed class GunPlacement
    {
        public int Id;
        public float PositionX;
        public float ProjectileSpeed = 5f;
        public float ProjectileDamage = 10f;
        public int ProjectileHits = 1;
        public float FireInterval = 1f;
        public bool CanProjectileBeCopied = true;
        public float InitialCooldown = 0f;
    }
}
