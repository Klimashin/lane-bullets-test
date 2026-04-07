using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "PierceBoostDefinition", menuName = "Gameplay/Pierce Boost Definition")]
    public sealed class PierceBoostBuildingDefinition : BuildingDefinition
    {
        public override BuildingType Type => BuildingType.PierceBoost;
        public int HitsBonus = 1;

        public override BuildingState CreateState(int id, float positionX)
        {
            return new BuildingState(id, positionX, this);
        }
    }
}
