namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class TargetState
    {
        public int Id { get; }
        public float PositionX { get; }
        public float Health { get; private set; }

        public bool IsDestroyed => Health <= 0f;

        public TargetState(int id, float positionX, float health)
        {
            Id = id;
            PositionX = positionX;
            Health = health;
        }

        public void ApplyDamage(float damage)
        {
            if (IsDestroyed)
            {
                return;
            }

            Health -= damage;
        }
    }
}