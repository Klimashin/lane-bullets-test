using _Project.Scripts.Gameplay.Simulation;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class BuildingView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null!;

        public int BuildingId { get; private set; }

        public void Initialize(int buildingId, ModifierBuildingType type)
        {
            BuildingId = buildingId;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }
    }
}