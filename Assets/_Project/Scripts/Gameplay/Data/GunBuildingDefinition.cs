using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Data
{
    [CreateAssetMenu(fileName = "GunDefinition", menuName = "Gameplay/Gun Definition")]
    public sealed class GunBuildingDefinition : BuildingDefinition
    {
        public override BuildingType Type => BuildingType.Gun;

        public float ProjectileSpeed = 5f;
        public float ProjectileDamage = 10f;
        public int ProjectileHits = 1;
        public float FireInterval = 1f;
        public float InitialCooldown = 1f;
        public float SpawnOffsetX = 0f;

        public override BuildingState CreateState(int id, float positionX)
        {
            return new GunBuildingState(id, positionX, this);
        }

        public override string GetStatsText() => $"DMG: {ProjectileDamage}";
    }
}
