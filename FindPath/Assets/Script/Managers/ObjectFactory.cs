using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FindPath
{
    public class ObjectFactory
    {
        [Inject] private readonly IObjectResolver _objectResolver;
        [Inject] private readonly AssetManager _assetManager;

        public async UniTask<GameObject> LoadGameObjectAsync(string name, Transform parent = null)
        {
            var obj = await _assetManager.LoadGameObjectAsync(name, parent);
            _objectResolver.InjectGameObject(obj);
            return obj;
        }

        public GameObject LoadGameObject(string name, Transform parent = null)
        {
            var obj = _assetManager.LoadGameObject(name, parent);
            _objectResolver.InjectGameObject(obj);
            return obj;
        }

        public T LoadResource<T>(string path, string name) where T : Object
        {
            return Resources.Load<T>(path + name);
        }
    }
}