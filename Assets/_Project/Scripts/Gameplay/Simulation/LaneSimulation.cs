using System;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class LaneSimulator
    {
        private int _nextProjectileId = 1;
        private float _laneMaxX;

        public LaneSimulator(float laneMaxX)
        {
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

            foreach (var building in lane.Buildings)
            {
                if (prevX < building.PositionX && building.PositionX <= nextX)
                {
                    events.Add(new LaneEvent(building));
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
            ModifierBuildingState? building,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            if (building == null)
            {
                return;
            }
            
            result.TriggeredBuildings.Add(new BuildingTriggerInfo(projectile.Id, building.Id));

            switch (building.Type)
            {
                case ModifierBuildingType.DamageBoost:
                    projectile.Damage *= 1.5f;
                    break;

                case ModifierBuildingType.PierceBoost:
                    projectile.RemainingHits += 1;
                    break;

                case ModifierBuildingType.Copy:
                    if (!projectile.CanBeCopied)
                    {
                        return;
                    }

                    var copy = CreateCopy(projectile, building.PositionX);
                    pendingProjectiles.Add(copy);
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

        private ProjectileState CreateCopy(ProjectileState source, float spawnX)
        {
            source.CanBeCopied = false;

            return new ProjectileState(
                id: _nextProjectileId++,
                positionX: spawnX,
                speed: source.Speed,
                damage: source.Damage * 0.5f,
                remainingHits: source.RemainingHits,
                canBeCopied: false);
        }
        
        private void SimulateGuns(
            LaneState lane,
            float deltaTime,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            foreach (var gun in lane.Guns)
            {
                SimulateGun(gun, deltaTime, pendingProjectiles, result);
            }
        }

        private void SimulateGun(
            GunState gun,
            float deltaTime,
            List<ProjectileState> pendingProjectiles,
            SimulationStepResult result)
        {
            gun.CooldownRemaining -= deltaTime;

            while (gun.CooldownRemaining <= 0f)
            {
                var projectile = CreateProjectileFromGun(gun);
                pendingProjectiles.Add(projectile);

                result.SpawnedProjectiles.Add(new ProjectileSpawnInfo(projectile.Id, projectile.PositionX));
                result.GunFires.Add(new GunFireInfo(gun.Id, projectile.Id));

                gun.CooldownRemaining += gun.FireInterval;
            }
        }

        private ProjectileState CreateProjectileFromGun(GunState gun)
        {
            return new ProjectileState(
                id: _nextProjectileId++,
                positionX: gun.PositionX,
                speed: gun.ProjectileSpeed,
                damage: gun.ProjectileDamage,
                remainingHits: gun.ProjectileHits,
                canBeCopied: gun.CanProjectileBeCopied);
        }
    }
}