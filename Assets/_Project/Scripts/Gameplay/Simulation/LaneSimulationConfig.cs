using System;

namespace _Project.Scripts.Gameplay.Simulation
{
    [Serializable]
    public sealed class LaneSimulationConfig
    {
        public float LaneStartX = 2f;
        public float LaneLength = 20f;
        public float BuildSlotSpacing = 3f;
        public float BuildingSlotsOffsetX = 0.5f;
        public float TargetsSpacing = 2f;
        public float FirstTargetOffsetFromBuilding = 2f;
        public float TargetOffsetX = 0f;
    }
}
