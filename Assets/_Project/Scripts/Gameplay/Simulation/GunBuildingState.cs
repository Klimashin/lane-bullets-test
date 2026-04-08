using _Project.Scripts.Gameplay.Data;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class GunBuildingState : BuildingState
    {
        public float CooldownRemaining { get; set; }
        public bool PendingFire { get; set; }

        public GunBuildingState(int id, float positionX, GunBuildingDefinition definition)
            : base(id, positionX, definition)
        {
            CooldownRemaining = definition.InitialCooldown;
        }
    }
}
