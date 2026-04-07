using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null!;

        public int ProjectileId { get; private set; }

        public void Initialize(int projectileId)
        {
            ProjectileId = projectileId;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }
    }
}