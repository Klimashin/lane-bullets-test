using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class TargetView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null!;
        [SerializeField] private TMP_Text _hpLabel = null!;

        public int TargetId { get; private set; }

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

        public void SetAlive(bool isAlive)
        {
            gameObject.SetActive(isAlive);
        }
    }
}