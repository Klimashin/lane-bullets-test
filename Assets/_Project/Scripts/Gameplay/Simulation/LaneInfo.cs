using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class SimulationStepResult
    {
        public List<ProjectileSpawnInfo> SpawnedProjectiles { get; } = new();
        public List<TargetHitInfo> TargetHits { get; } = new();
        public List<BuildingTriggerInfo> TriggeredBuildings { get; } = new();
        public List<GunFireInfo> GunFires { get; } = new();

        public void Clear()
        {
            SpawnedProjectiles.Clear();
            TargetHits.Clear();
            TriggeredBuildings.Clear();
            GunFires.Clear();
        }
    }

    public readonly struct ProjectileSpawnInfo
    {
        public int ProjectileId { get; }
        public float PositionX { get; }

        public ProjectileSpawnInfo(int projectileId, float positionX)
        {
            ProjectileId = projectileId;
            PositionX = positionX;
        }
    }

    public readonly struct TargetHitInfo
    {
        public int ProjectileId { get; }
        public int TargetId { get; }
        public float Damage { get; }

        public TargetHitInfo(int projectileId, int targetId, float damage)
        {
            ProjectileId = projectileId;
            TargetId = targetId;
            Damage = damage;
        }
    }

    public readonly struct BuildingTriggerInfo
    {
        public int ProjectileId { get; }
        public int BuildingId { get; }

        public BuildingTriggerInfo(int projectileId, int buildingId)
        {
            ProjectileId = projectileId;
            BuildingId = buildingId;
        }
    }
    
    public readonly struct GunFireInfo
    {
        public int SlotId { get; }
        public int ProjectileId { get; }

        public GunFireInfo(int slotId, int projectileId)
        {
            SlotId = slotId;
            ProjectileId = projectileId;
        }
    }
}