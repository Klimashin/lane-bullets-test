using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "GunDefinition", menuName = "Gameplay/Gun Definition")]
    public sealed class GunBuildingDefinition : BuildingDefinition
    {
        public override BuildingType Type => BuildingType.Gun;

        public float ProjectileSpeed = 5f;
        public float ProjectileDamage = 10f;
        public int ProjectileHits = 1;
        public float FireInterval = 1f;
        public bool CanProjectileBeCopied = true;
        public float InitialCooldown = 0f;
    }
}
