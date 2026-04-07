namespace _Project.Scripts.Gameplay.Simulation
{
    public sealed class GunState
    {
        public int Id { get; }
        public float PositionX { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileDamage { get; }
        public int ProjectileHits { get; }
        public float FireInterval { get; }
        public float CooldownRemaining { get; set; }
        public bool CanProjectileBeCopied { get; }

        public GunState(
            int id,
            float positionX,
            float projectileSpeed,
            float projectileDamage,
            int projectileHits,
            float fireInterval,
            bool canProjectileBeCopied = true,
            float initialCooldown = 0f)
        {
            Id = id;
            PositionX = positionX;
            ProjectileSpeed = projectileSpeed;
            ProjectileDamage = projectileDamage;
            ProjectileHits = projectileHits;
            FireInterval = fireInterval;
            CanProjectileBeCopied = canProjectileBeCopied;
            CooldownRemaining = initialCooldown;
        }
    }
}