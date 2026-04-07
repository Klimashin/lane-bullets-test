namespace _Project.Scripts.Gameplay.Simulation
{
    public enum LaneEventType
    {
        Building,
        Target
    }

    public readonly struct LaneEvent
    {
        public LaneEventType Type { get; }
        public float PositionX { get; }
        public ModifierBuildingState? Building { get; }
        public TargetState? Target { get; }

        public LaneEvent(ModifierBuildingState building)
        {
            Type = LaneEventType.Building;
            PositionX = building.PositionX;
            Building = building;
            Target = null;
        }

        public LaneEvent(TargetState target)
        {
            Type = LaneEventType.Target;
            PositionX = target.PositionX;
            Target = target;
            Building = null;
        }
    }
}