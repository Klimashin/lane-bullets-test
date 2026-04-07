using System;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class LaneSimulator
    {
        private int _nextProjectileId = 1;
        private float _laneStartX;
        private float _laneMaxX;

        public LaneSimulator(float laneStartX, float laneMaxX)
        {
            _laneStartX = laneStartX;
            _laneMaxX = laneMaxX;
        }

        public SimulationStepResult Step(LaneState lane, float deltaTime)
        {
            var result = new SimulationStepResult();
            var pendingProjectiles = new List<ProjectileState>();

            SimulateGuns(lane, deltaTime, pendingProjectiles, result);

            var activeProjectiles = lane.Projectiles.ToList();

            foreach (var projectile in activeProjectiles)
            {
                if (projectile.IsDestroyed)
                {
                    continue;
                }

                SimulateProjectile(projectile, lane, deltaTime, pendingProjectiles, result);
            }

            lane.Projectiles.AddRange(pendingProjectiles);
            lane.RemoveDestroyedEntities();
            lane.CleanupProjectilesOutsideRange(_laneMaxX);

            return result;
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
            var events = new List<LaneEvent>();

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

                if (prevX < target.PositionX && target.PositionX <= nextX)
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

            result.TriggeredBuildings.Add(new BuildingTriggerInfo(projectile.Id, building.Id));

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
            source.CanBeCopied = false;

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

        private ProjectileState CreateProjectileFromGun(GunBuildingState gun)
        {
            return new ProjectileState(
                id: _nextProjectileId++,
                positionX: gun.PositionX,
                speed: gun.Definition.ProjectileSpeed,
                damage: gun.Definition.ProjectileDamage,
                remainingHits: gun.Definition.ProjectileHits,
                canBeCopied: true);
        }
    }
}