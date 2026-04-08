using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class TargetView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null!;
        [SerializeField] private TMP_Text _hpLabel = null!;

        public int TargetId { get; private set; }

        private Tween? _hitTween;

        public void Initialize(int targetId)
        {
            TargetId = targetId;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }
        
        public void SetHealth(float currentHealth)
        {
            _hpLabel.text = Mathf.Max(0f, currentHealth).ToString("0.##");
        }

        public void PlayHitFeedback()
        {
            if (_hitTween != null && _hitTween.IsActive())
            {
                _hitTween.Kill();
            }

            _hitTween = transform.DOPunchScale(Vector3.one * 0.4f, 0.2f, vibrato: 1, elasticity: 0f);
        }

        private void OnDestroy()
        {
            if (_hitTween != null && _hitTween.IsActive())
            {
                _hitTween.Kill();
            }
        }
    }
}