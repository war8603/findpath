using System.Collections.Generic;
using FindPath;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Managers
{
    public interface IBaseManager
    {
        public void InitManager();
    }
    
    public class MainFrameWork : LifetimeScope
    {
        private readonly List<IBaseManager> _managers = new();
        private GameManager _gameManager;

        private void Start()
        {
            Init();
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Init()
        {
            _managers.Add(_gameManager = CreateGameObject<GameManager>());
            
            _managers.Add(CreateGameObject<BattleManager>());
            _managers.Add(CreateGameObject<OutGameManager>());
            
            _managers.Add(Create<DataManager>());
            _managers.Add(Create<InventoryManager>());
            _managers.Add(Create<AssetManager>());
            _managers.Add(Create<AdsManager>());
            _managers.Add(Create<IAPManager>());
            
            _managers.Add(CreateGameObject<UIManager>());
            _managers.Add(CreateGameObject<SkillManager>());
            _managers.Add(CreateGameObject<CustomObjectPool>());
            _managers.Add(CreateGameObject<SoundManager>());

            Create<ObjectFactory>();

            Build();
            
            _managers.ForEach(x => x.InitManager());
            
            _gameManager.StartOutGame();
        }

        private T Create<T>() where T : class, new()
        {
            var newClass = new T();
            RegisterClass(newClass);
            return newClass;
        }

        private T CreateGameObject<T>() where T : MonoBehaviour, new()
        {
            var obj = new GameObject(typeof(T).Name);
            var newClass = obj.AddComponent<T>();
            RegisterClass(newClass);
            return newClass;
        }

        private void RegisterClass<T>(T instance)
        {
            Enqueue(builder =>
            {
                builder.RegisterInstance(instance);
                builder.RegisterBuildCallback(container => container.Inject(instance));
            });
        }
    }
}