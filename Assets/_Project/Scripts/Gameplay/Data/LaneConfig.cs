using System;
using System.Collections.Generic;

namespace _Project.Scripts.Gameplay.Data
{
    [Serializable]
    public sealed class LaneConfig
    {
        public int BuildSlotCount = 5;
        public List<float> TargetHealthValues = new();
    }
}
