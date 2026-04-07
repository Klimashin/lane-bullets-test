using UnityEngine;

namespace _Project.Scripts.Gameplay.View
{
    public sealed class GunView : MonoBehaviour
    {
        public int GunId { get; private set; }

        public void Initialize(int gunId)
        {
            GunId = gunId;
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void PlayFireFeedback()
        {
            // implement punch scale
        }
    }
}