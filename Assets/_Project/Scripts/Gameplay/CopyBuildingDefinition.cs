using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [CreateAssetMenu(fileName = "CopyDefinition", menuName = "Gameplay/Copy Definition")]
    public sealed class CopyBuildingDefinition : BuildingDefinition
    {
        public override BuildingType Type => BuildingType.Copy;
        public float CopyDamageFraction = 0.5f;

        public override BuildingState CreateState(int id, float positionX)
        {
            return new BuildingState(id, positionX, this);
        }
    }
}
