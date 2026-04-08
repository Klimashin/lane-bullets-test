using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _damageText = null!;

        public int ProjectileId { get; private set; }
        
        private Tween? _feedbackTween;

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

        public void PlayFeedback()
        {
            _feedbackTween = transform.DOPunchScale(Vector3.one * 0.4f, 0.2f, vibrato: 1, elasticity: 0f);
        }

        private void OnDestroy()
        {
            if (_feedbackTween != null && _feedbackTween.IsActive())
            {
                _feedbackTween.Kill();
            }
        }
    }
}