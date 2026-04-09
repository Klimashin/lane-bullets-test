using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Data;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class LaneSimulator
    {
        private int _nextProjectileId = 1;
        private readonly float _laneStartX;
        private readonly float _laneMaxX;
        private readonly float _targetOffsetX;
        private readonly List<LaneState> _laneStates = new();
        private SimulationStepResult[] _stepResults = Array.Empty<SimulationStepResult>();
        private readonly List<ProjectileState> _pendingProjectilesBuffer = new();
        private readonly List<LaneEvent> _eventsBuffer = new();

        public IReadOnlyList<LaneState> LaneStates => _laneStates;

        private GunsControlMode _mode = GunsControlMode.Manual;
        public GunsControlMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                ResetAllGunCooldowns();
            }
        }

        public LaneSimulator(LaneSimulationConfig config, IReadOnlyList<LaneConfig> laneConfigs)
        {
            _laneStartX = config.LaneStartX;
            _laneMaxX = config.LaneStartX + config.LaneLength;
            _targetOffsetX = config.TargetOffsetX;

            foreach (var laneConfig in laneConfigs)
            {
                _laneStates.Add(CreateLane(config, laneConfig));
            }

            _stepResults = new SimulationStepResult[_laneStates.Count];
            for (int i = 0; i < _stepResults.Length; i++)
            {
                _stepResults[i] = new SimulationStepResult();
            }
        }

        public IReadOnlyList<SimulationStepResult> StepAll(float deltaTime)
        {
            for (int i = 0; i < _laneStates.Count; i++)
            {
                _stepResults[i].Clear();
                Step(_laneStates[i], deltaTime, _stepResults[i]);
            }

            return _stepResults;
        }

        public void TriggerAllGuns()
        {
            foreach (var lane in _laneStates)
            {
                foreach (var slot in lane.BuildSlots)
                {
                    if (slot.Building is GunBuildingState gun)
                    {
                        gun.PendingFire = true;
                    }
                }
            }
        }

        public BuildingState? TryPlaceBuilding(int laneId, int slotId, BuildingDefinition definition)
        {
            if (laneId < 0 || laneId >= _laneStates.Count)
            {
                return null;
            }

            var lane = _laneStates[laneId];

            if (slotId < 0 || slotId >= lane.BuildSlots.Count)
            {
                return null;
            }

            var slot = lane.BuildSlots[slotId];

            if (slot.IsOccupied)
            {
                return null;
            }

            var building = definition.CreateState(slotId, slot.PositionX);
            slot.PlaceBuilding(building);
            return building;
        }

        public void RemoveBuilding(int laneId, int slotId)
        {
            if (laneId < 0 || laneId >= _laneStates.Count)
            {
                return;
            }

            _laneStates[laneId].BuildSlots[slotId].RemoveBuilding();
        }

        private static LaneState CreateLane(LaneSimulationConfig config, LaneConfig laneConfig)
        {
            var lane = new LaneState();

            for (int i = 0; i < laneConfig.BuildSlotCount; i++)
            {
                float posX = config.LaneStartX + config.BuildingSlotsOffsetX + i * config.BuildSlotSpacing;
                lane.BuildSlots.Add(new BuildSlotState(i, posX));
            }

            float lastSlotX = config.LaneStartX + config.BuildingSlotsOffsetX
                + (laneConfig.BuildSlotCount - 1) * config.BuildSlotSpacing;
            float firstTargetX = lastSlotX + config.FirstTargetOffsetFromBuilding;

            for (int i = 0; i < laneConfig.TargetHealthValues.Count; i++)
            {
                float posX = firstTargetX + i * config.TargetsSpacing;
                lane.Targets.Add(new TargetState(i, posX, laneConfig.TargetHealthValues[i]));
            }

            return lane;
        }

        private void Step(LaneState lane, float deltaTime, SimulationStepResult result)
        {
            _pendingProjectilesBuffer.Clear();

            SimulateGuns(lane, deltaTime, _pendingProjectilesBuffer, result);

            int projectileCount = lane.Projectiles.Count;
            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = lane.Projectiles[i];
                if (projectile.IsDestroyed)
                {
                    continue;
                }

                SimulateProjectile(projectile, lane, deltaTime, _pendingProjectilesBuffer, result);
            }

            lane.Projectiles.AddRange(_pendingProjectilesBuffer);
            lane.RemoveDestroyedEntities();
            lane.CleanupProjectilesOutsideRange(_laneMaxX);
        }

        private void SimulateProjectile(
            ProjectileState projectile,
            LaneState lane,
            float deltaTime,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            float prevX = projectile.PositionX;
            float nextX = prevX + projectile.Speed * deltaTime;

            var events = CollectEventsOnSegment(lane, prevX, nextX);
            SortEvents(events);

            foreach (var laneEvent in events)
            {
                if (projectile.IsDestroyed)
                {
                    break;
                }

                switch (laneEvent.Type)
                {
                    case LaneEventType.Building:
                        ProcessBuilding(projectile, laneEvent.Building, pendingProjectiles, result);
                        break;

                    case LaneEventType.Target:
                        ProcessTarget(projectile, laneEvent.Target, result);
                        break;

                    default:
                        throw new Exception("Unimplemented LaneEventType");
                }
            }

            if (!projectile.IsDestroyed)
            {
                projectile.PositionX = nextX;
            }
        }

        private List<LaneEvent> CollectEventsOnSegment(LaneState lane, float prevX, float nextX)
        {
            _eventsBuffer.Clear();
            var events = _eventsBuffer;

            foreach (var slot in lane.BuildSlots)
            {
                if (slot.Building == null || slot.Building.Type == BuildingType.Gun)
                {
                    continue;
                }

                if (prevX < slot.Building.PositionX && slot.Building.PositionX <= nextX)
                {
                    events.Add(new LaneEvent(slot.Building));
                }
            }

            foreach (var target in lane.Targets)
            {
                if (target.IsDestroyed)
                {
                    continue;
                }

                if (prevX < target.PositionX + _targetOffsetX && target.PositionX + _targetOffsetX <= nextX)
                {
                    events.Add(new LaneEvent(target));
                }
            }

            return events;
        }

        private void SortEvents(List<LaneEvent> events)
        {
            events.Sort((a, b) =>
            {
                int xCompare = a.PositionX.CompareTo(b.PositionX);
                if (xCompare != 0)
                {
                    return xCompare;
                }

                return ((int)a.Type).CompareTo((int)b.Type);
            });
        }

        private void ProcessBuilding(
            ProjectileState projectile,
            BuildingState? building,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            if (building == null)
            {
                return;
            }

            switch (building.Definition)
            {
                case DamageBoostBuildingDefinition damageBoost:
                    projectile.Damage *= damageBoost.DamageMultiplier;
                    break;

                case PierceBoostBuildingDefinition pierceBoost:
                    projectile.RemainingHits += pierceBoost.HitsBonus;
                    break;

                case CopyBuildingDefinition copyDef:
                    if (!projectile.CanBeCopied)
                    {
                        return;
                    }

                    var copy = CreateCopy(projectile, _laneStartX, copyDef.CopyDamageFraction);
                    pendingProjectiles.Add(copy);
                    result.SpawnedProjectiles.Add(new ProjectileSpawnInfo(copy.Id, copy.PositionX));
                    break;
            }

            result.TriggeredBuildings.Add(new BuildingTriggerInfo(projectile.Id, building.Id));
        }

        private void ProcessTarget(
            ProjectileState projectile,
            TargetState? target,
            SimulationStepResult result)
        {
            if (target == null || target.IsDestroyed)
            {
                return;
            }

            target.ApplyDamage(projectile.Damage);
            result.TargetHits.Add(new TargetHitInfo(projectile.Id, target.Id, projectile.Damage));

            projectile.RemainingHits--;

            if (projectile.RemainingHits <= 0)
            {
                projectile.Destroy();
            }
        }

        private ProjectileState CreateCopy(ProjectileState source, float spawnX, float damageFraction)
        {
            return new ProjectileState(
                id: _nextProjectileId++,
                positionX: spawnX,
                speed: source.Speed,
                damage: source.Damage * damageFraction,
                remainingHits: source.RemainingHits,
                canBeCopied: false);
        }

        private void SimulateGuns(
            LaneState lane,
            float deltaTime,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            foreach (var slot in lane.BuildSlots)
            {
                if (slot.Building is GunBuildingState gun)
                {
                    SimulateGun(gun, deltaTime, pendingProjectiles, result);
                }
            }
        }

        private void SimulateGun(
            GunBuildingState gun,
            float deltaTime,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            if (_mode == GunsControlMode.Auto)
            {
                gun.CooldownRemaining -= deltaTime;
                if (gun.CooldownRemaining <= 0f)
                {
                    var def = (GunBuildingDefinition)gun.Definition;
                    gun.CooldownRemaining += def.FireInterval;
                    gun.PendingFire = true;
                }
            }

            if (!gun.PendingFire)
            {
                return;
            }

            gun.PendingFire = false;

            var projectile = CreateProjectileFromGun(gun);
            pendingProjectiles.Add(projectile);

            result.SpawnedProjectiles.Add(new ProjectileSpawnInfo(projectile.Id, projectile.PositionX));
            result.GunFires.Add(new GunFireInfo(gun.Id, projectile.Id));
        }

        private void ResetAllGunCooldowns()
        {
            foreach (var lane in _laneStates)
            {
                foreach (var slot in lane.BuildSlots)
                {
                    if (slot.Building is GunBuildingState gun)
                    {
                        var def = (GunBuildingDefinition)gun.Definition;
                        gun.CooldownRemaining = def.InitialCooldown;
                        gun.PendingFire = false;
                    }
                }
            }
        }

        private ProjectileState CreateProjectileFromGun(GunBuildingState gun)
        {
            var def = (GunBuildingDefinition)gun.Definition;
            return new ProjectileState(
                id: _nextProjectileId++,
                positionX: gun.PositionX + def.SpawnOffsetX,
                speed: def.ProjectileSpeed,
                damage: def.ProjectileDamage,
                remainingHits: def.ProjectileHits,
                canBeCopied: true);
        }
    }
}
