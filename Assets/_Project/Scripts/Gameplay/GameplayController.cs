using System.Collections.Generic;
using _Project.Scripts.Gameplay.Simulation;
using _Project.Scripts.Gameplay.View;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayController : MonoBehaviour
    {
        [SerializeField] private LaneView _laneViewPrefab = null!;

        [SerializeField] private int _lanesCount = 1;
        [SerializeField] private float _laneBaseY = 0f;
        [SerializeField] private float _laneSpacing = 3f;

        [SerializeField] private int _buildSlotCount = 5;
        [SerializeField] private float _laneStartX = 2f;
        [SerializeField] private float _buildSlotSpacing = 3f;
        [SerializeField] private float _laneBuildingSlotsOffsetX = 0.5f;

        [SerializeField] private List<float> _targetHealthValues = new();
        [SerializeField] private float _targetsStartX = 10f;
        [SerializeField] private float _targetsSpacing = 2f;

        [SerializeField] private float _simulationTick = 0.02f;
        [SerializeField] private float _laneLength = 20f;

        private readonly List<LaneState> _laneStates = new();
        private readonly List<LaneView> _laneViews = new();
        private LaneSimulator? _simulator;

        private float _tickAccumulator;
        private int _simulationSpeed = 1;

        private void Start()
        {
            _simulator = new LaneSimulator(_laneStartX, _laneStartX + _laneLength);

            for (int i = 0; i < _lanesCount; i++)
            {
                float worldY = _laneBaseY + i * _laneSpacing;
                var laneState = CreateLane();
                _laneStates.Add(laneState);

                var laneView = Instantiate(_laneViewPrefab, transform);
                laneView.Initialize(laneState, i, worldY);
                laneView.BuildingRemoveRequested += OnBuildingRemoveRequested;
                _laneViews.Add(laneView);
            }
        }

        private void Update()
        {
            if (_simulator == null)
            {
                return;
            }

            _tickAccumulator += Time.deltaTime * _simulationSpeed;

            while (_tickAccumulator >= _simulationTick)
            {
                _tickAccumulator -= _simulationTick;

                for (int i = 0; i < _laneStates.Count; i++)
                {
                    var stepResult = _simulator.Step(_laneStates[i], _simulationTick);
                    _laneViews[i].ApplyStepVisuals(stepResult);
                }
            }

            for (int i = 0; i < _laneStates.Count; i++)
            {
                _laneViews[i].SyncViews(_laneStates[i]);
            }
        }

        public void SetSimulationSpeed(int speed)
        {
            _simulationSpeed = Mathf.Max(0, speed);
        }

        public void TriggerAllGuns()
        {
            foreach (var laneState in _laneStates)
            {
                foreach (var slot in laneState.BuildSlots)
                {
                    if (slot.Building is GunBuildingState gun)
                    {
                        gun.PendingFire = true;
                    }
                }
            }
        }

        public bool TryPlaceBuilding(int laneId, int slotId, BuildingDefinition definition)
        {
            if (laneId < 0 || laneId >= _laneStates.Count)
            {
                return false;
            }

            var laneState = _laneStates[laneId];

            if (slotId < 0 || slotId >= laneState.BuildSlots.Count)
            {
                return false;
            }

            var slot = laneState.BuildSlots[slotId];

            if (slot.IsOccupied)
            {
                return false;
            }

            var building = definition.CreateState(slotId, slot.PositionX);
            slot.PlaceBuilding(building);
            _laneViews[laneId].SpawnBuildingView(building, definition);
            _laneViews[laneId].SetSlotOccupied(slotId, true);

            return true;
        }

        private void OnBuildingRemoveRequested(int laneId, int slotId)
        {
            if (laneId < 0 || laneId >= _laneStates.Count)
            {
                return;
            }

            _laneStates[laneId].BuildSlots[slotId].RemoveBuilding();
        }

        private LaneState CreateLane()
        {
            var lane = new LaneState();

            for (int i = 0; i < _buildSlotCount; i++)
            {
                float posX = _laneStartX + _laneBuildingSlotsOffsetX + i * _buildSlotSpacing;
                lane.BuildSlots.Add(new BuildSlotState(i, posX));
            }

            for (int i = 0; i < _targetHealthValues.Count; i++)
            {
                float posX = _targetsStartX + i * _targetsSpacing;
                lane.Targets.Add(new TargetState(i, posX, _targetHealthValues[i]));
            }

            return lane;
        }
    }

}
