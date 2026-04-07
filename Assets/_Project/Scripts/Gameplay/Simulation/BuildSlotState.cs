using System;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class BuildSlotState
    {
        public int Id { get; }
        public float PositionX { get; }
        public BuildingState? Building { get; private set; }

        public bool IsOccupied => Building != null;

        public BuildSlotState(int id, float positionX)
        {
            Id = id;
            PositionX = positionX;
        }

        public void PlaceBuilding(BuildingState building)
        {
            if (IsOccupied)
            {
                throw new InvalidOperationException($"Slot {Id} is already occupied.");
            }

            Building = building;
        }

        public BuildingState RemoveBuilding()
        {
            if (Building == null)
            {
                throw new InvalidOperationException($"Slot {Id} has no building.");
            }

            var building = Building;
            Building = null;
            return building;
        }
    }
}