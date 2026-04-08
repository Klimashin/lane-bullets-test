using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Data
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Gameplay/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Gameplay Settings")]
        public List<LaneConfig> Lanes = new();
        public List<BuildingDefinition> Buildings = new();

        [Header("Layout Settings")]
        public float LaneBaseY = 0f;
        public float LaneSpacing = 3f;
        public float SimulationTick = 0.02f;
        public float LaneStartX = 2f;
        public float LaneLength = 20f;
        public float BuildSlotSpacing = 3f;
        public float BuildingSlotsOffsetX = 0.5f;
        public float TargetsSpacing = 2f;
        public float FirstTargetOffsetFromBuilding = 2f;
        public float TargetOffsetX = 0f;
    }
}
