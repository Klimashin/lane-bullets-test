using _Project.Scripts.Gameplay;

namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class GunBuildingState : BuildingState
    {
        public new GunBuildingDefinition Definition { get; }
        public float CooldownRemaining { get; set; }
        public bool PendingFire { get; set; }

        public GunBuildingState(int id, float positionX, GunBuildingDefinition definition)
            : base(id, positionX, definition)
        {
            Definition = definition;
            CooldownRemaining = definition.InitialCooldown;
        }
    }
}
