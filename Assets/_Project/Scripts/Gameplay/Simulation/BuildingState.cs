using _Project.Scripts.Gameplay;
using _Project.Scripts.Gameplay.Data;

namespace _Project.Scripts.Gameplay.Simulation
{
    public class BuildingState
    {
        public int Id { get; }
        public float PositionX { get; }
        public BuildingType Type => Definition.Type;
        public BuildingDefinition Definition { get; }

        public BuildingState(int id, float positionX, BuildingDefinition definition)
        {
            Id = id;
            PositionX = positionX;
            Definition = definition;
        }
    }
}
