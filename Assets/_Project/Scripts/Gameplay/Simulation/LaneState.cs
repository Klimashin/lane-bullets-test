using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class LaneState
    {
        public List<GunState> Guns { get; } = new();
        public List<ProjectileState> Projectiles { get; } = new();
        public List<TargetState> Targets { get; } = new();
        public List<ModifierBuildingState> Buildings { get; } = new();

        public void RemoveDestroyedEntities()
        {
            Projectiles.RemoveAll(p => p.IsDestroyed);
            Targets.RemoveAll(t => t.IsDestroyed);
        }
        
        public void CleanupProjectilesOutsideRange(float maxX)
        {
            Projectiles.RemoveAll(p => p.PositionX > maxX);
        }
    }
}