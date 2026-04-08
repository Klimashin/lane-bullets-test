using _Project.Scripts.Gameplay.Data;
using _Project.Scripts.Gameplay.Simulation;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class BuildingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statsText = null!;

        public int BuildingId { get; private set; }

        public void Initialize(int buildingId, BuildingDefinition definition)
        {
            BuildingId = buildingId;
            _statsText.text = definition.GetStatsText();
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void PlayFeedback()
        {
            // implement punch scale
        }
    }
}