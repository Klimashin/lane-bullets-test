using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Simulation
{
    [Serializable]
    public sealed class LaneSimulationConfig
    {
        public int LanesCount = 1;
        public float LaneStartX = 2f;
        public float LaneLength = 20f;
        public int BuildSlotCount = 5;
        public float BuildSlotSpacing = 3f;
        public float BuildingSlotsOffsetX = 0.5f;
        public List<float> TargetHealthValues = new();
        public float TargetsStartX = 10f;
        public float TargetsSpacing = 2f;
    }
}
