using _Project.Scripts.Gameplay;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class BuildingState
    {
        public int Id { get; }
        public float PositionX { get; }
        public BuildingType Type { get; }
        public BuildingDefinition Definition { get; }

        // Gun-only fields
        public float ProjectileSpeed { get; }
        public float ProjectileDamage { get; }
        public int ProjectileHits { get; }
        public float FireInterval { get; }
        public float CooldownRemaining { get; set; }
        public bool CanProjectileBeCopied { get; }

        public BuildingState(
            int id,
            float positionX,
            GunBuildingDefinition definition,
            float initialCooldown = 0f)
        {
            Id = id;
            PositionX = positionX;
            Type = BuildingType.Gun;
            Definition = definition;
            ProjectileSpeed = definition.ProjectileSpeed;
            ProjectileDamage = definition.ProjectileDamage;
            ProjectileHits = definition.ProjectileHits;
            FireInterval = definition.FireInterval;
            CanProjectileBeCopied = definition.CanProjectileBeCopied;
            CooldownRemaining = initialCooldown;
        }

        public BuildingState(int id, float positionX, BuildingDefinition definition)
        {
            Id = id;
            PositionX = positionX;
            Type = definition.Type;
            Definition = definition;
        }
    }
}
