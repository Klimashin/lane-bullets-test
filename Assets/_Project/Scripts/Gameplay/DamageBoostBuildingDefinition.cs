using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "DamageBoostDefinition", menuName = "Gameplay/Damage Boost Definition")]
    public sealed class DamageBoostBuildingDefinition : BuildingDefinition
    {
        public override BuildingType Type => BuildingType.DamageBoost;
        public float DamageMultiplier = 1.5f;
    }
}
