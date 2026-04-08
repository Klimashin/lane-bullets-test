using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null!;
        [SerializeField] private TMP_Text _damageText = null!;

        public int ProjectileId { get; private set; }

        public void Initialize(int projectileId)
        {
            ProjectileId = projectileId;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void SetDamage(float damage)
        {
            _damageText.SetText("{0:1}", damage);
        }

        public void PlayHitFeedback()
        {
            transform.DOPunchScale(Vector3.one * 0.4f, 0.2f, vibrato: 1, elasticity: 0f);
        }
    }
}