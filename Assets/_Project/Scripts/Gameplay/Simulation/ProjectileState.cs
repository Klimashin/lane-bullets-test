namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class ProjectileState
    {
        public int Id { get; }
        public float PositionX { get; set; }
        public float Speed { get; }
        public float Damage { get; set; }
        public int RemainingHits { get; set; }
        public bool CanBeCopied { get; set; }
        public bool IsDestroyed { get; private set; }

        public ProjectileState(
            int id,
            float positionX,
            float speed,
            float damage,
            int remainingHits,
            bool canBeCopied)
        {
            Id = id;
            PositionX = positionX;
            Speed = speed;
            Damage = damage;
            RemainingHits = remainingHits;
            CanBeCopied = canBeCopied;
        }

        public void Destroy()
        {
            IsDestroyed = true;
        }
    }
}