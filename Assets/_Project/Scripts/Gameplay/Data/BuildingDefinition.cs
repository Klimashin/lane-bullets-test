using _Project.Scripts.Gameplay.Simulation;
using _Project.Scripts.Gameplay.View;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Data
{
    public abstract class BuildingDefinition : ScriptableObject
    {
        public BuildingView Prefab = null!;
        public GameObject UIPrefab = null!;
        public abstract BuildingType Type { get; }
        public abstract BuildingState CreateState(int id, float positionX);
        public virtual string GetStatsText() => string.Empty;
    }
}
