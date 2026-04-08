using System.Collections.Generic;
using _Project.Scripts.Gameplay.Data;
using _Project.Scripts.Gameplay.Simulation;
using _Project.Scripts.Gameplay.View;
using Reflex.Attributes;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayController : MonoBehaviour
    {
        [SerializeField] private LaneView _laneViewPrefab = null!;

        public int SimulationSpeed { get; private set; } = 1;

        private LaneSimulator? _simulator;
        private readonly List<LaneView> _laneViews = new();
        private float _tickAccumulator;
        private GameplayConfig _config = null!;

        [Inject]
        public void Inject(GameplayConfig config)
        {
            _config = config;
        }

        private void Start()
        {
            var simulationConfig = new LaneSimulationConfig
            {
                LanesCount = _config.LanesCount,
                LaneStartX = _config.LaneStartX,
                LaneLength = _config.LaneLength,
                BuildSlotCount = _config.BuildSlotCount,
                BuildSlotSpacing = _config.BuildSlotSpacing,
                BuildingSlotsOffsetX = _config.BuildingSlotsOffsetX, 
                TargetHealthValues = _config.TargetHealthValues,
                TargetsStartX = _config.TargetsStartX,
                TargetsSpacing = _config.TargetsSpacing,
                TargetOffsetX = _config.TargetOffsetX
            };

            var laneBaseY = _config.LaneBaseY;
            var laneSpacing = _config.LaneSpacing;

            _simulator = new LaneSimulator(simulationConfig);

            for (int i = 0; i < _simulator.LaneStates.Count; i++)
            {
                float worldY = laneBaseY + i * laneSpacing;
                var laneView = Instantiate(_laneViewPrefab, transform);
                laneView.Initialize(_simulator.LaneStates[i], i, worldY);
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

            _tickAccumulator += Time.deltaTime * SimulationSpeed;

            while (_tickAccumulator >= _config.SimulationTick)
            {
                _tickAccumulator -= _config.SimulationTick;

                var results = _simulator.StepAll(_config.SimulationTick);

                for (int i = 0; i < results.Count; i++)
                {
                    _laneViews[i].ApplyStepVisuals(results[i]);
                }
            }

            for (int i = 0; i < _laneViews.Count; i++)
            {
                _laneViews[i].SyncViews(_simulator.LaneStates[i]);
            }
        }

        public void SetSimulationSpeed(int speed)
        {
            SimulationSpeed = Mathf.Clamp(speed, 0, 10);
        }

        public void TriggerAllGuns()
        {
            _simulator?.TriggerAllGuns();
        }

        public GunsControlMode GunsControlMode => _simulator?.Mode ?? GunsControlMode.Manual;

        public void SetGunsControlMode(GunsControlMode mode)
        {
            if (_simulator != null)
            {
                _simulator.Mode = mode;
            }
        }

        public bool TryPlaceBuilding(int laneId, int slotId, BuildingDefinition definition)
        {
            if (_simulator == null)
            {
                return false;
            }

            var building = _simulator.TryPlaceBuilding(laneId, slotId, definition);

            if (building == null)
            {
                return false;
            }

            _laneViews[laneId].SpawnBuildingView(building, definition);
            _laneViews[laneId].SetSlotOccupied(slotId, true);

            return true;
        }

        private void OnBuildingRemoveRequested(int laneId, int slotId)
        {
            _simulator?.RemoveBuilding(laneId, slotId);
        }
    }
}
