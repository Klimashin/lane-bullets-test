namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class ModifierBuildingState
    {
        public int Id { get; }
        public float PositionX { get; }
        public ModifierBuildingType Type { get; }

        public ModifierBuildingState(int id, float positionX, ModifierBuildingType type)
        {
            Id = id;
            PositionX = positionX;
            Type = type;
        }
    }
}