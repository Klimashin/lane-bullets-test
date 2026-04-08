using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Data
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Gameplay/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Gameplay Settings")]
        [Range(1, 4)] public int LanesCount = 1;
        [Range(1, 9)] public int BuildSlotCount = 5;
        public List<float> TargetHealthValues = new();
        public List<BuildingDefinition> Buildings = new();

        [Header("Layout settings")]
        public float LaneBaseY = 0f;
        public float LaneSpacing = 3f;
        public float SimulationTick = 0.02f;
        public float TargetsStartX = 10f;
        public float TargetsSpacing = 2f;
        public float LaneStartX = 2f;
        public float LaneLength = 20f;
        public float BuildSlotSpacing = 3f;
        public float BuildingSlotsOffsetX = 0.5f;
    }
}
