using _Project.Scripts.Gameplay.Data;
using Reflex.Core;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private Camera _camera = null!;
        [SerializeField] private GameplayController _gameplayController = null!;
        [SerializeField] private GameplayConfig _gameplayConfig = null!;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_camera);
            containerBuilder.RegisterValue(_gameplayController);
            containerBuilder.RegisterValue(_gameplayConfig);
        }
    }
}
